using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class TreeRewardSlot : UIInfo<MaterialRewardInfo>
    {
        [SerializeField] UIRewardHelper reward;

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            reward.SetData(info.Reward);
        }
    }
}
