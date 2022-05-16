namespace Ragnarok
{
    public class GuildRank : IPacket<Response>, IRank
    {
        public int guildId;
        public long guildScore; // Score
        public short guildRank;
        public int maxMemberCount;
        public int curMemberCount;
        public int emblem;
        public string guildName;
        public string guildMasterName;
        public int guildExp;

        public void Initialize(Response response)
        {
            guildId = response.GetInt("1");
            guildScore = response.GetInt("2");
            guildRank = response.GetShort("3");
            maxMemberCount = response.GetInt("4");
            curMemberCount = response.GetInt("5");
            emblem = response.GetInt("6");
            guildName = response.GetUtfString("7");
            guildMasterName = response.GetUtfString("8");
            guildExp = response.GetInt("9");

            if (response.ContainsKey("10"))
                guildScore = response.GetLong("10");
        }

        /// <summary>
        /// 클라가 집계하는 랭킹
        /// </summary>
        public void SetRanking(short ranking)
        {
            guildRank = ranking;
        }
    }
}
