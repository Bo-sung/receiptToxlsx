using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIDuelReward : UIElement<UIDuelReward.IInput>
    {
        public interface IInput
        {
            string GetTitle();
            RewardData[] GetRewards();
        }

        [SerializeField] UILabelHelper labelRank;
        [SerializeField] UIGrid grid;
        [SerializeField] UIRewardHelper[] rewards;

        protected override void OnLocalize()
        {
            UpdateTitle();
        }

        protected override void Refresh()
        {
            RewardData[] arrData = info.GetRewards();
            int dataCount = arrData == null ? 0 : arrData.Length;
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(i < dataCount ? arrData[i] : null);
            }

            grid.Reposition();

            UpdateTitle();
        }

        private void UpdateTitle()
        {
            if (info == null)
                return;

            labelRank.Text = info.GetTitle();
        }
    }
}