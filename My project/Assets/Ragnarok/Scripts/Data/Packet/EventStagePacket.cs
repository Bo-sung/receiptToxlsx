namespace Ragnarok
{
    public sealed class EventStagePacket : IPacket<Response>
    {
        public static readonly EventStagePacket EMPTY = new EventStagePacket();

        public int seq;
        public long remainTime;
        
        void IInitializable<Response>.Initialize(Response response)
        {
            seq = response.GetInt("1");
            remainTime = response.GetLong("2");
        }
    }
}