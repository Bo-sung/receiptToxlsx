namespace Ragnarok
{
    /// <summary>
    /// 첫구매 후 다음 첫구매까지 남은 시간 정보
    /// </summary>
    public class UserFirstGoodsPacket : IPacket<Response>
    {
        public int shopId;
        public long remainTime;

        public void Initialize(Response response)
        {
            shopId = response.GetInt("1");
            remainTime = response.GetLong("2");
        }
    }

    /// <summary>
    /// 매일매일 패키지 정보
    /// </summary>
    public class UserEverydayGoodsPacket : IPacket<Response>
    {
        public int shopId; //상점ID
        public bool isReward; // 오늘 보상을 받을 수 있다면 true, 이미 받았다면 false
        public int getCount; // 값이 -1일 경우 28일차 보상을 전부 받은 상태이다.

        // isReward가 true이고 getCount이 -1일 경우 재구입이 가능한상태이다.
        public void Initialize(Response response)
        {
            shopId = response.GetInt("1");
            isReward = response.GetBool("2");
            getCount = response.GetInt("3");
        }
    }
}
