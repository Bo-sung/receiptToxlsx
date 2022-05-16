using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class CatCoinRewardSubSlot : UIInfo<CatCoinRewardInfo>, IAutoInspectorFinder
    {
        [SerializeField] UILabelHelper labelTime;
        [SerializeField] UIRewardHelper itemBase;
        [SerializeField] UILabelHelper labelReward;

        protected override void Refresh()
        {
            if (IsInvalid())
                return;
            labelTime.Text = LocalizeKey._9007.ToText().Replace("{MINUTE}", info.TotalMinute); // {MINUTE}분
            itemBase.SetData(info.reward);
            labelReward.Text = $"{info.reward.ItemName} x{info.reward.Count}";
        }
    }
}
