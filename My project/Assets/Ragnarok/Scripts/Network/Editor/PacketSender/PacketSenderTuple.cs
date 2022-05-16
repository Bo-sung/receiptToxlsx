#if UNITY_EDITOR
using Sfs2X.Entities.Data;

namespace Ragnarok
{
    public class PacketSenderTuple
    {
        private readonly Protocol protocol;
        public readonly string cmd;
        public readonly string text;

        public ISFSObject sended;
        public ISFSObject received;

        public PacketSenderTuple(Protocol protocol)
        {
            this.protocol = protocol;

            string temp = this.protocol.ToString();
            string[] results = temp.Split(':');
            cmd = results[0];
            text = results[1];
        }

        public string GetCommand()
        {
            return protocol.Key;
        }
    }
}
#endif