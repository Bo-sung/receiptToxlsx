namespace Ragnarok
{
    /// <summary>
    /// 카프라 운송 패킷
    /// </summary>
    public sealed class KafraDeliveryPacket : IPacket<Response>
    {
        public int Id { get; private set; } 
        public int Count{ get; private set; }

        void IInitializable<Response>.Initialize(Response response)
        {
            Id = response.GetInt("1");
            Count = response.GetInt("2");
        }
    }
}