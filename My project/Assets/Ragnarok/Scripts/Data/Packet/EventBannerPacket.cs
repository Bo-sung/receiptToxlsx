namespace Ragnarok
{
    public class EventBannerPacket : IPacket<Response>
    {
        public int seq; // 배너 고유번호
        public string url;
        public string description;
        public long startTime;
        public long endTime;
        public string textureName;
        public byte pos; // 배너 우선순위 (순서)
        public long remain_time; // 남은 시간 (0 아래로 떨어지면 배너 비활성화)

        void IInitializable<Response>.Initialize(Response response)
        {
            seq = response.GetInt("1");
            url = response.GetUtfString("2");
            description = response.GetUtfString("3");
            startTime = response.GetLong("4");
            endTime = response.GetLong("5");
            textureName = response.GetUtfString("6");
            pos = response.GetByte("15");
            remain_time = response.GetLong("16");
        }
    }
}