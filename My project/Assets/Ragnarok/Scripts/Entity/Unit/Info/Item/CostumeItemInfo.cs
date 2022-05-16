using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public class CostumeItemInfo : ItemInfo
    {
        ObscuredByte itemPos;

        public override bool IsStackable => false;
        public override bool IsEquipped => EquippedSlotType != ItemEquipmentSlotType.None;
        public override ItemEquipmentSlotType SlotType => data.GetCostumeSlotType();
        public override ItemEquipmentSlotType EquippedSlotType
        {
            get => itemPos.ToEnum<ItemEquipmentSlotType>();
            set => itemPos = value.ToByteValue();
        }
        public override CostumeType CostumeType => data.class_type.ToEnum<CostumeType>();
        public override string PrefabName => data.prefab_name;
        public override int CostumeDataId => data.event_id;

        public override void SetItemInfo(int tier, int itemLevel, byte itemPos, long equippedCardNo1, long equippedCardNo2, long equippedCardNo3, long equippedCardNo4, bool isLock, int itemTranscend = 0, int itemChangedElement = 0, int itemElementLevel = 0)
        {
            this.itemPos = itemPos;
        }

        public override void Reload(bool isEquipCard)
        {
        }
    }
}