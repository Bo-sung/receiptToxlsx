namespace Ragnarok
{
    public sealed class RewardInfo : IInfo, System.IEquatable<RewardInfo>
    {
        public readonly RewardData data;

        bool IInfo.IsInvalidData => false;

        event System.Action IInfo.OnUpdateEvent
        {
            add { }
            remove { }
        }

        public RewardInfo(RewardPacket packet)
            : this(packet.rewardType, packet.rewardValue, packet.rewardCount, packet.rewardOption)
        {
        }

        public RewardInfo(RewardType rewardType, int rewardValue, int rewardCount, int rewardOption = 0)
            : this((byte)rewardType, rewardValue, rewardCount, rewardOption)
        {
        }

        public RewardInfo(byte rewardType, int rewardValue, int rewardCount, int rewardOption = 0)
        {
            data = new RewardData(rewardType, rewardValue, rewardCount, rewardOption);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RewardInfo other))
                return false;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return data.GetHashCode();
        }

        public bool Equals(RewardInfo other)
        {
            if (other == null)
                return false;

            return data.Equals(other.data);
        }
    }
}