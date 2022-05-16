namespace Ragnarok
{
    public class GateRoomPacket : IPacket<Response>
    {
        public int channelId;
        public int leaderCid;
        public GateRoomPlayerPacket[] players;

        void IInitializable<Response>.Initialize(Response response)
        {
            channelId = response.GetInt("1");
            leaderCid = response.GetInt("2");
            players = response.GetPacketArray<GateRoomPlayerPacket>("3");
        }
    }
}