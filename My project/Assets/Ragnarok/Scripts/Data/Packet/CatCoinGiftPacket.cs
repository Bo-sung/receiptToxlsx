namespace Ragnarok
{
    public class CatCoinGiftPacket : IPacket<Response>
    {
        public int CompleteCount { get; private set; }
        public bool TodayRewarded { get; private set; }

        public void Initialize(Response response)
        {
            CompleteCount = response.GetInt("1");
            TodayRewarded = response.GetInt("2") == 1;
        }
    }
}