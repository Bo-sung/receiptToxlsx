using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public class CharacterSkillSlotData : IPacket<Response>, SkillModel.ISlotValue
    {
        private ObscuredLong slotNo;
        private ObscuredLong skillNo; // CharacterSkillData 에서 참조할 id (0일 경우 비어있는 슬롯)
        private ObscuredByte order_id; // 1~4 (처음에는 최대2, 스킬 슬롯 확장하면 최대4까지 늘어날 수 있다) : BasisType 참조
        private ObscuredByte is_use; // 전투 중 자동 스킬 사용 여부 (0: 사용안함, 1: 사용중)

        long SkillModel.ISlotValue.SlotNo => slotNo;
        long SkillModel.ISlotValue.SkillNo => skillNo;
        int SkillModel.ISlotValue.SlotIndex => order_id;
        bool SkillModel.ISlotValue.IsAutoSkill => is_use == 1;

        void IInitializable<Response>.Initialize(Response response)
        {
            slotNo = response.GetLong("1");
            skillNo = response.GetLong("2");
            order_id = response.GetByte("3");
            is_use = response.GetByte("4");
        }

        public void Initialize(long slotNo, long skillNo, int slotIndex, bool isAutoUse)
        {
            const byte UNUSE_FLAG = 0;
            const byte USE_FLAG = 1;

            this.slotNo = slotNo;
            this.skillNo = skillNo;
            order_id = (byte)slotIndex;
            is_use = isAutoUse ? USE_FLAG : UNUSE_FLAG;
        }
    }
}