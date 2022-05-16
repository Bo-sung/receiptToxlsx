using Sfs2X.Entities.Data;

namespace Ragnarok
{
    /// <summary>
    /// AuctionItemPacket 패킷
    /// </summary>
    public class AuctionItemPacket : IPacket<Response>
    {
        public ISFSObject sfsObj;

        public void Initialize(Response response)
        {
            sfsObj = response;
        }
    }
}