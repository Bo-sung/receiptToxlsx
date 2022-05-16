namespace Ragnarok
{
    public class GameServerPacket : IPacket<Response>
    {
        public string connectIP;
        public int connectPort;
        public int updateKey;
        public int severKey;
        public int zoneIndex;
        public string tossResourceUrl;

        void IInitializable<Response>.Initialize(Response response)
        {
            connectIP = response.GetUtfString("1");
            connectPort = response.GetInt("2");
            updateKey = response.GetInt("3");
            severKey = response.GetInt("4");
            zoneIndex = response.GetInt("5");
            tossResourceUrl = response.ContainsKey("c") ? response.GetUtfString("c") : string.Empty;
        }
    }
}