using Ragnarok.View;

namespace Ragnarok
{
    public sealed class GuildBattleHistoryPacket : IPacket<Response>, UIGuildHistoryElement.IInput
    {
        public int GuildId => 0;
        public string GuildName { get; private set; }
        public string CharacterName { get; private set; }
        public int Damage { get; private set; }
        public int Cid { get; private set; }
        public int exp;
        public int Emblem { get; private set; }
        public int Uid { get; private set; }

        public int MaxHp { get; private set; }
        public int GuildLevel { get; private set; }

        public void Initialize(Response response)
        {
            GuildName = response.GetUtfString("1");
            CharacterName = response.GetUtfString("2");
            Damage = response.GetInt("3");
            Cid = response.GetInt("4");
            exp = response.GetInt("5");
            Emblem = response.GetInt("6");
            Uid = response.GetInt("7");
        }

        public void SetMaxHp(int maxHp)
        {
            MaxHp = maxHp;
        }

        public void SetGuildLevel(int guildLevel)
        {
            GuildLevel = guildLevel;
        }
    }
}