namespace Ragnarok
{
    public sealed class CustomerRewardPacket : IPacket<Response>
    {
        public int NormalStep { get; private set; }
        public RemainTime NormalRemainTime { get; private set; }
        public int PremiumStep { get; private set; }
        public RemainTime PremiumRemainTime { get; private set; }

        public void Initialize(Response response)
        {
            NormalStep = response.GetInt("1");
            NormalRemainTime = response.GetLong("2");
            PremiumStep = response.GetInt("3");
            PremiumRemainTime = response.GetLong("4");
        }
    }
}