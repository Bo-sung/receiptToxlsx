namespace Ragnarok
{
    public class GuildJoinSubmitPacket : IPacket<Response>
    {
        public int seq;
        public int uid;
        public int cid;
        public string name;
        public byte job;
        public byte gender;
        public short job_level;
        public string hex_cid;
        public long insert_dt;

        void IInitializable<Response>.Initialize(Response response)
        {
            seq = response.GetInt("1");
            uid = response.GetInt("2");
            cid = response.GetInt("3");
            name = response.GetUtfString("4");
            job = response.GetByte("5");
            gender = response.GetByte("6");
            job_level = response.GetShort("7");
            hex_cid = response.GetUtfString("8");
            insert_dt = response.GetLong("9");
        }
    }
}
