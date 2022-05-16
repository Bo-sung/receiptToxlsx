namespace Ragnarok
{
    /// <summary>
    /// 테이밍 던전 정보
    /// </summary>
    public class TamingDungeonInfoPacket : IPacket<Response>
    {
        public int tamingId; // 테이밍 테이블 ID
        public bool isOpen; // 테이밍 던전 오픈여부
        public RemainTime remainTime; // 진행중일떄는 남은시간, 진행중이 아닐때는 다음 오픈까지 남은시간

        public void Initialize(Response response)
        {
            tamingId = response.GetInt("1");
            isOpen = response.GetBool("2");
            remainTime = response.GetLong("3");
        }
    }
}