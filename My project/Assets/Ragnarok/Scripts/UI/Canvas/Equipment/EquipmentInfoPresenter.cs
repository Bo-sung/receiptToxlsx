using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class EquipmentInfoPresenter : ViewPresenter
    {
        /******************** Models ********************/
        private InventoryModel inventoryModel;
        private readonly QuestModel questModel;
        private readonly ItemDataManager itemDataRepo;

        /******************** Repositories ********************/
        private readonly EquipItemLevelupDataManager equipItemLevelupDataRepo;

        /******************** Event ********************/
        public event InventoryModel.ItemUpdateEvent OnUpdateItem
        {
            add { inventoryModel.OnUpdateItem += value; }
            remove { inventoryModel.OnUpdateItem -= value; }
        }

        public event System.Action<bool, int> OnEquipItemLevelUp
        {
            add { inventoryModel.OnEquipItemLevelUp += value; }
            remove { inventoryModel.OnEquipItemLevelUp -= value; }
        }

        public event InventoryModel.EquipmentEvent OnUpdateEquipment
        {
            add { inventoryModel.OnUpdateEquipment += value; }
            remove { inventoryModel.OnUpdateEquipment -= value; }
        }

        private long equipmentNo;
        private bool isRequestEquipmentLevelUp; // 튜토리얼 전용

        public EquipmentInfoPresenter()
        {
            inventoryModel = Entity.player.Inventory;
            questModel = Entity.player.Quest;
            itemDataRepo = ItemDataManager.Instance;

            equipItemLevelupDataRepo = EquipItemLevelupDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 다른 플레이어의 인벤토리 사용 (null : 내 플레이어)
        /// </summary>
        public void SetOthersInventoryModel(InventoryModel invenModel)
        {
            if (invenModel is null)
                invenModel = Entity.player.Inventory;

            if (ReferenceEquals(this.inventoryModel, invenModel))
                return;

            this.inventoryModel = invenModel;
        }

        public void SetEquipmentNo(long equipmentNo)
        {
            this.equipmentNo = equipmentNo;
        }

        public ItemInfo GetEquipment()
        {
            return inventoryModel.GetItemInfo(equipmentNo);
        }

        public bool CanLevelUp()
        {
            bool canLevelUp = true;
            RewardData material = GetMeterialItem();
            if (material != null)
            {
                if (material.RewardType == RewardType.Item)
                {
                    int count = GetItemCount(material.ItemId);
                    if (count < material.Count) // 재료 부족
                        canLevelUp = false;
                }
                else
                {
                    CoinType coinType = material.RewardType.ToCoinType();
                    long count = coinType.GetCoin();
                    if (count < material.Count)
                        canLevelUp = false;
                }
            }

            return canLevelUp;
        }

        /// <summary>
        /// 강화에 필요한 재료
        /// </summary>
        /// <returns></returns>
        public RewardData GetMeterialItem()
        {
            ItemInfo info = GetEquipment();

            int type =GetLevelUpType(info.SlotType);

            EquipItemLevelupData levelUpData = equipItemLevelupDataRepo.Get(type, info.Rating, info.Smelt);
            if (levelUpData is null)
                return null;

            RewardData material = new RewardData(levelUpData.resource_type, levelUpData.resource_value, levelUpData.resource_count);

            return material;
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

        public int GetItemCount(int itemId)
        {
            return inventoryModel.GetItemCount(itemId);
        }

        public void OnSelectCardSlot(byte index, CardSlotEvent cardSlotEvent)
        {
            if (!IsEditableEquipment())
                return;

            Debug.Log($"카드 슬롯 클릭 이벤트={index} {cardSlotEvent}");
            switch (cardSlotEvent)
            {
                case CardSlotEvent.Smelt:
                    UI.Show<UICardSmelt>(GetEquipment().GetCardItem(index));
                    break;

                case CardSlotEvent.AutoEquip:
                    inventoryModel.RequestAutoCardEquip(equipmentNo, (byte)(index + 1)).WrapNetworkErrors();
                    break;

                case CardSlotEvent.UnEquip:
                    var sender = new CardEquipSender[1];
                    sender[0] = new CardEquipSender(0, (byte)(index + 1));

                    inventoryModel.RequestMultiCardEquip(equipmentNo, sender).WrapNetworkErrors();
                    break;

                case CardSlotEvent.CardInven:
                    UI.Show<UICardInven>(new UICardInvenData(GetEquipment(), index));
                    break;

                case CardSlotEvent.CardInfo:
                    UI.Show<UICardInfo>(new UICardInfo.Input(GetEquipment().GetCardItem(index)));
                    break;

                case CardSlotEvent.SlotUnLock:

                    ItemData data = GetShadowCardOpenItem();
                    if (data == null)
                        return;

                    var myCount = GetItemCount(data.id);
                    var needCount = 1;

                    int equipLevel = GetEquipment().Smelt;
                    int level = BasisType.EQUIPMENT_CARD_SLOT_UNLOCK_LEVEL.GetInt(index + 1);

                    bool isNeedLevelUp = equipLevel < level; // 장비 강화 필요 여부
                    bool isNeedMatarial = myCount < needCount; // 재료 필요 여부

                    var sb = StringBuilderPool.Get();
                    bool isAllow = true;                    

                    if(isNeedMatarial)
                    {
                        isAllow = false;
                        sb.Append(LocalizeKey._90285.ToText() // [62AEE4][C]{ITEM}[/c][-] 재료가 부족합니다.
                            .Replace(ReplaceKey.ITEM, data.name_id.ToText()));

                        if (isNeedLevelUp)
                            sb.AppendLine();
                    }
                  
                    if(isNeedLevelUp)
                    {
                        isAllow = false;
                        sb.Append(LocalizeKey._90286.ToText() // [62AEE4][C]+{VALUE}강화[/c][-]가 필요합니다.
                            .Replace(ReplaceKey.VALUE, level));
                    }

                    if(isAllow)
                    {
                        // 강화 재료로 아이템 투입
                        sb.Append(LocalizeKey._90284.ToText() // [62AEE4][C]{ITEM}[/c][-]을 소모하여 슬롯을 해금합니다.\n계속 하시겠습니까?
                            .Replace(ReplaceKey.ITEM, data.name_id.ToText()));
                    }

                    RewardData reward = new RewardData(RewardType.Item, data.id, 1);
                    UI.Show<UISelectMaterialPopup>().Set(reward, myCount, needCount, sb.Release(), LocalizeKey._2906.ToText(), isAllow, OnAllowShadowOpenCardSlot);
                    break;
            }
        }

        void OnAllowShadowOpenCardSlot()
        {
            inventoryModel.RequestShadowItemOpenCardSlot(equipmentNo);
        }

        /// <summary>
        /// 아이템 장착,교체,해제
        /// </summary>
        public void OnClickedBtnEquip()
        {
            inventoryModel.RequestItemEquip(GetEquipment()).WrapNetworkErrors();
        }

        /// <summary>
        /// 장비 강화 요청
        /// </summary>
        public void OnClickedBtnLevelUp()
        {
            if (!questModel.IsOpenContent(ContentType.ItemEnchant))
                return;

            RewardData material = GetMeterialItem();

            if (material == null)
                return;

            if (material.RewardType == RewardType.Item)
            {
                var myCount = GetItemCount(material.ItemId);
                var needCount = material.Count;

                string text;
                bool isAllow = true;
                if (myCount >= needCount)
                {
                    // 강화 재료로 아이템 투입
                    text = LocalizeKey._90175.ToText()
                        .Replace(ReplaceKey.ITEM, material.ItemName); // [62AEE4][C]{ITME}[/c][-]을 소모하여 장비를 강화합니다.\n계속 하시겠습니까?
                }
                else
                {
                    isAllow = false;
                    text = LocalizeKey._16018.ToText(); // 강화 재료가 부족합니다
                }

                UI.Show<UISelectMaterialPopup>().Set(material, myCount, needCount, text, LocalizeKey._16003.ToText(), isAllow, RequestEquipmentLevelUp);
            }
            else
            {
                // 재화 체크
                CoinType coinType = material.RewardType.ToCoinType();
                int needCoin = material.Count;
                if (!coinType.Check(needCoin))
                    return;

                RequestEquipmentLevelUp();
            }
        }

        /// <summary>
        /// 강화
        /// </summary>
        private void RequestEquipmentLevelUp()
        {
            isRequestEquipmentLevelUp = true;
            inventoryModel.RequestEquipmentLevelUp(GetEquipment()).WrapNetworkErrors();
        }

        /// <summary>
        /// 강화 가능 여부
        /// </summary>
        public bool CanEquipmentLevelUp()
        {
            if (!questModel.IsOpenContent(ContentType.ItemEnchant))
                return false;

            RewardData material = GetMeterialItem();

            if (material == null)
                return false;

            if (material.RewardType == RewardType.Item)
            {
                var myCount = GetItemCount(material.ItemId);
                var needCount = material.Count;
                if (myCount < needCount)
                    return false;
            }
            else
            {
                // 재화 체크
                CoinType coinType = material.RewardType.ToCoinType();
                int needCoin = material.Count;
                if (!coinType.Check(needCoin))
                    return false;
            }

            return true;
        }

        public bool IsRequestEquipmentLevelUp()
        {
            if (isRequestEquipmentLevelUp)
            {
                isRequestEquipmentLevelUp = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 아이템 잠금 버튼 이벤트
        /// </summary>
        public void OnClickedBtnLock()
        {
            inventoryModel.RequestItemLock(GetEquipment()).WrapNetworkErrors();
        }

        /// <summary>
        /// 편집 가능한 장비인지 (내 장비인지)
        /// </summary>
        public bool IsEditableEquipment() => ReferenceEquals(this.inventoryModel, Entity.player.Inventory);

        /// <summary>
        /// 신규 컨텐츠 플래그 제거
        /// </summary>
        public void RemoveNewOpenContent_ItemEnchant()
        {
            questModel.RemoveNewOpenContent(ContentType.ItemEnchant); // 신규 컨텐츠 플래그 제거 (아이템 강화)
        }

        /// <summary>
        /// 쉐도우 장비 카드 슬롯 오픈 아이템
        /// </summary>
        public ItemData GetShadowCardOpenItem()
        {
            ItemData data = itemDataRepo.Get(BasisItem.ShadowCardSlotOpen.GetID());
            return data;
        }

        /// <summary>
        /// 쉐도우 장비 카드 슬롯 오픈시 필요 아이템 아이콘 이름
        /// </summary>
        public string GetShadowCardOpenSlotItemIconName()
        {
            ItemData data = GetShadowCardOpenItem();
            if (data == null)
                return string.Empty;

            return data.icon_name;
        }

        /// <summary>
        /// 쉐도우 장비 카드 슬롯 오픈시 필요 아이템 이름
        /// </summary>
        public string GetShadowCardOpenSlotItemName()
        {
            ItemData data = GetShadowCardOpenItem();
            if (data == null)
                return string.Empty;

            return data.name_id.ToText();
        }
    }
}