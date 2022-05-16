using Sfs2X.Entities.Data;

namespace Ragnarok
{
    /// <summary>
    /// 최근 개인상점 등록 정보 패킷
    /// </summary>
    public class PrivateStoreItemPacket : IPacket<Response>
    {
        public ISFSObject sfsObj;

        public void Initialize(Response response)
        {
            sfsObj = response;
        }
    }
}