namespace Ragnarok
{
    public class WorldBossInfoPacket : IPacket<Response>
    {
        public int world_boss_id;
        public int max_hp;
        public int current_hp;
        public long remain_time;
        public int join_char_count;

        public void Initialize(Response response)
        {
            world_boss_id   = response.GetInt("1");
            max_hp          = response.GetInt("2");
            current_hp      = response.GetInt("3");
            remain_time     = response.GetLong("4");
            join_char_count = response.GetInt("5");

        }
    }
}
