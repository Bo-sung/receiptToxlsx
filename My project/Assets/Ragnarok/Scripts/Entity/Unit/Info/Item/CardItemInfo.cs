using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class CardItemInfo : ItemInfo, CardItemInfo.ICardInfoSimple
    {
        public interface ICardInfoSimple
        {
            int ID { get; }
            int Rating { get; }
            int NameId { get; }
            string IconName { get; }
        }

        private readonly CardOptionDataManager cardOptionDataRepo;

        private ObscuredBool isEquipped;
        private ObscuredBool isLock;
        private ObscuredInt cardLevel;
        private ObscuredLong optionValue1, optionValue2, optionValue3, optionValue4;
        private ObscuredLong reOptionValue1, reOptionValue2, reOptionValue3, reOptionValue4;
        private float sumOptionRate;
        private int optionCount;
        private bool optionIsDirty = true;
        private int restorePointLevel = 0;

        public override bool IsStackable => false;
        public override int CardLevel => cardLevel;
        public override int MaxCardLevel => GetMaxCardLevel();
        public override bool IsCardMaxLevel => IsMaxLevel();
        public override float OptionRate
        {
            get
            {
                if (optionIsDirty)
                    CalculateOptionRate();
                return sumOptionRate / optionCount;
            }
        }
        public override float SuccessRate => GetSuccessRate();
        public override int NeedCardSmeltZeny => GetNeedCardSmeltZeny();
        public override bool IsEquipped => isEquipped;
        public override EquipmentClassType ClassType => data.class_type.ToEnum<EquipmentClassType>();
        public override bool IsLock => isLock;
        public override int Rating => data.rating;
        public override int BattleScore => GetBattleScore();
        public override string CardOptionPanelName => GetCardOptionPanel();
        public override int ItemTranscend => 0;
        public override int ItemChangedElement => 0;
        public int RestorePointLevel => restorePointLevel;
        public override bool IsShadow => data == null ? false : ItemDetailType == ItemDetailType.Shadow;
        public override ItemDetailType ItemDetailType => data.duration.ToEnum<ItemDetailType>();

        int ICardInfoSimple.ID => data.id;
        int ICardInfoSimple.Rating => data.rating;
        int ICardInfoSimple.NameId => data.name_id;
        string ICardInfoSimple.IconName => data.icon_name;

        public CardItemInfo()
        {
            cardOptionDataRepo = CardOptionDataManager.Instance;
            cardLevel = 1; // 카드레벨 기본 1
        }

        public override void SetItemInfo(int tier, int itemLevel, byte itemPos, long equippedCardNo1, long equippedCardNo2, long equippedCardNo3, long equippedCardNo4, bool isLock, int itemTranscend = 0, int itemChangedElement = 0, int itemElementLevel = 0)
        {
            this.isLock = isLock;
            cardLevel = tier;
            restorePointLevel = itemLevel > 0 ? itemLevel * 10 : 1;
            optionValue1 = equippedCardNo1;
            optionValue2 = equippedCardNo2;
            optionValue3 = equippedCardNo3;
            optionValue4 = equippedCardNo4;
            reOptionValue1 = itemTranscend / 10000;
            reOptionValue2 = itemTranscend % 10000;
            reOptionValue3 = itemChangedElement / 10000;
            reOptionValue4 = itemChangedElement % 10000;
            optionIsDirty = true;
        }

        public override void Reload(bool isEquipCard)
        {
        }

        public override void SetEquipped(bool isEquipped)
        {
            if (this.isEquipped == isEquipped)
                return;

            this.isEquipped = isEquipped;
            InvokeEvent();
        }

        /// <summary>
        /// 최대 제련도
        /// </summary>
        private int GetMaxCardLevel()
        {
            if (IsShadow)
                return BasisType.SHADOW_CARD_MAX_LEVEL.GetInt();

            return BasisType.CARD_SMELT_MAX_PER.GetInt();
        }

        protected override long GetEquippedCardNo(int index)
        {
            switch (index)
            {
                case 0: return optionValue1;
                case 1: return optionValue2;
                case 2: return optionValue3;
                case 3: return optionValue4;
            }

            throw new System.ArgumentException($"[올바르지 않은 {nameof(index)}] {nameof(index)} = {index}");
        }

        public override IEnumerable<BattleOption> GetBattleOptionCollection()
        {
            if (data.battle_option_type_1 != 0)
            {
                CardOptionData optionData = cardOptionDataRepo.Get(data.battle_option_type_1);
                yield return GetBattleOption(optionData, optionValue1);
            }

            if (data.battle_option_type_2 != 0)
            {
                CardOptionData optionData = cardOptionDataRepo.Get(data.battle_option_type_2);
                yield return GetBattleOption(optionData, optionValue2);
            }

            if (data.battle_option_type_3 != 0)
            {
                CardOptionData optionData = cardOptionDataRepo.Get(data.battle_option_type_3);
                yield return GetBattleOption(optionData, optionValue3);
            }

            if (data.battle_option_type_4 != 0)
            {
                CardOptionData optionData = cardOptionDataRepo.Get(data.battle_option_type_4);
                yield return GetBattleOption(optionData, optionValue4);
            }
        }

        private BattleOption GetBattleOption(CardOptionData optionData, long optionValue)
        {
            BattleOptionType battleOptionType = optionData.battle_option_type.ToEnum<BattleOptionType>();
            float permyriadValue1 = MathUtils.ToPermyriadValue(optionData.option_value_1_rate); // value1의 rate
            float permyriadValue2 = MathUtils.ToPermyriadValue(optionData.option_value_2_rate); // value2의 rate
            float value1 = battleOptionType.IsConditionalSkill() ?  permyriadValue1 : permyriadValue1 * optionValue; // optionValue 적용
            float value2 = battleOptionType.IsConditionalOption() ? permyriadValue2 : permyriadValue2 * optionValue;
            return new BattleOption(battleOptionType, MathUtils.ToInt(value1, 2), MathUtils.ToInt(value2, 2));
        }

        public override IEnumerable<CardBattleOption> GetCardBattleOptionCollection()
        {
            optionCount = 0;
            sumOptionRate = 0;

            if (data.battle_option_type_1 != 0)
            {
                CardOptionData optionData = cardOptionDataRepo.Get(data.battle_option_type_1);
                CardBattleOption option = GetCardBattleOption(optionData, optionValue1);
                optionCount++;
                if (option.battleOptionType.IsConditionalSkill())
                {
                    sumOptionRate += 1;
                }
                else
                {
                    sumOptionRate += MathUtils.GetRate((int)(option.serverValue - option.totalMinValue), (int)(option.totalMaxValue - option.totalMinValue));
                }
                yield return option;
            }

            if (data.battle_option_type_2 != 0)
            {
                CardOptionData optionData = cardOptionDataRepo.Get(data.battle_option_type_2);
                CardBattleOption option = GetCardBattleOption(optionData, optionValue2);
                optionCount++;
                if (option.battleOptionType.IsConditionalSkill())
                {
                    sumOptionRate += 1;
                }
                else
                {
                    sumOptionRate += MathUtils.GetRate((int)(option.serverValue - option.totalMinValue), (int)(option.totalMaxValue - option.totalMinValue));
                }
                yield return option;
            }

            if (data.battle_option_type_3 != 0)
            {
                CardOptionData optionData = cardOptionDataRepo.Get(data.battle_option_type_3);
                CardBattleOption option = GetCardBattleOption(optionData, optionValue3);
                optionCount++;
                if (option.battleOptionType.IsConditionalSkill())
                {
                    sumOptionRate += 1;
                }
                else
                {
                    sumOptionRate += MathUtils.GetRate((int)(option.serverValue - option.totalMinValue), (int)(option.totalMaxValue - option.totalMinValue));
                }
                yield return option;
            }

            if (data.battle_option_type_4 != 0)
            {
                CardOptionData optionData = cardOptionDataRepo.Get(data.battle_option_type_4);
                CardBattleOption option = GetCardBattleOption(optionData, optionValue4);
                optionCount++;
                if (option.battleOptionType.IsConditionalSkill())
                {
                    sumOptionRate += 1;
                }
                else
                {
                    sumOptionRate += MathUtils.GetRate((int)(option.serverValue - option.totalMinValue), (int)(option.totalMaxValue - option.totalMinValue));
                }
                yield return option;
            }
        }

        CardBattleOption GetCardBattleOption(CardOptionData optionData, long serverValue)
        {
            long totalMin = 0;
            long totalMax = 0;
            int rate1 = optionData.option_value_1_rate;
            int rate2 = optionData.option_value_2_rate;

            BattleOptionType battleOptionType = optionData.battle_option_type.ToEnum<BattleOptionType>();

            // 레벨 1부터 cardLevel 까지 누적
            for (int i = 1; i <= cardLevel; i++)
            {
                var data = GetCurMinMaxValue(optionData, i);
                totalMin += data.Item1;
                totalMax += data.Item2;
            }

            // 다음 레벨에 증가 수치
            int nextLevel = cardLevel + 1;
            var nextData = GetCurMinMaxValue(optionData, nextLevel);
            long nextMin = nextData.Item1;
            long nextMax = nextData.Item2;

            return new CardBattleOption(battleOptionType, nextMin, nextMax, totalMin, totalMax, serverValue, rate1, rate2);
        }

        /// <summary>
        /// 레벨에 따른 카드옵션 Min, Max
        /// </summary>
        /// <param name="optionData"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        (int, int) GetCurMinMaxValue(CardOptionData optionData, int level)
        {
            int min, max;

            if (optionData.value_1_prob != 0)
            {
                min = optionData.value_1_default_min;
                max = optionData.value_1_default_max;
            }
            else
            {
                min = optionData.value_2_default_min;
                max = optionData.value_2_default_max;
            }

            if (level == 1)
                return (min, max);

            // 최대 레벨 도달시 오를 수치 없음.
            if (level > MaxCardLevel)
                return (0, 0);

            if (level % optionData.level_division != optionData.level_mod)
                return (0, 0);

            int amplification = 1;
            if (optionData.marking_level != 0 && level % optionData.marking_level == 0)
                amplification = optionData.amplification_value;

            // 소수점 첫번째 자리에서 올림        
            int increase = Mathf.CeilToInt((level / (float)optionData.marking_level) - 1);

            min = (optionData.period_min + optionData.increase_value * increase) * amplification;
            max = (optionData.period_max + optionData.increase_value * increase) * amplification;

            return (min, max);
        }

        public override string GetBackSpriteName(bool isInventory, bool supportTranscend = false)
        {
            if (isInventory)
                return IsShadow ? Constants.CommonAtlas.UI_COMMON_BG_ITEM_TYPE2_SHADOW : Constants.CommonAtlas.Ui_Common_BG_Item_Type2_Default;

            return IsShadow ? Constants.CommonAtlas.UI_COMMON_BG_ITEM_SHADOW : Constants.CommonAtlas.Ui_Common_BG_Item_Default;
        }

        public override string GetSlotIconName()
        {
            if (IsShadow)
                return Constants.CommonAtlas.UI_COMMON_ICON_CARD_3;

            if (IsCardMaxLevel)
                return Constants.CommonAtlas.UI_COMMON_ICON_CARD_2;

            if (IsEquipped)
                return Constants.CommonAtlas.UI_COMMON_ICON_CARD_1;

            return Constants.CommonAtlas.UI_COMMON_ICON_CARD_NONE;
        }

        public override string GetCardLevelView()
        {
            return CardLevel.ToString();
        }

        public int GetNeedCardSmeltZeny()
        {
            if (IsShadow)
                return cardLevel * BasisType.SHADOW_CARD_LEVEL_UP_ZENY.GetInt();

            return CardLevel * BasisType.CONST_ZENY.GetInt(3);
        }

        private float GetSuccessRate()
        {
            return BasisType.CARD_SMELT_SUCCESS_RATE.GetFloat(CardLevel);
        }

        private bool IsMaxLevel()
        {
            return CardLevel == MaxCardLevel;
        }

        public override long GetCardOptionValue(int index)
        {
            switch (index)
            {
                case 0: return optionValue1;
                case 1: return optionValue2;
                case 2: return optionValue3;
                case 3: return optionValue4;
            }

            throw new System.ArgumentException($"[올바르지 않은 {nameof(index)}] {nameof(index)} = {index}");
        }

        private int GetBattleScore()
        {
            float cardAP = 0f;
            cardAP += Rating * BasisType.ATTACK_POWER_COEFFICIENT.GetInt(7) * 0.01f;
            cardAP *= 1f + (CardLevel * BasisType.ATTACK_POWER_COEFFICIENT.GetInt(12) * 0.01f);
            return MathUtils.ToInt(cardAP);
        }

        private string GetCardOptionPanel()
        {
            return data.prefab_name; // 카드패널 이름
        }

        public void CalculateOptionRate()
        {
            optionIsDirty = false;
            foreach (var each in GetCardBattleOptionCollection()) ; // 여기서만 초기화되는 필드가 있어서, 초기화를 목적으로 돌립니다.
        }

        protected override string GetDescription()
        {
            if (IsShadow && CanTrade)
            {
                return StringBuilderPool.Get()
                       .Append(data.des_id.ToText())
                       .Append(LocalizeKey._30509.ToText()) // [c][cdcdcd] (레벨1만 드레이드)[-][/c]
                       .Release();
            }

            return base.GetDescription();
        }

        public CardItemInfo GetRestoredCard()
        {
            var ret = new CardItemInfo();
            ret.SetData(data);
            ret.SetItemInfo(restorePointLevel, 0, 0
                , reOptionValue1
                , reOptionValue2
                , reOptionValue3
                , reOptionValue4
                , false
                , 0
                , 0
                , 0);

            return ret;
        }
    }
}