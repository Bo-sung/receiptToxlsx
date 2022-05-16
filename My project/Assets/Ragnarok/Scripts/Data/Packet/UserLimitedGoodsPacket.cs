namespace Ragnarok
{
    /// <summary>
    /// 구매 제한 상품 구매 횟수
    /// </summary>
    public class UserLimitedGoodsPacket : IPacket<Response>
    {
        public int shopId;
        public int count;
        public long remainTime;

        public void Initialize(Response response)
        {
            shopId = response.GetInt("1");
            count = response.GetByte("2");
            remainTime = response.GetLong("3");
        }
    }

    public class PeriodProductPacket : IPacket<Response>
    {
        public int shopId;
        public long remainTime;

        public void Initialize(Response response)
        {
            shopId = response.GetInt("1");
            remainTime = response.GetLong("2");
        }
    }
}
