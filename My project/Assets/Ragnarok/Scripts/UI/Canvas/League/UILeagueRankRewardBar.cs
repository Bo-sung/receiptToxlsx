using UnityEngine;

namespace Ragnarok.View.League
{
    public class UILeagueRankRewardBar : UIElement<UILeagueRankRewardBar.IInput>, IInspectorFinder
    {
        public interface IInput
        {
            bool IsEntryReward(); // 참가보상 유무
            string GetName();
            RewardData GetRewardData(int index);
        }

        [SerializeField] UILabelHelper labelRank;
        [SerializeField] UIGrid grid;
        [SerializeField] UIRewardHelper[] rewards;

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            labelRank.Text = info.IsEntryReward()
                ? LocalizeKey._47016.ToText() // 참가 보상
                : info.GetName();

            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(info.GetRewardData(i));
            }

            grid.Reposition();
        }

        bool IInspectorFinder.Find()
        {
            rewards = GetComponentsInChildren<UIRewardHelper>();
            return true;
        }
    }
}