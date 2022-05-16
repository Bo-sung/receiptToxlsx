namespace Ragnarok
{
    public class BingoStateDecoratedData
    {
        public enum State
        {
            Normal, Checkable, Checked,
            Rewardable, Rewarded
        }

        public BingoData bingoData;
        public State state;
        public bool isInRewardableLine;

        public bool IsLineRewardData => bingoData.IsLineReward;
    }
}
