namespace Ragnarok
{
    public class ArenaRankPacket : IPacket<Response>
    {
        public IRank[] ranks;
        public long rankUpdateTime; // 랭킹 업데이트 시간
        public long myRank;
        public double myScore;

        void IInitializable<Response>.Initialize(Response response)
        {
            ranks = response.GetPacketArray<Rank>("a");
            rankUpdateTime = response.GetLong("t");
            myRank = response.GetLong("r");
            myScore = response.GetDouble("s");
        }
    }
}
