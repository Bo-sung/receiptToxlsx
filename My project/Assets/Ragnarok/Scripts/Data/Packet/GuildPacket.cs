namespace Ragnarok
{
    public class GuildPacket : IPacket<Response>
    {
        public int guild_id;
        public string guild_name;
        public int guild_emblem;
        public byte guild_position;
        public int guild_coin;
        public byte guild_quest_reward_cnt; // 길드 퀘스트 보상 받은 횟수
        public long guild_skill_buy_dt; // 길드 스킬 구입 쿨타임
        public byte guild_skill_buy_count; // 오늘 냥다래로 구입한 길드스킬 구입 카운드

        void IInitializable<Response>.Initialize(Response response)
        {
            guild_id = response.GetInt("1");
            guild_name = response.GetUtfString("2");
            guild_emblem = response.GetInt("3");
            guild_position = response.GetByte("4");
            guild_coin = response.GetInt("5");
            guild_quest_reward_cnt = response.GetByte("6");
            guild_skill_buy_dt = response.GetLong("7");
            guild_skill_buy_count = response.GetByte("8");
        }
    }
}
