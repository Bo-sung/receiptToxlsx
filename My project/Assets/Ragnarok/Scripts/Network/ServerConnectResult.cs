namespace Ragnarok
{
    public class ServerConnectResult
    {
        public enum Type
        {
            /// <summary>
            /// 성공
            /// </summary>
            Success,

            /// <summary>
            /// 인터넷 연결 상태 확인
            /// </summary>
            CheckInternetState,

            /// <summary>
            /// 서버 닫힘
            /// </summary>
            ServerClosed,

            /// <summary>
            /// 연결 해제
            /// </summary>
            Disconnect,

            /// <summary>
            /// 알수 없는 에러
            /// </summary>
            Unknown,
        }

        public readonly Type type;
        public readonly string message;

        public ServerConnectResult(Type type)
            : this(type, string.Empty)
        {
        }

        public ServerConnectResult(Type type, string message)
        {
            this.type = type;
            this.message = message;
        }
    }
}