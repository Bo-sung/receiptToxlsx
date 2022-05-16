namespace Ragnarok
{
    /// <summary>
    /// 보상 데이터
    /// </summary>
    public class RewardPacket : IPacket<Response>
    {
        public byte rewardType;
        public int rewardValue;
        public int rewardOption;
        public int rewardCount;

        public RewardPacket() : this(0, 0, 0, 0)
        {
        }

        public RewardPacket(byte rewardType, int rewardValue, int rewardOption, int rewardCount)
        {
            this.rewardType = rewardType;
            this.rewardValue = rewardValue;
            this.rewardOption = rewardOption;
            this.rewardCount = rewardCount;
        }

        void IInitializable<Response>.Initialize(Response obj)
        {
            rewardType = obj.GetByte("1");
            rewardValue = obj.GetInt("2");
            rewardOption = obj.GetInt("3");
            rewardCount = obj.GetInt("4");
        }
    }
}
