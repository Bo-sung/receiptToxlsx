namespace Ragnarok
{
    /// <summary>
    /// 귓속말 상대에 대한 정보
    /// </summary>
    public class WhisperInfo
    {
        public int cid;
        public int uid;
        public string nickname;

        public WhisperInfo(int cid, int uid, string nickname)
        {
            this.cid = cid;
            this.uid = uid;
            this.nickname = nickname;
        }
    }
}