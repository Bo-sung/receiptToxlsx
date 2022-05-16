using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class DailyCheckInfo : DataInfo<LoginBonusData>
    {
        public int Day => data.day;

        public RewardData RewardData => data.rewardData;

        public string Name => RewardData.ItemName;

        public int Count => RewardData.Count;
    }
}
