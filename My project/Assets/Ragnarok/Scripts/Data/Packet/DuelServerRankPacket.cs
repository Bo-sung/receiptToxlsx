namespace Ragnarok
{
    public class DuelServerRankPacket : IPacket<Response>
    {
        public int serverRank;

        void IInitializable<Response>.Initialize(Response response)
        {
            serverRank = response.GetInt("1");
        }
    }
}