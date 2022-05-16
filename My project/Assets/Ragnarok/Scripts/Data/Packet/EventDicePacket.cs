namespace Ragnarok
{
    public sealed class EventDicePacket : IPacket<Response>
    {
        public static readonly EventDicePacket EMPTY = new EventDicePacket();

        public int group_id;
        public int step;
        public int complete_count;
        public int complete_reward_step;
        public byte dice_double_state; // 1이면 더블

        void IInitializable<Response>.Initialize(Response response)
        {
            group_id = response.GetInt("1");
            step = response.GetInt("2");
            complete_count = response.GetInt("3");
            complete_reward_step = response.GetInt("4");
            dice_double_state = response.GetByte("5");
        }
    }
}