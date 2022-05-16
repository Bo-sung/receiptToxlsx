using Sfs2X.Entities.Data;

namespace Ragnarok
{
    public class ProtocolTuple
    {
        public readonly Protocol protocol;
        public readonly ISFSObject param;

        public ProtocolTuple(Protocol protocol, ISFSObject param)
        {
            this.protocol = protocol;
            this.param = param;
        }
    }
}