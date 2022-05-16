namespace Ragnarok
{
    public class RankPacket
    {
        /// <summary>
        /// 랭킹 목록
        /// </summary>
        public IRank[] ranks;

        /// <summary>
        /// 랭킹 업데이트 시간
        /// </summary>
        public long rankUpdateTime;

        public long myRank;
        public double? myScore;

        public string masterName;
        public int memberCount;
        public int maxMemberCount;
        public int myGuildExp;

        public void Initialize(Response response, RankType type)
        {
            if (response.ContainsKey("a"))
            {
                if (type == RankType.Guild || type == RankType.GuildBattle || type == RankType.EventGuildBattle)
                {
                    ranks = response.GetPacketArray<GuildRank>("a");
                }
                else
                {
                    ranks = response.GetPacketArray<Rank>("a");
                }
            }

            if (response.ContainsKey("t"))
                rankUpdateTime = response.GetLong("t");

            if (response.ContainsKey("r"))
                myRank = response.GetLong("r");

            if (response.ContainsKey("s"))
                myScore = response.GetDouble("s");

            if (type == RankType.GuildBattle || type == RankType.EventGuildBattle)
            {
                masterName = response.GetUtfString("ma");
                memberCount = response.GetInt("c");
                maxMemberCount = response.GetInt("m");
                myGuildExp = response.GetInt("e");
            }
        }
    }
}
