namespace Ragnarok
{
    public class CharacterSkillData : IPacket<Response>, SkillModel.ISkillValue
    {
        public long no;
        public int skill_id;
        public short skill_id_lv; // 0일 경우에는 보유하지 않은 스킬
        public byte order_id;
        public int change_skill_id; // 변경된 스킬 아이디

        bool SkillModel.ISkillValue.IsInPossession => skill_id_lv > 0;
        long SkillModel.ISkillValue.SkillNo => no;
        int SkillModel.ISkillValue.SkillId => skill_id;
        int SkillModel.ISkillValue.SkillLevel => skill_id_lv;
        int SkillModel.ISkillValue.OrderId => order_id;
        int SkillModel.ISkillValue.ChangeSkillId => change_skill_id;

        void IInitializable<Response>.Initialize(Response response)
        {
            no = response.GetLong("1");
            skill_id = response.GetInt("2");
            skill_id_lv = response.GetShort("3");
            order_id = response.GetByte("4");
            change_skill_id = response.GetInt("5");
        }
    }
}