using UnityEngine;

namespace Ragnarok
{
    public sealed class UIRewardInfoSlot : UIInfo<RewardInfo>
    {
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UILabelHelper labelName;

        public override void SetData(RewardInfo info)
        {
            base.SetData(info);
            rewardHelper.SetData(info.data);
        }

        protected override void Refresh()
        {
            labelName.Text = info.data.ItemName;
        }
    }
}