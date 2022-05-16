namespace Ragnarok
{
    public sealed class EventBuffPacket : IPacket<Response>
    {
        public EventBuff eventBuff;
        public long leftTime;

        void IInitializable<Response>.Initialize(Response t)
        {
            eventBuff = t.GetPacket<EventBuff>("a");
            leftTime = t.GetLong("b");
        }
    }

    public sealed class EventBuff : IPacket<Response>
    {
        public long start_dt;
        public long end_dt;
        public short exp_rate;
        public short job_exp_rate;
        public short item_drop_rate;
        public short piece_drop_rate;
        public short gold_rate;
        public string title;

        void IInitializable<Response>.Initialize(Response t)
        {
            start_dt = t.GetLong("1");
            end_dt = t.GetLong("2");
            exp_rate = t.GetShort("3");
            job_exp_rate = t.GetShort("4");
            item_drop_rate = t.GetShort("5");
            piece_drop_rate = t.GetShort("6");
            gold_rate = t.GetShort("7");
            title = t.GetUtfString("8");
        }
    }
}