namespace Ragnarok
{
    public class GuildSimplePacket : IPacket<Response>
    {
        public int guild_id;
        public string name;
        public int emblem;
        public string master_name;
        public byte member_count;
        public string introduction;
        public byte level;
        public int exppoint;
        public byte max_member_count;
        public string hex_cid;
        public byte is_Auto_Join;

        public void Initialize(Response response)
        {
            guild_id         = response.GetInt("1");
            name             = response.GetUtfString("2");
            emblem           = response.GetInt("3");
            master_name      = response.GetUtfString("4");
            member_count     = response.GetByte("5");
            introduction     = response.GetUtfString("6");
            level            = response.GetByte("7");
            exppoint         = response.GetInt("8");
            max_member_count = response.GetByte("10");
            hex_cid          = response.GetUtfString("11");
            is_Auto_Join     = response.GetByte("12");
        }
    }
}
