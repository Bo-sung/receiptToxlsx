namespace Ragnarok
{
    public sealed class GuildSkillPacket : IPacket<Response>, GuildModel.IGuildSkill
    {
        public int GuldId { get; private set; }
        public int SkillId { get; private set; }
        public int Exp { get; private set; }
        public int Level { get; private set; }

        public void Initialize(Response response)
        {
            GuldId = response.GetInt("1");
            SkillId = response.GetInt("2");
            Exp = response.GetInt("3");
            Level = response.GetByte("4");
        }
    }
}