namespace Ragnarok
{
    public sealed class EventQuizPacket : IPacket<Response>
    {
        public static readonly EventQuizPacket EMPTY = new EventQuizPacket();

        public int start_date;
        public int seq_Index; // 시퀀스 인덱스 (0부터 시작, 참고로 데이터의 시퀀스는 1부터 시작한다)

        void IInitializable<Response>.Initialize(Response response)
        {
            start_date = response.GetInt("1");
            seq_Index = response.GetInt("2");
        }
    }
}