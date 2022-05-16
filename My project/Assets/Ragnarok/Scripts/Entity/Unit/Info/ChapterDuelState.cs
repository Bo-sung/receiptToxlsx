using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Ragnarok
{
    public class ChapterDuelState
    {
        private readonly DuelRewardData[] rewardDatas;
        private readonly bool[] OwningAlphabets;
        private readonly bool[] validAlphabets;

        public int Chapter { get; private set; }
        public DuelRewardData CurDuelRewardData { get { return rewardDatas[Mathf.Min(RewardedCount, rewardDatas.Length - 1)]; } }
        public int RewardedCount { get; private set; }
        public string DuelWord { get; private set; }
        public IEnumerable<DuelRewardData> RewardDatas => rewardDatas;

        public ChapterDuelState(DuelRewardData[] rewardDatas)
        {
            Chapter = rewardDatas[0].check_value;
            this.rewardDatas = rewardDatas;
            OwningAlphabets = new bool[8];
            validAlphabets = new bool[8];
            RewardedCount = -1;
            DuelWord = null;
        }

        public bool IsOwningAlphabet(int index)
        {
            return OwningAlphabets[index];
        }

        public bool IsValidAlphabet(int index)
        {
            return validAlphabets[index];
        }

        public void InitOwningAndRewardState(int curOwningBit, int curRewardedCount)
        {
            for (int i = 0; i < 8; ++i)
                OwningAlphabets[i] = (curOwningBit & (1 << i)) > 0;
            
            if (RewardedCount != curRewardedCount)
            {
                RewardedCount = curRewardedCount;
                var data = CurDuelRewardData;

                DuelWord = string.Concat(data.GetWord());
                int worldBit = data.need_bit_type;

                for (int i = 0; i < 8; ++i)
                    validAlphabets[i] = (worldBit & (1 << i)) > 0;
            }
        }

        public void AddOwningBit(int newOwningBit)
        {
            for (int i = 0; i < 8; ++i)
                if ((newOwningBit & (1 << i)) > 0)
                    OwningAlphabets[i] = true;
        }

        public void AddOwningIndex(int newAlphabetIndex)
        {
            OwningAlphabets[newAlphabetIndex] = true;
        }
    }
}