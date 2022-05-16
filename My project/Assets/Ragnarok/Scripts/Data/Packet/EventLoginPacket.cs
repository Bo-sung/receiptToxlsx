namespace Ragnarok
{
    public class EventLoginPacket : IPacket<Response>
    {
        public int Group { get; private set; }
        public int Day { get; private set; }
        public RemainTime RemainTime { get; private set; }
        public bool IsReward { get; private set; } // 보상수령 ui표시 여부 // true:표시, false: 표시 안함

        public void Initialize(Response t)
        {
            Group = t.GetInt("1");
            Day = t.GetInt("2");
            RemainTime = t.GetLong("3");
            IsReward = t.GetBool("4");
        }
    }
}