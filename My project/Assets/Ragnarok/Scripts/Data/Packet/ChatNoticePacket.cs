namespace Ragnarok
{
    /// <summary>
    /// 채팅 공지사항 패킷
    /// </summary>
    public class ChatNoticePacket
    {
        public string noti_message;
        public int noti_version;

        public ChatNoticePacket(string msg, int version) => Initialize(msg, version);

        public void Initialize(string msg, int version)
        {
            noti_message = msg;
            noti_version = version;
        }
    }

}