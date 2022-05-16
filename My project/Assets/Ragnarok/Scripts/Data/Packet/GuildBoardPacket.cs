namespace Ragnarok
{
    public class GuildBoardPacket : IPacket<Response>
    {
        public string name;
        public string message;
        public long insert_dt;
        public int seq;
        public int message_id;
        public int cid;
        public int uid;
        public string hex_cid;

        void IInitializable<Response>.Initialize(Response response)
        {
            name = response.GetUtfString("1");
            message = response.GetUtfString("2");
            insert_dt = response.GetLong("3");
            seq = response.GetInt("4");
            message_id = response.GetInt("5");
            cid = response.GetInt("6");
            uid = response.GetInt("7");
            hex_cid = response.GetUtfString("8");
        }
    }
}
