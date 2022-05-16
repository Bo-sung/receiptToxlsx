namespace Ragnarok
{
    public class GuildRequestSimplePacket : IPacket<Response>
    {
        public int seq; // 정렬 기준
        public int guild_id;
        public string guild_name;
        public int emblem;
        public string master_name;
        public byte guild_level;

        public void Initialize(Response response)
        {
            seq = response.GetInt("1");
            guild_id = response.GetInt("2");
            guild_name = response.GetUtfString("3");
            emblem = response.GetInt("4");
            master_name = response.GetUtfString("5");
            guild_level = response.GetByte("6");
        }        
    }
}
