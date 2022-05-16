using System.Threading.Tasks;

namespace Ragnarok
{
    /// <see cref="UIAutoEquip"/>
    public class AutoEquipPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
            void ResetTimer();
        }

        IView view;
        InventoryModel invenModel;
        BetterList<EquipmentItemInfo> recommendItemList;
        EquipmentItemInfo lastRecommendItem;

        bool isEquiping;

        public override void AddEvent()
        {
            invenModel.OnObtainEquipment += OnObtainEquipment;
            invenModel.OnUpdateItem += UpdateRecommendingItem;
            invenModel.OnUpdateEquipment += UpdateRecommendingItem;

        }

        public override void RemoveEvent()
        {
            invenModel.OnObtainEquipment -= OnObtainEquipment;
            invenModel.OnUpdateItem -= UpdateRecommendingItem;
            invenModel.OnUpdateEquipment -= UpdateRecommendingItem;
        }

        public AutoEquipPresenter(IView view)
        {
            this.view = view;

            recommendItemList = new BetterList<EquipmentItemInfo>();
            lastRecommendItem = null;
            invenModel = Entity.player.Inventory;
            isEquiping = false;
        }

        /// <summary>
        /// UI에 표시할 첫번째 추천 아이템의 정보 반환
        /// </summary>
        public ItemInfo GetRecommendItem()
        {
            if (recommendItemList.size == 0)
                return null;

            return recommendItemList[0];
        }

        /// <summary>
        /// 전투력 변동치 반환
        /// </summary>
        public int GetAttackPowerDifference()
        {
            if (recommendItemList.size == 0)
                return 0;

            EquipmentItemInfo newItem = recommendItemList[0];
            int newItemAP = newItem.GetAttackPower();
            EquipmentItemInfo equippedItem = Find(newItem.SlotType);
            int equippedItemAP = equippedItem ? equippedItem.GetAttackPower() : 0;

            return newItemAP - equippedItemAP;
        }

        private EquipmentItemInfo Find(ItemEquipmentSlotType slotType)
        {
            ItemInfo[] equippedItems = invenModel.GetEquippedItems();
            foreach (var item in equippedItems)
            {
                if (item.SlotType == slotType)
                    return item as EquipmentItemInfo;
            }

            return null;
        }

        public async void OnClickedBtnEquip()
        {
            if (recommendItemList.size == 0)
                return;

            if (isEquiping) // 장착 요청이 완료되기 전에, 동일한 장비를 여러번 장착요청 할 수 없게끔.
                return;

            isEquiping = true;

            EquipmentItemInfo equipment = recommendItemList[0];
            recommendItemList.RemoveAt(0);
            await RequestItemEquip(equipment); // 장착 요청 
            // (장착템의 변동이 생기면서 자동으로 Update될 것이다.)
            //PutNextItem(); // 다음 아이템 가져오기
            UpdateRecommendingItem();

            isEquiping = false;
        }

        /// <summary>
        /// 아이템 장착
        /// </summary>
        public async Task RequestItemEquip(ItemInfo equipment)
        {
            if (equipment == null)
                return;

            await invenModel.RequestItemEquip(equipment);
        }


        /// <summary>
        /// 다음 아이템 항목 가져오기
        /// </summary>
        public void PutNextItem()
        {
            if (recommendItemList.size == 0)
                return;

            recommendItemList.RemoveAt(0); // 현재 아이템 항목 제거.
            UpdateRecommendingItem(); // 추천 유효성 검사 후 업데이트
        }

        /// <summary>
        /// 현재 출력중인 아이템이 유효한지 체크하고 업데이트한다.
        /// 유효성 검사 목록 : 1)해당 아이템 존재 유무, 2)장착 여부. 3)장착중인 아이템과의 전투력 비교.
        /// </summary>
        public void UpdateRecommendingItem()
        {
            // 유효하지 않은 추천템들을 들어오는 순서대로 삭제 처리한다.
            while (recommendItemList.size != 0)
            {
                EquipmentItemInfo newItem = recommendItemList[0];

                // 아이템 소유 여부 체크
                bool isNewItemExist = invenModel.GetItemInfo(newItem.ItemNo);
                if (!isNewItemExist) // 해당 아이템을 미 소유중이면 제거.
                {
                    recommendItemList.RemoveAt(0);
                    continue;
                }

                // 전투력 비교 

                // 무기일 경우에는 직업추천타입 여부에 따라서 전투력 비교가 생략될 수 있음.
                if (newItem.SlotType == ItemEquipmentSlotType.Weapon)
                {
                    bool isEquippedAppropriateType = invenModel.IsJobRecommendClassType(Entity.player.battleItemInfo.WeaponType, Entity.player.Character.Job);
                    bool isNewItemAppropriateType = invenModel.IsJobRecommendClassType(newItem.ClassType, Entity.player.Character.Job);

                    if (isEquippedAppropriateType && !isNewItemAppropriateType) // 현재 무기가 직업추천이고, 새 무기가 그렇지 않으면 그냥 탈락.
                    {
                        recommendItemList.RemoveAt(0);
                        continue;
                    }
                    else if (!isEquippedAppropriateType && isNewItemAppropriateType) // 현재 무기가 직업추천이 아닌데, 새 무기가 직업추천이면 전투력 비교 없이 통과.
                    {
                        break;
                    }
                }

                // 그 외의 경우는 전투력 비교.
                int attackPowerDiff = GetAttackPowerDifference();
                if (attackPowerDiff <= 0)
                {
                    recommendItemList.RemoveAt(0);
                    continue;
                }

                break;
            }

            //// 현재 선택된 추천템과 동일한 파츠의 장비가 큐에 있으면, 
            //// 그 중 가장 전투력이 높은 장비를 선택해서 교체한다. (나머지는 큐에서 전부 삭제)
            //if (recommendItemList.size > 0)
            //{
            //    // 추천 아이템과 같은 슬롯타입의 장비들 리스트 구하기
            //    List<EquipmentItemInfo> sameSlotTypeList = GetListBySlotType(recommendItemList[0].SlotType);

            //    // 현재 슬롯타입이 무기면, 직업추천 장비

            //    // 그 중에서 가장 강한 아이템 구하기
            //    var strongestInList = GetStrongestInList(sameSlotTypeList);

            //    // 동일 종류 아이템 전부 큐에서 제거 후 가장 센 아이템만 첫번째에 추가.
            //    foreach (var item in sameSlotTypeList)
            //    {
            //        recommendItemList.Remove(item);
            //    }
            //    recommendItemList.Insert(0, strongestInList);
            //}


            if (recommendItemList.size == 0 || lastRecommendItem != recommendItemList[0])
            {
                view.ResetTimer();
            }
            view.Refresh();

            this.lastRecommendItem = recommendItemList.size != 0 ? recommendItemList[0] : null;
        }

        /// <summary>
        /// 장비 획득 이벤트
        /// </summary>
        void OnObtainEquipment(EquipmentItemInfo equipment)
        {
            // 장착 경고 메시지가 존재할 경우
            string equiqWarningMessage = equipment.GetEquiqWarningMessage(isPopupMessage: false);
            if (!string.IsNullOrEmpty(equiqWarningMessage))
                return;

            // 일단 리스트에 넣는다. 
            // 현재 장비보다 강한지 약한지는 이 아이템이 메인UI에 뜰 차례가 되었을 때 판단하고 삭제하든 말든 한다.
            // 도중에 내 장비 상황이 바뀔 가능성이 있기 때문.
            recommendItemList.Add(equipment);
            UpdateRecommendingItem();
        }
    }
}