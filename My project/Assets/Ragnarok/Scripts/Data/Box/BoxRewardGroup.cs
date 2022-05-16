using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    public class BoxRewardGroup : BoxRewardElement.IInput
    {
        private int giveCount; // 지급 횟수
        private int totalRate;
        private List<BoxRewardInfo> rewards;

        public BoxRewardGroup(int giveCount)
        {
            this.giveCount = giveCount;
            rewards = new List<BoxRewardInfo>();
        }

        public void AddReward(BoxRewardInfo boxRewardInfo)
        {
            rewards.Add(boxRewardInfo);
        }

        public void Sort()
        {
            rewards.Sort();
        }

        public void SetTotalRate()
        {
            totalRate = 0;
            for (int i = 0; i < rewards.Count; i++)
            {
                totalRate += rewards[i].GetRate();
            }
        }

        int BoxRewardElement.IInput.GiveCount => giveCount;
        int BoxRewardElement.IInput.TotalRate => totalRate;

        List<BoxRewardInfo> BoxRewardElement.IInput.GetRewards()
        {
            return rewards;
        }
    }
}