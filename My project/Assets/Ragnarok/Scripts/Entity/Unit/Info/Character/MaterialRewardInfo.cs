using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class MaterialRewardInfo : DataInfo<RewardData>
    {
        public RewardData Reward => data;
    }
}
