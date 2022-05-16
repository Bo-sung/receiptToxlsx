namespace Ragnarok
{
    /// <summary>
    /// 테이밍 던전 오픈 시간 정보
    /// </summary>
    public class TamingDungeonTimePacket : IPacket<Response>
    {
        public long[] today_open_time; 
        public long currentServerTime; // 현 서버 시간

        public void Initialize(Response response)
        {
            today_open_time = response.GetLongArray("1");
            currentServerTime = response.GetLong("2");
        }
    }
}