namespace Ragnarok
{
    public class SkillValuePacket : IPacket<Response>, SkillModel.ISkillValue
    {
        public int skill_id;
        public int skill_level;
        public byte pos;

        private long skillNo;

        bool SkillModel.ISkillValue.IsInPossession => skill_level > 0;
        long SkillModel.ISkillValue.SkillNo => skillNo;
        int SkillModel.ISkillValue.SkillId => skill_id;
        int SkillModel.ISkillValue.SkillLevel => skill_level;
        int SkillModel.ISkillValue.OrderId => 0;
        int SkillModel.ISkillValue.ChangeSkillId => 0;

        void IInitializable<Response>.Initialize(Response response)
        {
            skill_id = response.GetInt("1");
            skill_level = response.GetInt("2");
            pos = response.GetByte("3");
        }

        public void SetSkillNo(long skillNo)
        {
            this.skillNo = skillNo;
        }
    }
}