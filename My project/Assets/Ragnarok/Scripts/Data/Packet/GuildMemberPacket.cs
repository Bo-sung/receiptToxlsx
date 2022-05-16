namespace Ragnarok
{
    public class GuildMemberPacket : IPacket<Response>
    {
        public int uid;
        public int cid;
        public string name;
        public byte job;
        public byte gender;
        public short job_level;
        public int guild_emblem;
        public byte guild_position;
        public string hex_cid;
        public int guild_remain_point;
        public int guild_donate_point;
        public long login_time;
        public byte connect;
        public bool attend;
        public int guild_out_remain_time; // 길드 탈퇴가능 남은 시간
        public int profileId;
        public int batlteScore;

        void IInitializable<Response>.Initialize(Response response)
        {
            uid = response.GetInt("2");
            cid = response.GetInt("3");
            name = response.GetUtfString("4");
            job = response.GetByte("5");
            gender = response.GetByte("6");
            job_level = response.GetShort("7");
            guild_emblem = response.GetInt("8");
            guild_position = response.GetByte("9");
            hex_cid = response.GetUtfString("10");
            guild_remain_point = response.GetInt("a");
            guild_donate_point = response.GetInt("b");
            login_time = response.GetLong("d");
            connect = response.GetByte("e");
            attend = response.GetBool("g");
            guild_out_remain_time = response.GetInt("ot");
            profileId = response.GetInt("11");
            batlteScore = response.GetInt("12");
        }
    }
}
