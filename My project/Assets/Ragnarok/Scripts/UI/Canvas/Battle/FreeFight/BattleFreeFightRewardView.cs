using UnityEngine;

namespace Ragnarok.View
{
    public class BattleFreeFightRewardView : BattleRewardView
    {
        [SerializeField] GameObject nextReward;
        [SerializeField] UILabelHelper labelNextReward;

        private int nextRemainKillCount = -1;

        protected override void Awake()
        {
            base.Awake();

            SetTitle(LocalizeKey._40015); // 현재 보상
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();

            UpdateNextKillCountText(); // 다음 보상까지
        }

        public override void Show()
        {
            base.Show();
            SetNextRemainKillCount(0);
        }

        public void SetNextRemainKillCount(int nextRemainKillCount)
        {
            if (this.nextRemainKillCount == nextRemainKillCount)
                return;

            this.nextRemainKillCount = nextRemainKillCount;

            bool isShowNextReward = this.nextRemainKillCount > 0;
            NGUITools.SetActive(nextReward, isShowNextReward);
            labelNextReward.SetActive(isShowNextReward);

            UpdateNextKillCountText();
        }

        private void UpdateNextKillCountText()
        {
            labelNextReward.Text = LocalizeKey._40017.ToText() // 다음 보상까지 {COUNT}
                .Replace(ReplaceKey.COUNT, nextRemainKillCount);
        }
    }
}