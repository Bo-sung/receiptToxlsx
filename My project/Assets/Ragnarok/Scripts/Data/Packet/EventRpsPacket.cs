namespace Ragnarok
{
    public class EventRpsPacket : IPacket<Response>
    {
        public byte result; // 이벤트 결과
        public byte round; // 이벤트 단계

        void IInitializable<Response>.Initialize(Response response)
        {
            result = response.GetByte("1");
            round = response.GetByte("2");
        }
    }
}