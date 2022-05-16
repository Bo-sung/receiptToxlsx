namespace Ragnarok
{
    public class ChristmasSkillData : SkillModel.ISkillValue, SkillModel.ISlotValue
    {
        private SkillData skillData;
        private int slotIndex;

        public long SkillNo => skillData.id;
        public int SkillId => skillData.SkillId;
        public int SkillLevel => skillData.lv;

        public long SlotNo => skillData.id;
        public int SlotIndex => slotIndex;

        public bool IsInPossession => true;
        public int OrderId => 0;
        public int ChangeSkillId => 0;
        public bool IsAutoSkill => false;

        public void Initialize(SkillData skillData, int slotIndex)
        {
            this.skillData = skillData;
            this.slotIndex = slotIndex;
        }
    }
}