namespace Ragnarok
{
    public class ServerConnectAwaiter : TaskAwaiter<ServerConnectResult>
    {
        public void Complete(ServerConnectResult.Type type)
        {
            Complete(new ServerConnectResult(type), null);
        }

        public void Complete(ServerConnectResult.Type type, string message)
        {
            Complete(new ServerConnectResult(type), null);
        }
    }
}