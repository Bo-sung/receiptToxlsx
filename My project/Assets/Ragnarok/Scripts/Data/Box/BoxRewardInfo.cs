using Ragnarok.View;
using System;

namespace Ragnarok
{
    public class BoxRewardInfo : IComparable<BoxRewardInfo>, BoxRewardSlot.IInput
    {
        private RewardData rewardData;
        private int rate;
        private bool isFixed;

        public BoxRewardInfo(RewardData rewardData, int rate, bool isFixed)
        {
            this.rewardData = rewardData;
            this.rate = rate;
            this.isFixed = isFixed;
        }

        public int CompareTo(BoxRewardInfo other)
        {
            return other.rate - rate;
        }

        public int GetRate()
        {
            return rate;
        }

        private string GetItemName()
        {
            return rewardData.ItemName;
        }

        private string GetItemCount()
        {
            return rewardData.Count.ToString("N0");
        }        

        string BoxRewardSlot.IInput.ItemName => GetItemName();
        string BoxRewardSlot.IInput.ItemCount => GetItemCount();
        bool BoxRewardSlot.IInput.IsFixed => isFixed;
        int BoxRewardSlot.IInput.Rate => GetRate();
    }
}