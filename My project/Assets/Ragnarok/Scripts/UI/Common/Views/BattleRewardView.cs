using UnityEngine;

namespace Ragnarok.View
{
    public class BattleRewardView : UIView, IInspectorFinder
    {
        [SerializeField] UILabelHelper labelRewardTitle;
        [SerializeField] UIGrid gridReward;
        [SerializeField] UIRewardHelper[] rewards;
        [SerializeField] UILabelHelper labelNoData;

        private int titleLocalKey;

        protected override void OnLocalize()
        {
            labelNoData.LocalKey = LocalizeKey._40016; // 보상 없음
            UpdateTitle();
        }

        public override void Show()
        {
            base.Show();
            SetData(null); // 초기화
        }

        public void SetTitle(int titleLocalKey)
        {
            this.titleLocalKey = titleLocalKey;
            UpdateTitle();
        }

        public void SetData(RewardData[] arrReward)
        {
            int rewardCount = arrReward == null ? 0 : arrReward.Length;
            labelNoData.SetActive(rewardCount == 0);

            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(i < rewardCount ? arrReward[i] : null);
            }
            gridReward.Reposition();
        }

        private void UpdateTitle()
        {
            labelRewardTitle.LocalKey = titleLocalKey;
        }

        bool IInspectorFinder.Find()
        {
            rewards = GetComponentsInChildren<UIRewardHelper>();
            return true;
        }
    }
}