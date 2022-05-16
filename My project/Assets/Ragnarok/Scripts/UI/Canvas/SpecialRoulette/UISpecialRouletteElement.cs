using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UISpecialRouletteElement : UIElement<UISpecialRouletteElement.IInput>
    {
        public interface IInput
        {
            int Id { get; }
            RewardData Reward { get; }
            bool IsComplete { get; }
            void SetReceived(bool isReceived);
            int Rate { get; }
            int TotalRate { get; }
            void SetTotalRate(int totalRate);
        }

        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] GameObject complete;
        [SerializeField] UILabelHelper labelRate;

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            rewardHelper.SetData(info.Reward);
            complete.SetActive(info.IsComplete);
            if (info.IsComplete)
            {
                labelRate.Text = string.Empty;
            }
            else
            {
                labelRate.Text = (info.Rate / (float)info.TotalRate).ToString("0.###%");
            }
        }
    }
}