using System.Collections.Generic;

namespace Ragnarok
{
    public class EquipmentItemAttackPowerInfo : BetterList<EquipmentItemInfo>
    {
        private InventoryModel invenModel;
        private readonly Dictionary<long, int> attackPowerDic; // Key: ItemNo, Value: AttackPower

        /// <summary>
        /// 장비 별 전투력 테이블 (플레이어의 스탯에 따라 변동하므로 실제적인 전투력은 아니고, 장비 간의 비교를 위한 수치)
        /// </summary>
        public Dictionary<long, int> AttackPowerDic => attackPowerDic;

        /// <summary>
        /// 전투력 갱신 완료 이벤트
        /// </summary>
        public event System.Action OnAttackPowerTableUpdate; 

        public EquipmentItemAttackPowerInfo(InventoryModel invenModel)
        {
            this.invenModel = invenModel;

            attackPowerDic = new Dictionary<long, int>(LongEqualityComparer.Default);

            UpdateAllEquipmentsAttackPower(); // 최초 1회 전투력 전체 갱신
        }

        public void AddEvent()
        {
            invenModel.OnObtainEquipment += OnObtainEquipment;
            invenModel.OnUpdateEquipment += OnUpdateEquipment;
            invenModel.OnUpdateItem += OnUpdateItem;
        }

        public void RemoveEvent()
        {
            invenModel.OnObtainEquipment -= OnObtainEquipment;
            invenModel.OnUpdateEquipment -= OnUpdateEquipment;
            invenModel.OnUpdateItem -= OnUpdateItem;
        }

        /// <summary>
        /// 해당 슬롯타입의 장비목록 반환
        /// </summary>
        public EquipmentItemInfo[] GetEquipmentsBySlotType(ItemEquipmentSlotType slotType)
        {
            List<EquipmentItemInfo> equipmentInfos = invenModel.itemList.FindAll(e => e is EquipmentItemInfo).ConvertAll(e => e as EquipmentItemInfo);
            return equipmentInfos.FindAll(e => e.SlotType == slotType).ToArray();
        }

        /// <summary>
        /// 아이템 업데이트 (불필요할 수도 있음)
        /// </summary>
        private void OnUpdateItem()
        {
            return; // 매번 아이템이 업데이트 될 때마다 전체 장비를 체크하면 부하가 걸릴 수 있다 .. 일단은 제외
        }

        /// <summary>
        /// 장비 획득
        /// </summary>
        private void OnObtainEquipment(EquipmentItemInfo newItem)
        {
            // 모든 장비에 대한 전투력 업데이트.
            // 획득 당시의 스탯이 추산 전투력에 영향을 주므로 한 아이템의 전투력만 업데이트 하면 제대로 비교가 되지 않는다.
            UpdateAllEquipmentsAttackPower();
        }

        /// <summary>
        /// 장비 업데이트 (강화, 장착/해제 등)
        /// </summary>
        private void OnUpdateEquipment()
        {
            // 업데이트 된 장비를 알 수 없으므로, 모든 장비 체크
            UpdateAllEquipmentsAttackPower();
        }

        /// <summary>
        /// 모든 장비의 전투력 갱신
        /// </summary>
        private void UpdateAllEquipmentsAttackPower()
        {
            List<EquipmentItemInfo> equipmentInfos = invenModel.itemList.FindAll(e => e is EquipmentItemInfo).ConvertAll(e => e as EquipmentItemInfo);

            foreach (var equipmentInfo in equipmentInfos)
            {
                attackPowerDic[equipmentInfo.ItemNo] = equipmentInfo.GetAttackPower(); // 전투력 갱신
            }

            OnAttackPowerTableUpdate?.Invoke();
        }
    }
}