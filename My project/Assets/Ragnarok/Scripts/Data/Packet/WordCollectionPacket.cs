namespace Ragnarok
{
    public class WordCollectionPacket : IPacket<Response>
    {
        public static readonly WordCollectionPacket EMPTY = new WordCollectionPacket();

        public long remainTime { get; private set; } // 이벤트 남은 시간
        public int seasionNo { get; private set; } // 이벤트 시즌정보
        public int completeRewardStep { get; private set; } // 보상 받은 회차 정보

        public void Initialize(Response response)
        {
            remainTime = response.GetLong("1");
            seasionNo = response.GetInt("2");
            completeRewardStep = response.GetInt("3");
        }
    }
}
