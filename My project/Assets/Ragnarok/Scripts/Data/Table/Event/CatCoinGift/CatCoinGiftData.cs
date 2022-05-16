using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class CatCoinGiftData : CatCoinGiftView.IInput
    {
        public int Id { get; private set; }
        public RewardData RewardData { get; private set; }

        public int CurrentCount { get; private set; }
        public int CompleteCount { get; private set; }
        public bool CanReward { get; private set; }

        public System.Action<int> OnReward;

        public CatCoinGiftData(int id, RewardData reward)
        {
            Id = id;
            RewardData = reward;
        }

        public void SetData(int curCnt, int maxCnt, bool canReward)
        {
            CurrentCount = curCnt;
            CompleteCount = maxCnt;
            CanReward = canReward;
        }
    }
}