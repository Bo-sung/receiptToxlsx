using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIRewardListElement : UIElement<UIRewardListElement.IInput>
    {
        public interface IInput
        {
            RewardData Reward { get; }
            int Rate { get; }
            int TotalRate { get; }
            void SetTotalRate(int totalRate);
        }

        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UILabelHelper labelRate;

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            rewardHelper.SetData(info.Reward);
            if(labelRate)
                labelRate.Text = (info.Rate / (float)info.TotalRate).ToString("0.###%");
        }
    }
}