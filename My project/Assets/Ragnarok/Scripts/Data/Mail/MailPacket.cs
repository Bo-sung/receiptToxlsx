namespace Ragnarok
{
    /// <summary>
    /// 우편 정보
    /// </summary>
    public sealed class MailPacket : IPacket<Response>
    {
        public int id;
        public int cid;
        public int uid;
        public string message;
        public byte rewardType;
        public int rewardValue;
        public int rewardCount;
        public int rewardEventId;
        public int rewardOption; // 상점탭에서 상점 ID로 쓰임
        public long insertTime;

        void IInitializable<Response>.Initialize(Response response)
        {
            id = response.GetInt("1");
            cid = response.GetInt("2");
            uid = response.GetInt("3");
            message = response.GetUtfString("4");
            rewardType = response.GetByte("5");
            rewardValue = response.GetInt("6");
            rewardCount = response.GetInt("7");
            rewardEventId = response.GetInt("8");
            rewardOption = response.GetInt("9");
            insertTime = response.GetLong("10");
        }
    }
}