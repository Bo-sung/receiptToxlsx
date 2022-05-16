using Sfs2X.Entities.Data;

namespace Ragnarok
{
    public class ResponseAwaiter : TaskAwaiter<Response>
    {
        public readonly ISFSObject sendParam;

        public ResponseAwaiter(ISFSObject sendParam)
        {
            this.sendParam = sendParam;
        }
    }
}