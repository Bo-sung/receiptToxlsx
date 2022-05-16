namespace Ragnarok
{
    /// <summary>
    /// 챗모드(거래소, 일반, 길드챗 등)
    /// </summary>
    public class ChatModeInfo : IInfo
    {
        public bool IsInvalidData => false;

        public readonly ChatMode mode;

        /// <summary>
        /// [귓속말] CID
        /// </summary>
        public readonly int cid;

        /// <summary>
        /// [귓속말] UID
        /// </summary>
        public readonly int uid;

        /// <summary>
        /// [귓속말] 닉네임
        /// </summary>
        public readonly string nickname;

        public event System.Action OnUpdateEvent;

        public ChatModeInfo(ChatMode mode)
        {
            this.mode = mode;
        }

        public ChatModeInfo(ChatMode mode, int cid, int uid, string nickname)
        {
            this.mode = mode;

            this.cid = cid;
            this.uid = uid;
            this.nickname = nickname;
        }
    }
}