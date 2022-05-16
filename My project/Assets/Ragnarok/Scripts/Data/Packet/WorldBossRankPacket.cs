namespace Ragnarok
{
    public class WorldBossRankPacket : IPacket<Response>
    {
        public int uid;
        public int cid;
        public int ranking;
        public int score;
        public string char_name;
        public int world_boss_id;

        public void Initialize(Response response)
        {
            uid           = response.GetInt("1");
            cid           = response.GetInt("2");
            ranking       = response.GetShort("3");
            score         = response.GetInt("4");
            char_name     = response.GetUtfString("5");
            world_boss_id = response.GetInt("6");
        }
    }
}