using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIGateRewardElement : UIElement<RewardData>
    {
        [SerializeField] UIRewardHelper rewardHelper;

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            rewardHelper.SetData(info);
        }
    }
}