namespace Ragnarok
{
    public class SpecialRoulettePacket : IPacket<Response>
    {
        public string gachaIds; // 룰렛판 가챠ID 목록 ( , 기준 )
        public long remainTime; // 이벤트 남은 시간
        public int useItemId; // 사용되는 아이템
        public string receivedGachIds; // 획득한 보상 목록 ( , 기준 )
        public int groupId; // S등급 보상 목록 가챠테이블 GroupId

        public void Initialize(Response response)
        {
            gachaIds = response.GetUtfString("1");
            remainTime = response.GetLong("2");
            useItemId = response.GetInt("3");
            receivedGachIds = response.GetUtfString("4");
            groupId = response.GetInt("5");
        }
    }
}
