namespace Ragnarok
{
    public sealed class GuildBattleOpponentPacket : IPacket<Response>, UIGuildBattleElement.IInput
    {
        public string GuildName { get; private set; }
        public int CurHp { get; private set; }
        public int exp;
        public int GuildId { get; private set; }
        public int Emblem { get; private set; }

        public int GuildLevel { get; private set; }
        public int MaxHp { get; private set; }

        void IInitializable<Response>.Initialize(Response response)
        {
            GuildName = response.GetUtfString("1");
            CurHp = response.GetInt("2");
            exp = response.GetInt("3");
            GuildId = response.GetInt("4");
            Emblem = response.GetInt("5");
        }

        public void SetGuildLevel(int level)
        {
            GuildLevel = level;
        }

        public void SetMaxHp(int maxHp)
        {
            MaxHp = maxHp;
        }
    }
}