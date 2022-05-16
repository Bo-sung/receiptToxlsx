namespace Ragnarok
{
    public sealed class GuildBattleBuffPacket : IPacket<Response>, ISkillDataKey
    {
        public int SkillId { get; private set; }
        public int TotalExp { get; private set; }

        public int Id => SkillId;
        public int Level { get; private set; }

        void IInitializable<Response>.Initialize(Response response)
        {
            SkillId = response.GetInt("1");
            TotalExp = response.GetInt("2");
        }

        public void SetLevel(int level)
        {
            Level = level;
        }
    }
}