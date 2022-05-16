using UnityEngine;

namespace Ragnarok.View
{
    public class AddGoodsView : UIView
    {
        public interface IInput
        {
            RewardData GetAddGoodsReward();
        }

        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelDescription;

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._8106; // 추가 지급
        }

        public void Set(IInput input)
        {
            rewardHelper.SetData(input.GetAddGoodsReward());
            labelName.Text = input.GetAddGoodsReward().ItemName;
            labelDescription.Text = input.GetAddGoodsReward().GetDescription();
        }
    }
}