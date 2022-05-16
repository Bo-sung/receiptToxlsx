namespace Ragnarok
{
    public class GuildInfoPacket : IPacket<Response>
    {
        public int guild_id;
        public string name;
        public int emblem;
        public string master_name;
        public byte member_count;
        public string introduction;
        public string guild_notice;
        public byte level;
        public int exppoint;
        public int day_accrue_donate;
        public int useable_accrue_donate;
        public string hex_cid;
        public int master_uid;
        public int master_cid;
        public byte max_member_count;
        public byte is_auto_join;
        public int freeGuildNameChangeCount; // 길드명 무료 변경 남은 횟수 (0 이면 유료)

        void IInitializable<Response>.Initialize(Response response)
        {
            guild_id = response.GetInt("1");
            name = response.GetUtfString("2");
            emblem = response.GetInt("3");
            master_name = response.GetUtfString("4");
            member_count = response.GetByte("5");
            introduction = response.GetUtfString("6");
            guild_notice = response.GetUtfString("7");
            level = response.GetByte("8");
            exppoint = response.GetInt("9");
            day_accrue_donate = response.GetInt("11");
            useable_accrue_donate = response.GetInt("12");
            hex_cid = response.GetUtfString("16");
            master_uid = response.GetInt("17");
            master_cid = response.GetInt("18");
            max_member_count = response.GetByte("19");
            is_auto_join = response.GetByte("20");
            freeGuildNameChangeCount = response.GetInt("21");
        }
    }
}
