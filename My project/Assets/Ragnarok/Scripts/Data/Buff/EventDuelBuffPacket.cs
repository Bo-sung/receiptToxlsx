namespace Ragnarok
{
    public sealed class EventDuelBuffPacket : IPacket<Response>
    {
        public int buf_script; //스크립트 id
        public int buf_gold;
        public int buf_exp;
        public int buf_jobexp;
        public int buf_item;

        public long remain_time; //종료까지 남은시간

        void IInitializable<Response>.Initialize(Response t)
        {
            buf_script = t.GetInt("1");
            buf_gold = t.GetInt("2");
            buf_exp = t.GetInt("3");
            buf_jobexp = t.GetInt("4");
            buf_item = t.GetInt("5");

            remain_time = t.GetLong("6");
        }
    }
}