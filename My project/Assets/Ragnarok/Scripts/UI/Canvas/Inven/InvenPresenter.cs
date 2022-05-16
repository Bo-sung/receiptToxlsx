using Ragnarok.View;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIInven"/>
    /// </summary>
    public sealed class InvenPresenter : ViewPresenter, UICostumeInfoSlot.Impl
    {
        public interface IView
        {
            void SetZeny(long value);
            void SetCatCoin(long value);
            void ShowInvenWeight(int invenWeight, int currentInvenWeight);
            void UpdateView();
            void SortItem();

            EquipmentItemView GetEquipmentItemView();
            CardItemView GetCardItemView();
            CostumeItemView GetCostumeView();
        }

        /// <summary>
        /// 분해 모드
        /// </summary>
        public bool IsDisassembleMode { get; private set; }

        private readonly IView view;
        private readonly InventoryModel inventoryModel;
        private readonly GoodsModel goodsModel;
        private readonly BetterList<long> selectedDisassembleNos;

        // 전투력 계산 관련
        private int savedAP;
        private readonly BetterList<long> savedEquipItemList;
        private readonly Dictionary<long, int> savedEquipmentAP; // dic[itemNo] = AP;
        private readonly Buffer<ItemInfo> valuableItemBuffer; // 귀중품

        public InvenPresenter(IView view)
        {
            this.view = view;
            inventoryModel = Entity.player.Inventory;
            goodsModel = Entity.player.Goods;
            selectedDisassembleNos = new BetterList<long>();
            savedAP = 0;
            savedEquipItemList = new BetterList<long>();
            savedEquipmentAP = new Dictionary<long, int>(LongEqualityComparer.Default);
            valuableItemBuffer = new Buffer<ItemInfo>();
            OnUpdateInvenWeight();
        }

        public override void AddEvent()
        {
            goodsModel.OnUpdateZeny += view.SetZeny;
            goodsModel.OnUpdateCatCoin += view.SetCatCoin;
            inventoryModel.OnUpdateItem += view.UpdateView;
            inventoryModel.OnUpdateInvenWeight += OnUpdateInvenWeight;
        }

        public override void RemoveEvent()
        {
            goodsModel.OnUpdateZeny -= view.SetZeny;
            goodsModel.OnUpdateCatCoin -= view.SetCatCoin;
            inventoryModel.OnUpdateItem -= view.UpdateView;
            inventoryModel.OnUpdateInvenWeight -= OnUpdateInvenWeight;
        }

        public void OnClickedBtnZeny()
        {
            UI.ShowZenyShop();
        }

        public void OnClickedBtnCatCoin()
        {
            UI.ShowCashShop();
        }

        /// <summary>
        /// 저장된 AP 정보가 유효한지 체크하고, 유효하지 않으면 초기화.
        /// </summary>
        void UpdateSavedAP()
        {
            if (Entity.player.SavedBattleStatusData is null)
                return;

            // 장착 장비 정보 비교
            ItemInfo[] equippedItems = inventoryModel.GetEquippedItems();
            bool isSameList = true;
            if (equippedItems.Length == savedEquipItemList.size)
            {
                foreach (var itemNo in savedEquipItemList)
                {
                    if (!System.Array.Exists(equippedItems, e => e.ItemNo == itemNo))
                    {
                        isSameList = false;
                        break;
                    }
                }
            }
            else
            {
                isSameList = false;
            }

            // 전투력 비교
            bool isSameAP = (Entity.player.SavedBattleStatusData.AP == savedAP);

            if (isSameList && isSameAP)
                return;

            // 전투력이 변동되었다면 가진 장비들에 대해서 전부 AP저장. -> 필요한 애들이 그때그때 저장하도록.
            savedEquipmentAP.Clear();

            //var equipmentList = inventoryModel.itemList.FindAll(e => e is EquipmentItemInfo);
            //foreach (var item in equipmentList)
            //{
            //    EquipmentItemInfo equipmentItem = item as EquipmentItemInfo;
            //    savedEquipmentAP[equipmentItem.ItemNo] = equipmentItem.GetAttackPower();
            //}

            // 최근 상태 저장
            savedEquipItemList.Clear();
            foreach (var item in equippedItems)
            {
                savedEquipItemList.Add(item.ItemNo);
            }

            savedAP = Entity.player.SavedBattleStatusData.AP;
        }

        /// <summary>
        /// 이 장비가 현재 장착중인 장비보다 강한지 체크.
        /// </summary>
        public bool IsStrongerEquipment(EquipmentItemInfo equipment)
        {
            if (equipment == null)
                return false;

            // 쉐도우 장비는 비교 체크 제거
            if (equipment.IsShadow)
                return false;

            if (equipment.SlotType == ItemEquipmentSlotType.Weapon && !inventoryModel.IsJobRecommendClassType(equipment.ClassType, Entity.player.Character.Job))
                return false;

            UpdateSavedAP();

            // 장착중인 아이템의 AP
            EquipmentItemInfo equippedItem = Find(equipment.SlotType);
            int equippedItemPower = GetEquipmentAttackPower(equippedItem);

            // 새 아이템의 AP
            int equipmentPower = GetEquipmentAttackPower(equipment);

            // 비교
            return (equipmentPower > equippedItemPower);
        }

        private EquipmentItemInfo Find(ItemEquipmentSlotType slotType)
        {
            ItemInfo[] equippedItems = inventoryModel.GetEquippedItems();
            for (int i = 0; i < equippedItems.Length; i++)
            {
                if (equippedItems[i].SlotType == slotType)
                    return equippedItems[i] as EquipmentItemInfo;
            }

            return null;
        }

        /// <summary>
        /// 해당 장비의 전투력수치 얻기. (인벤토리 캐싱)
        /// </summary>
        private int GetEquipmentAttackPower(EquipmentItemInfo equipment)
        {
            if (equipment == null)
                return 0;

            if (!savedEquipmentAP.ContainsKey(equipment.ItemNo))
            {
                savedEquipmentAP[equipment.ItemNo] = equipment.GetAttackPower();
            }

            return savedEquipmentAP[equipment.ItemNo];
        }

        /// <summary>
        /// 가장 강한 장비들로 자동 장착
        /// </summary>
        public void AutoEquip()
        {
            // 가방을 돌면서 각 슬롯타입의 장비 중에서 가장 강한 아이템을 추려내서 장착한다.
            List<ItemInfo> itemList = inventoryModel.itemList.FindAll(e => e is EquipmentItemInfo);
            List<ItemInfo> toEquipList = new List<ItemInfo>();

            // 쉐도우 장비는 자동장착에서 제외
            ItemEquipmentSlotType[] slotTypes =
            {
                ItemEquipmentSlotType.Weapon,
                ItemEquipmentSlotType.HeadGear,
                ItemEquipmentSlotType.Garment,
                ItemEquipmentSlotType.Armor,
                ItemEquipmentSlotType.Accessory1,
                ItemEquipmentSlotType.Accessory2,
            };

            for (int i = 0; i < slotTypes.Length; ++i)
            {
                ItemEquipmentSlotType thisType = slotTypes[i];

                EquipmentItemInfo pickedItem = GetStrongestEquipmentInSlotType(itemList, thisType);
                if (pickedItem)
                {
                    toEquipList.Add(pickedItem);
                }
            }

            // 이미 장착중이라면 리스트에서 제거.
            toEquipList.RemoveAll(e => e.IsEquipped);

            inventoryModel.RequestMultiItemEquip(toEquipList).WrapNetworkErrors();
        }

        /// <summary>
        /// 리스트의 해당 슬롯타입 장비 중에서 가장 강한 아이템을 반환
        /// </summary>
        private EquipmentItemInfo GetStrongestEquipmentInSlotType(List<ItemInfo> itemList, ItemEquipmentSlotType slotType)
        {
            List<ItemInfo> equipmentList = itemList.FindAll(e => e is EquipmentItemInfo && e.SlotType == slotType);

            // 무기면 직업추천타입 여부를 따로 검사한다.
            bool isWeapon = slotType == ItemEquipmentSlotType.Weapon;
            if (isWeapon)
            {
                // 리스트 안에 하나라도 내 직업추천무기가 있다면, 
                // 직업추천무기가 아닌 장비들은 전부 리스트에서 제외한다.
                if (equipmentList.Exists(e => IsJobRecommendClassType(e.ClassType)))
                {
                    equipmentList.RemoveAll(e => !IsJobRecommendClassType(e.ClassType));
                }
            }

            // 가장 강한 아이템 뽑아서 반환
            EquipmentItemInfo strongestItem = null;
            int strongestAP = 0;
            foreach (var item in equipmentList)
            {
                EquipmentItemInfo thisItem = item as EquipmentItemInfo;
                int thisAP = thisItem.GetAttackPower();
                if (strongestAP < thisAP || strongestItem == null)
                {
                    strongestAP = thisAP;
                    strongestItem = thisItem;
                }
            }

            return strongestItem;
        }

        /// <summary>
        /// 해당 클래스타입이 내 직업의 추천 타입인지
        /// </summary>
        private bool IsJobRecommendClassType(EquipmentClassType classType)
        {
            EquipmentClassType appropriateEquipmentClassType = Entity.player.Character.Job.GetJobAppropriateEquipmentClassTypes();
            return appropriateEquipmentClassType.HasFlag(classType);
        }

        /// <summary>
        /// 장비탭 반환
        /// </summary>
        /// <returns></returns>
        public EquipmentItemView GetEquipmentItemView()
        {
            return view.GetEquipmentItemView();
        }

        /// <summary>
        /// 카드탭 반환
        /// </summary>
        /// <returns></returns>
        public CardItemView GetCardItemView()
        {
            return view.GetCardItemView();
        }

        /// <summary>
        /// 장비목록 반환
        /// </summary>
        /// <returns></returns>
        public ItemInfo[] GetEquipmentItemInfos(int rank, ItemEquipmentSlotType type)
        {
            if (IsDisassembleMode)
            {
                return inventoryModel.itemList.FindAll(a => (a is EquipmentItemInfo)
                    && (rank != 0 ? a.Rating == rank : a.Rating >= 0)
                    && IncludeSlotType(type, a.ClassType)
                    && (a.IsEquipped == false)
                    && (a.IsLock == false)).ToArray();
            }

            return inventoryModel.itemList.FindAll(a => (a is EquipmentItemInfo)
                && (rank != 0 ? a.Rating == rank : a.Rating >= 0)
                && IncludeSlotType(type, a.ClassType)).ToArray();
        }

        private bool IncludeSlotType(ItemEquipmentSlotType type, EquipmentClassType classType)
        {
            switch (type)
            {
                case ItemEquipmentSlotType.HeadGear:
                    return classType.HasFlag(EquipmentClassType.HeadGear);
                case ItemEquipmentSlotType.Garment:
                    return classType.HasFlag(EquipmentClassType.Garment);
                case ItemEquipmentSlotType.Armor:
                    return classType.HasFlag(EquipmentClassType.Armor);
                case ItemEquipmentSlotType.Accessory1:
                    return classType.HasFlag(EquipmentClassType.Accessory1);
                case ItemEquipmentSlotType.Accessory2:
                    return classType.HasFlag(EquipmentClassType.Accessory2);

                case ItemEquipmentSlotType.Weapon:
                    return classType.HasFlag(EquipmentClassType.OneHandedSword) ||
                        classType.HasFlag(EquipmentClassType.OneHandedStaff) ||
                        classType.HasFlag(EquipmentClassType.Dagger) ||
                        classType.HasFlag(EquipmentClassType.Bow) ||
                        classType.HasFlag(EquipmentClassType.TwoHandedSword) ||
                        classType.HasFlag(EquipmentClassType.TwoHandedSpear);

                case ItemEquipmentSlotType.None:
                    return true;

            }
            return false;
        }

        /*private bool IsSlotType(ItemEquipmentSlotType type, ItemEquipmentSlotType slotType)
        {
            switch (type)
            {
                case ItemEquipmentSlotType.None:
                    return slotType != ItemEquipmentSlotType.None;

                case ItemEquipmentSlotType.HeadGear:
                    return slotType == ItemEquipmentSlotType.HeadGear || slotType == ItemEquipmentSlotType.ShadowHeadGear;
                case ItemEquipmentSlotType.Garment:
                    return slotType == ItemEquipmentSlotType.Garment || slotType == ItemEquipmentSlotType.ShadowGarment;
                case ItemEquipmentSlotType.Armor:
                    return slotType == ItemEquipmentSlotType.Armor || slotType == ItemEquipmentSlotType.ShadowArmor;
                case ItemEquipmentSlotType.Weapon:
                    return slotType == ItemEquipmentSlotType.Weapon || slotType == ItemEquipmentSlotType.ShadowWeapon;
                case ItemEquipmentSlotType.Accessory1:
                    return slotType == ItemEquipmentSlotType.Accessory1 || slotType == ItemEquipmentSlotType.ShadowAccessory1;
                case ItemEquipmentSlotType.Accessory2:
                    return slotType == ItemEquipmentSlotType.Accessory2 || slotType == ItemEquipmentSlotType.ShadowAccessory2;               
            }
            return slotType == type;
        }*/

        /// <summary>
        /// 카드목록 반환
        /// </summary>
        /// <returns></returns>
        public ItemInfo[] GetCardItemInfos(int rank)
        {
            return inventoryModel.itemList.FindAll(a => a is CardItemInfo && a.IsEquipped == false && (rank == 0 ? true : a.Rating == rank)).ToArray();
        }

        /// <summary>
        /// 재료목록 반환
        /// </summary>
        public ItemInfo[] GetPartsItemInfos()
        {
            return inventoryModel.itemList.FindAll(a => a is PartsItemInfo).ToArray();
        }

        /// <summary>
        /// 소모품 목록 반환
        /// </summary>
        public ItemInfo[] GetConsumableItemInfos()
        {
            return inventoryModel.itemList.FindAll(a => a is ConsumableItemInfo).ToArray();
        }

        /// <summary>
        /// 박스 목록 반환
        /// </summary>
        public ItemInfo[] GetBoxItemInfos()
        {
            return inventoryModel.itemList.FindAll(a => a is BoxItemInfo).ToArray();
        }

        /// <summary>
        /// 코스튬 목록 반환
        /// </summary>
        /// <returns></returns>
        public ItemInfo[] GetCostumeItemInfos()
        {
            if (IsDisassembleMode)
            {
                return inventoryModel.itemList.FindAll(a => (a is CostumeItemInfo)
                    && (a.IsEquipped == false)
                    && (a.IsLock == false)).ToArray();
            }

            return inventoryModel.itemList.FindAll(a => a is CostumeItemInfo).ToArray();
        }

        public void ClearAllDisassemble()
        {
            selectedDisassembleNos.Clear();
        }

        /// <summary>
        /// 분해 전체 선택
        /// </summary>
        public void SelectAllDisassemble(ItemGroupType itemGroupType, int rank = 0, ItemEquipmentSlotType slotType = ItemEquipmentSlotType.None)
        {
            ClearAllDisassemble();

            List<ItemInfo> array = null;

            switch (itemGroupType)
            {
                case ItemGroupType.Equipment:
                    array = inventoryModel.itemList.FindAll(a => (a is EquipmentItemInfo)
                        && (rank != 0 ? a.Rating == rank : a.Rating >= 0)
                        && (slotType != ItemEquipmentSlotType.None ? a.SlotType == slotType : a.SlotType != ItemEquipmentSlotType.None)
                        && (a.IsEquipped == false)
                        && (a.IsLock == false));
                    break;
                case ItemGroupType.Card:
                    array = inventoryModel.itemList.FindAll(e => (e is CardItemInfo) &&
                        (rank == 0 ? true : (e.Rating == rank)) &&
                        e.IsEquipped == false &&
                        e.IsLock == false);
                    break;
                case ItemGroupType.Costume:
                    array = inventoryModel.itemList.FindAll(e => (e is CostumeItemInfo) &&
                        e.IsEquipped == false);
                    break;
            }

            foreach (var item in array)
            {
                selectedDisassembleNos.Add(item.ItemNo);
            }
            view.UpdateView();
        }

        /// <summary>
        /// 분해모드 아이템 선택
        /// </summary>
        /// <param name="info"></param>
        public void SelectDisassemble(ItemInfo info)
        {
            if (IsDisassemble(info.ItemNo))
            {
                RemoveDisassembleMode(info);
            }
            else
            {
                AddDisassembleMode(info);
            }
        }

        /// <summary>
        /// 분해 목록 추가
        /// </summary>
        public void AddDisassembleMode(ItemInfo info)
        {
            if (info.IsEquipped)
            {
                string title = LocalizeKey._5.ToText(); // 알람
                string description = LocalizeKey._90017.ToText(); // 장착 중인 장비는 분해할 수 없습니다.                                                                 
                UI.ConfirmPopup(title, description);
                return;
            }

            selectedDisassembleNos.Add(info.ItemNo);
        }

        /// <summary>
        /// 분해 목록 제거
        /// </summary>
        /// <param name="info"></param>
        public void RemoveDisassembleMode(ItemInfo info)
        {
            selectedDisassembleNos.Remove(info.ItemNo);
        }

        /// <summary>
        /// 분해 아이템 선택여부
        /// </summary>
        /// <param name="itemNo"></param>
        /// <returns></returns>
        public bool IsDisassemble(long itemNo)
        {
            return selectedDisassembleNos.Contains(itemNo);
        }

        /// <summary>
        /// 분해모드 여부
        /// </summary>
        /// <param name="isDisassemble"></param>
        public void SetDisassembleMode(bool isDisassemble, bool isUpdateView = true)
        {
            IsDisassembleMode = isDisassemble;
            selectedDisassembleNos.Clear();
            if (isUpdateView)
            {
                view.SortItem();
                view.UpdateView();
            }
        }

        /// <summary>
        /// 인벤 확장
        /// </summary>
        public void RequestInvenExpand()
        {
            inventoryModel.RequestInvenExpand().WrapNetworkErrors();
        }

        /// <summary>
        /// 장비 분해
        /// </summary>
        public void RequestItemDisassemble(ItemGroupType itemGroupType)
        {
            RequestItemDisassembleAsync(itemGroupType).WrapNetworkErrors();
        }

        private async Task RequestItemDisassembleAsync(ItemGroupType itemGroupType)
        {
            string title;
            string description = string.Empty;
            if (selectedDisassembleNos.size == 0)
            {
                // 분해 할 장비를 선택안함
                title = LocalizeKey._5.ToText(); // 알람
                switch (itemGroupType)
                {
                    case ItemGroupType.Equipment:
                        description = LocalizeKey._90016.ToText(); // 분해할 장비를 선택해주세요.
                        break;

                    case ItemGroupType.Card:
                        description = LocalizeKey._90091.ToText(); // 분해할 카드를 선택해주세요.
                        break;

                    case ItemGroupType.Costume:
                        description = LocalizeKey._90208.ToText(); // 분해할 코스튬을 선택해주세요.
                        break;
                }

                UI.ConfirmPopup(title, description);
                return;
            }

            // 카드 인첸트 체크
            switch (itemGroupType)
            {
                case ItemGroupType.Equipment:
                    description = LocalizeKey._90018.ToText(); // 장비를 분해하시겠습니까?
                    break;

                case ItemGroupType.Card:
                    description = LocalizeKey._90092.ToText(); // 카드를 분해하시겠습니까?
                    break;

                case ItemGroupType.Costume:
                    description = LocalizeKey._90209.ToText(); // 코스튬을 분해하시겠습니까?
                    break;
            }

            foreach (var item in selectedDisassembleNos)
            {
                ItemInfo itemInfo = inventoryModel.GetItemInfo(item);
                if (itemInfo == null)
                    continue;

                if (itemInfo is EquipmentItemInfo equipmentItemInfo)
                {
                    // 4성 이상의 장비아이템, 카드장착된 장비아이템
                    if (equipmentItemInfo.Rating >= 4 || equipmentItemInfo.IsCardEnchanted)
                    {
                        valuableItemBuffer.Add(itemInfo);
                    }
                }
                else if (itemInfo is CardItemInfo cardItemInfo)
                {
                    // 4성 이상의 카드, 30강 이상의 카드
                    if (cardItemInfo.Rating >= 4 || cardItemInfo.CardLevel >= 30)
                    {
                        valuableItemBuffer.Add(itemInfo);
                    }
                }
            }

            ItemInfo[] valuableItems = valuableItemBuffer.GetBuffer(isAutoRelease: true);
            if (valuableItems.Length == 0)
            {
                if (!await UI.SelectPopup(description))
                    selectedDisassembleNos.Clear();
            }
            else
            {
                // 선택 해제된 아이템
                long[] unselectedItemNos = await UI.Show<UIItemUnselect>().Show(valuableItems);

                // null 일 경우에는 아예 취소
                if (unselectedItemNos == null)
                {
                    selectedDisassembleNos.Clear();
                }
                else
                {
                    for (int i = 0; i < unselectedItemNos.Length; i++)
                    {
                        selectedDisassembleNos.Remove(unselectedItemNos[i]);
                    }
                }
            }

            long[] finalSelectedDisassembleNos = selectedDisassembleNos.ToArray();
            int size = finalSelectedDisassembleNos == null ? 0 : finalSelectedDisassembleNos.Length;
            if (size > 0)
            {
                await inventoryModel.RequestItemDisassemble(finalSelectedDisassembleNos, type: 1, itemGroupType);
            }

            SetDisassembleMode(false);
        }

        private void OnUpdateInvenWeight()
        {
            view.ShowInvenWeight(inventoryModel.MaxInvenWeight, inventoryModel.CurrentInvenWeight);
        }

        public void HideNew(ItemInfo[] arrayInfo)
        {
            inventoryModel.SetHideNew(arrayInfo);
        }

        public void HideNew(ItemInfo info)
        {
            inventoryModel.SetHideNew(info);
        }

        public bool IsNew(int tab)
        {
            List<ItemInfo> item = null;
            switch (tab)
            {
                case 0:
                    item = inventoryModel.itemList
                        .FindAll(a => a.ItemGroupType == ItemGroupType.Equipment && a.IsNew);
                    break;
                case 1:
                    item = inventoryModel.itemList
                        .FindAll(a => (a.ItemGroupType == ItemGroupType.Card) && a.IsNew);
                    break;
                case 2:
                    item = inventoryModel.itemList
                        .FindAll(a => (a.ItemGroupType == ItemGroupType.ProductParts) && a.IsNew);
                    break;
                case 3:
                    item = inventoryModel.itemList
                        .FindAll(a => a.ItemGroupType == ItemGroupType.ConsumableItem && a.IsNew);
                    break;
                case 4:
                    item = inventoryModel.itemList
                        .FindAll(a => a.ItemGroupType == ItemGroupType.Costume && a.IsNew);
                    break;
            }
            return item != null && item.Count > 0;
        }

        public void OnSelect(ItemInfo info)
        {
            if (IsDisassembleMode)
            {
                SelectDisassemble(info);
            }
            else
            {
                if (info.IsNew)
                {
                    HideNew(info);
                }
                UI.Show<UICostumeInfo>().Set(info.ItemNo);
            }
        }

        public TweenAlpha TweenAlpha => view.GetCostumeView().publicTweenAlpha;
    }
}