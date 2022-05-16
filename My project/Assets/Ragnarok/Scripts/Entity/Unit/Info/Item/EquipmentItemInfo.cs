using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class EquipmentItemInfo : ItemInfo
    {
        private const int SHADOW_MAX_LEVEL_FACTOR = 10; // 쉐도우 장비 최대 강화레벨 계산 상수

        private readonly IInventoryModel inventoryModel; // 인벤토리 모델
        private readonly SmeltCoefficientDataManager smeltCoefficientDataRepo;
        private readonly BotCoefficientDataManager botCoefficientDataRepo;
        private readonly EquipItemLevelupDataManager equipItemLevelupDataRepo;

        ObscuredInt itemLevel;
        ObscuredByte itemPos;
        ObscuredLong equippedCardNo1, equippedCardNo2, equippedCardNo3, equippedCardNo4;
        ObscuredBool isLock;
        ObscuredInt itemTranscend;
        ObscuredInt itemChangedElement;
        ObscuredInt itemElementLevel;
        ObscuredInt cardSlotCount;

        public override bool IsStackable => false;
        public override bool IsEquipped => EquippedSlotType != ItemEquipmentSlotType.None;
        public override ItemEquipmentSlotType SlotType => data.GetEquipmentSlotType();
        public override ItemEquipmentSlotType EquippedSlotType
        {
            get => itemPos.ToEnum<ItemEquipmentSlotType>();
            set => itemPos = value.ToByteValue();
        }
        public override ElementType ElementType => GetElementType();
        public override EquipmentClassType ClassType => data.class_type.ToEnum<EquipmentClassType>();
        public override int Rating => data.rating;
        public override string PrefabName => data.prefab_name;
        public override int Tier => GetTier();
        public override int Smelt => itemLevel;
        public override int MaxSmelt => GetMaxSmelt();
        public override bool IsCardEnchanted => GetIsCardEnchanted();
        public override bool IsLock => isLock;
        public override int ItemTranscend => itemTranscend;
        public override int ItemChangedElement => itemChangedElement;
        public override bool IsEquippedCard => GetIsEquippedCard();
        public override int ElementLevel => GetElementLevel();
        public long EquippedCardNo1 => equippedCardNo1;
        public long EquippedCardNo2 => equippedCardNo2;
        public long EquippedCardNo3 => equippedCardNo3;
        public long EquippedCardNo4 => equippedCardNo4;
        public override bool IsShadow => data == null ? false : ItemDetailType == ItemDetailType.Shadow;
        public override int CardSlotCount => GetCardSlotCount();
        public override ItemDetailType ItemDetailType => data.duration.ToEnum<ItemDetailType>();

        public EquipmentItemInfo(IInventoryModel inventoryModel = null)
        {
            this.inventoryModel = inventoryModel; // 장착중인 카드 정보 가져오는데 사용
            smeltCoefficientDataRepo = SmeltCoefficientDataManager.Instance;
            botCoefficientDataRepo = BotCoefficientDataManager.Instance;
            equipItemLevelupDataRepo = EquipItemLevelupDataManager.Instance;
        }

        public override void SetItemInfo(int tier, int itemLevel, byte itemPos, long equippedCardNo1, long equippedCardNo2, long equippedCardNo3, long equippedCardNo4, bool isLock, int itemTranscend = 0, int itemChangedElement = 0, int itemElementLevel = 0)
        {
            Reload(isEquipCard: false); // 기존 카드 장착 플래그 변경 (장착해제)

            this.itemLevel = itemLevel;
            this.itemPos = itemPos;
            this.equippedCardNo1 = equippedCardNo1;
            this.equippedCardNo2 = equippedCardNo2;
            this.equippedCardNo3 = equippedCardNo3;
            this.equippedCardNo4 = equippedCardNo4;
            this.isLock = isLock;
            this.itemTranscend = itemTranscend;
            this.itemChangedElement = itemChangedElement;
            this.itemElementLevel = itemElementLevel;
            cardSlotCount = tier; // 쉐도우 장비 오픈된 카드 슬롯 개수
        }

        public override void Reload(bool isEquipCard)
        {
            if (isEquipCard)
            {
                SetCardEquip(isEquipped: true); // 현재 카드 장착 플래그 변경 (장착)
            }
            else
            {
                SetCardEquip(isEquipped: false); // 현재 카드 장착 플래그 변경 (장착)
            }
        }

        public void ForceSetItemTranscend(int itemTranscend)
        {
            this.itemTranscend = itemTranscend;
        }

        public void ForceSetChangedElement(int itemChangedElement, int itemElementLevel)
        {
            this.itemChangedElement = itemChangedElement;
            this.itemElementLevel = itemElementLevel;
        }

        /// <summary>
        /// 해당 장비의 전투력 반환. 
        /// </summary>
        public int GetAttackPower()
        {
            int virtualAP = Entity.player.attackPowerInfo.GetVirtualAttackPower(this);
            int curAP = Entity.player.SavedBattleStatusData.AP;
            int diff = virtualAP - curAP; // 이 장비를 장착했을 때 상승하는 전투력
            return diff;
        }

        protected override long GetEquippedCardNo(int index)
        {
            switch (index)
            {
                case 0: return equippedCardNo1;
                case 1: return equippedCardNo2;
                case 2: return equippedCardNo3;
                case 3: return equippedCardNo4;
            }

            throw new System.ArgumentException($"[올바르지 않은 {nameof(index)}] {nameof(index)} = {index}");
        }

        public int GetFirstEquippedCardIndex()
        {
            if (equippedCardNo1 != 0)
                return 0;
            if (equippedCardNo2 != 0)
                return 1;
            if (equippedCardNo3 != 0)
                return 2;
            if (equippedCardNo4 != 0)
                return 3;
            return -1;
        }

        public override ItemInfo GetCardItem(int index)
        {
            if (inventoryModel == null)
                return null;

            return inventoryModel.GetItemInfo(GetEquippedCardNo(index));
        }

        /// <summary>
        /// [장비] 카드 슬롯 해금 여부
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public override bool IsOpenCardSlot(int index)
        {
            //Debug.Log($"{Smelt}={GetCardSlotOpenLevel(index)}");
            // 카드 슬롯 개방 조건 체크
            return Smelt >= GetCardSlotOpenLevel(index);
        }

        /// <summary>
        /// [장비] 카드 슬롯 해금 레벨
        /// </summary>
        public override int GetCardSlotOpenLevel(int index)
        {
            return BasisType.EQUIPMENT_CARD_SLOT_UNLOCK_LEVEL.GetInt(index + 1);
        }

        public override bool CanEquipCard(EquipmentClassType cardClassType)
        {
            return cardClassType.HasFlag(ClassType);
        }

        protected override Status GetStatus(int tier)
        {
            float tierPer = MathUtils.ToPercentValue(tier);

            int atk = MathUtils.ToInt(data.atk_min * (1f + tierPer) * (1f + 0.5f * ItemTranscend));
            int matk = MathUtils.ToInt(data.matk_min * (1f + tierPer) * (1f + 0.5f * ItemTranscend));
            int def = MathUtils.ToInt(data.def_min * (1f + tierPer) * (1f + 0.5f * ItemTranscend));
            int mdef = MathUtils.ToInt(data.mdef_min * (1f + tierPer) * (1f + 0.5f * ItemTranscend));

            return new Status(atk, matk, def, mdef); // 아이템 스탯
        }

        public override IEnumerable<BattleOption> GetBattleOptionCollection(int smelt)
        {
            BattleOption option1 = new BattleOption(data.battle_option_type_1, data.value1_b1, data.value2_b1);
            BattleOption option2 = new BattleOption(data.battle_option_type_2, data.value1_b2, data.value2_b2);
            BattleOption option3 = new BattleOption(data.battle_option_type_3, data.value1_b3, data.value2_b3);
            BattleOption option4 = new BattleOption(data.battle_option_type_4, data.value1_b4, data.value2_b4);

            if (option1.battleOptionType != BattleOptionType.None)
                yield return GetBattleOption(option1, smelt);

            if (option2.battleOptionType != BattleOptionType.None)
                yield return GetBattleOption(option2, smelt);

            if (option3.battleOptionType != BattleOptionType.None)
                yield return GetBattleOption(option3, smelt);

            if (option4.battleOptionType != BattleOptionType.None)
                yield return GetBattleOption(option4, smelt);
        }

        public override string GetBackSpriteName(bool isInventory, bool supportTranscend = false)
        {
            // 쉐도우 장비는 단일 색상
            if (IsShadow)
                return isInventory ? Constants.CommonAtlas.UI_COMMON_BG_ITEM_TYPE2_SHADOW : Constants.CommonAtlas.UI_COMMON_BG_ITEM_SHADOW;

            if (ItemTranscend > 0 && supportTranscend)
                return isInventory ? Constants.CommonAtlas.UI_COMMON_BG_ITEM_TYPE2_06 : Constants.CommonAtlas.UI_COMMON_BG_ITEM_06;

            if (isInventory)
            {
                return GetBackSpriteName();
            }

            // 장비 등급에 따른 배경색 변경
            switch (Rating)
            {
                case 1: return Constants.CommonAtlas.UI_COMMON_BG_ITEM_01;
                case 2: return Constants.CommonAtlas.UI_COMMON_BG_ITEM_02;
                case 3: return Constants.CommonAtlas.UI_COMMON_BG_ITEM_03;
                case 4: return Constants.CommonAtlas.UI_COMMON_BG_ITEM_04;
                case 5: return Constants.CommonAtlas.UI_COMMON_BG_ITEM_05;
            }
            return default;
        }

        private string GetBackSpriteName()
        {
            switch (Rating)
            {
                case 1: return Constants.CommonAtlas.UI_COMMON_BG_ITEM_TYPE2_01;
                case 2: return Constants.CommonAtlas.UI_COMMON_BG_ITEM_TYPE2_02;
                case 3: return Constants.CommonAtlas.UI_COMMON_BG_ITEM_TYPE2_03;
                case 4: return Constants.CommonAtlas.UI_COMMON_BG_ITEM_TYPE2_04;
                case 5: return Constants.CommonAtlas.UI_COMMON_BG_ITEM_TYPE2_05;
            }
            return default;
        }

        public override Color GetLockBackColor(bool supportTranscend = false)
        {
            if (IsShadow)
                return new Color32(146, 134, 219, 255);

            if (ItemTranscend > 0 && supportTranscend)
                return new Color32(199, 177, 235, 255);

            switch (Rating)
            {
                case 1: return new Color32(179, 186, 191, 255);
                case 2: return new Color32(194, 220, 98, 255);
                case 3: return new Color32(142, 213, 239, 255);
                case 4: return new Color32(213, 183, 219, 255);
                case 5: return new Color32(239, 179, 109, 255);
            }
            return default;
        }

        /// <summary>
        /// 최대 재련도
        /// </summary>
        private int GetMaxSmelt()
        {
            if (IsShadow)
                return BasisType.SHADOW_MAX_LEVEL.GetInt(Rating);

            return BasisType.PARTS_ITEM_MAX_SMELT.GetInt();
        }

        public override int GetMaxCardSlot()
        {
            if(IsShadow)
            {
                int maxLevel = BasisType.SHADOW_MAX_LEVEL.GetInt(Rating);
                return Mathf.Min(Constants.Size.MAX_EQUIPPED_CARD_COUNT, maxLevel / SHADOW_MAX_LEVEL_FACTOR); // 1~4
            }
            return Constants.Size.MAX_EQUIPPED_CARD_COUNT;
        }

        /// <summary>
        /// 카드 장착 플래그 변경
        /// </summary>
        private void SetCardEquip(bool isEquipped)
        {
            for (int i = 0; i < GetMaxCardSlot(); i++)
            {
                ItemInfo cardItem = GetCardItem(i);
                if (cardItem == null)
                    continue;

                cardItem.SetEquipped(isEquipped);
            }
        }

        private bool GetIsCardEnchanted()
        {
            for (int i = 0; i < GetMaxCardSlot(); i++)
            {
                ItemInfo cardItem = GetCardItem(i);
                if (cardItem != null)
                    return true;
            }
            return false;
        }

        private Status GetStatus(Status status, SmeltCoefficientDataManager.PlusStatus plusStatus, float ratingFactor)
        {
            int atk = status.atk == 0 ? 0 : MathUtils.ToInt(status.atk * Mathf.Pow(MathUtils.ToPermyriadValue(plusStatus.atk) + 1, ratingFactor));
            int matk = status.matk == 0 ? 0 : MathUtils.ToInt(status.matk * Mathf.Pow(MathUtils.ToPermyriadValue(plusStatus.matk) + 1, ratingFactor));
            int def = status.def == 0 ? 0 : MathUtils.ToInt(status.def * Mathf.Pow(MathUtils.ToPermyriadValue(plusStatus.def) + 1, ratingFactor));
            int mdef = status.mdef == 0 ? 0 : MathUtils.ToInt(status.mdef * Mathf.Pow(MathUtils.ToPermyriadValue(plusStatus.mdef) + 1, ratingFactor));
            return new Status(atk, matk, def, mdef);
        }

        private BattleOption GetBattleOption(BattleOption option, int smelt)
        {
            if (smelt == 0)
                return option;

            BattleOptionType battleOptionType = option.battleOptionType;
            BotCoefficientDataManager.PlusStatus plusStatus = botCoefficientDataRepo.Get(battleOptionType, smelt);
            int value1 = battleOptionType.IsConditionalSkill() ? option.value1 : GetBattleOptionValue(option.value1, plusStatus.value1);
            int value2 = battleOptionType.IsConditionalOption() ? option.value2 : GetBattleOptionValue(option.value2, plusStatus.value2);
            return new BattleOption(battleOptionType, value1, value2);
        }

        private int GetBattleOptionValue(int value, int plusValue)
        {
            if (value == 0)
                return 0;

            return MathUtils.ToInt(value * (1 + MathUtils.ToPermyriadValue(plusValue)));
        }

        private int GetTier()
        {
            int type = GetLevelUpType(SlotType);

            int tier = 0;
            EquipItemLevelupData data = equipItemLevelupDataRepo.Get(type, Rating, Smelt);
            if (data != null)
                tier = data.tier_per;

            return tier;
        }

        private int GetLevelUpType(ItemEquipmentSlotType slotType)
        {
            int type = 0;
            switch (slotType)
            {
                case ItemEquipmentSlotType.Weapon:
                    type = 1;
                    break;

                case ItemEquipmentSlotType.HeadGear:
                case ItemEquipmentSlotType.Garment:
                case ItemEquipmentSlotType.Armor:
                case ItemEquipmentSlotType.Accessory1:
                case ItemEquipmentSlotType.Accessory2:
                    type = 2;
                    break;

                case ItemEquipmentSlotType.ShadowWeapon:
                    type = 3;
                    break;

                case ItemEquipmentSlotType.ShadowArmor:
                    type = 4;
                    break;

                case ItemEquipmentSlotType.ShadowHeadGear:
                    type = 5;
                    break;

                case ItemEquipmentSlotType.ShadowGarment:
                    type = 6;
                    break;

                case ItemEquipmentSlotType.ShadowAccessory1:
                    type = 7;
                    break;

                case ItemEquipmentSlotType.ShadowAccessory2:
                    type = 8;
                    break;
            }

            return type;
        }

        private bool GetIsEquippedCard()
        {
            return equippedCardNo1 != 0 || equippedCardNo2 != 0 || equippedCardNo3 != 0 || equippedCardNo4 != 0;
        }

        private ElementType GetElementType()
        {
            if ((SlotType == ItemEquipmentSlotType.Armor || SlotType == ItemEquipmentSlotType.Weapon) && ItemChangedElement > 0)
                return ItemChangedElement.ToEnum<ElementType>();

            return data.element_type.ToEnum<ElementType>();
        }

        private int GetElementLevel()
        {
            return itemElementLevel;
        }

        public override string GetEquiqWarningMessage(bool isPopupMessage)
        {
            int transcend = ItemTranscend;
            if (transcend == 0)
                return string.Empty;

            if (inventoryModel == null)
                return string.Empty;

            int jobLevel = inventoryModel.GetJobLevel();
            int needLevel = BasisType.ITEM_TRANSCEND_JOB_LEVEL.GetInt(transcend);

            // JobLevel 제한
            if (jobLevel < needLevel)
            {
                if (isPopupMessage)
                {
                    // JOB Lv이 부족하여 초월 장비를 장착할 수 없습니다.\n\n[c][4497F4](JOB Lv {LEVEL} 필요)[-][/c]
                    return LocalizeKey._6020.ToText().Replace(ReplaceKey.LEVEL, needLevel);
                }

                // JOB Lv\n{LEVEL}
                return LocalizeKey._6019.ToText().Replace(ReplaceKey.LEVEL, needLevel);
            }

            return string.Empty;
        }

        public override string GetPurchaseWarningMessage()
        {
            int transcend = ItemTranscend;
            if (transcend == 0)
                return string.Empty;

            if (inventoryModel == null)
                return string.Empty;

            int jobLevel = inventoryModel.GetJobLevel();
            int needLevel = BasisType.ITEM_TRANSCEND_JOB_LEVEL.GetInt(transcend);

            // JobLevel 제한
            if (jobLevel < needLevel)
            {
                // 현재 구매하려는 아이템은\n[c][4497F4]Job Lv[-][/c]이 [c][4497F4]부족[-][/c]하여 장착할 수 없습니다.\n\n그래도 구매하시겠습니까?
                return LocalizeKey._6021.ToText();
            }

            return string.Empty;
        }

        private int GetCardSlotCount()
        {
            if (IsShadow)
                return cardSlotCount;

            return Constants.Size.MAX_EQUIPPED_CARD_COUNT;
        }
    }
}