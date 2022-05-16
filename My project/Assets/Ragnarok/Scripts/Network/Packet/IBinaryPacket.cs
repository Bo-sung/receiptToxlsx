using Sfs2X.Util;

namespace Ragnarok
{
    public interface IBinaryPacket : IData
    {
        void Init(ByteArray byteArray);
    }
}