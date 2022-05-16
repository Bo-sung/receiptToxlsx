using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIRankRewardElement : UIElement<UIRankRewardElement.IInput>, IInspectorFinder
    {
        public interface IInput
        {
            string GetName();
            RewardData GetRewardData(int index);
        }

        [SerializeField] UILabelHelper labelName;
        [SerializeField] UIGrid rewardGrid;
        [SerializeField] UIRewardHelper[] rewards;

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            labelName.Text = info.GetName();

            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(info.GetRewardData(i));
            }

            rewardGrid.Reposition();
        }

        bool IInspectorFinder.Find()
        {
            rewards = GetComponentsInChildren<UIRewardHelper>();
            return true;
        }
    }
}