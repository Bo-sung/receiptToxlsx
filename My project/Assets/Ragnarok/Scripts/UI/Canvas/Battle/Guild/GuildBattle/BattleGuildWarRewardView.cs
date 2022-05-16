using UnityEngine;

namespace Ragnarok.View
{
    public class BattleGuildWarRewardView : BattleRewardView
    {
        [SerializeField] UILabelValue totalDamage;
        [SerializeField] GameObject nextReward;
        [SerializeField] UILabelHelper labelNextReward;

        private int progress = -1;

        protected override void Awake()
        {
            base.Awake();

            SetTitle(LocalizeKey._40015); // 현재 보상
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();

            totalDamage.TitleKey = LocalizeKey._40023; // 누적피해량
            UpdateNextRemainDamage();
        }

        public override void Show()
        {
            base.Show();

            SetNextRemainDamageProgress(0);
        }

        public void SetDamage(int damage, int progress)
        {
            totalDamage.Value = StringBuilderPool.Get()
                .Append(damage.ToString("N0"))
                .Append(" (").Append(progress).Append("%)")
                .Release();
        }

        public void SetNextRemainDamageProgress(int progress)
        {
            if (this.progress == progress)
                return;

            this.progress = progress;

            bool isShowNextReward = this.progress > 0;
            NGUITools.SetActive(nextReward, isShowNextReward);
            labelNextReward.SetActive(isShowNextReward);

            UpdateNextRemainDamage();
        }

        private void UpdateNextRemainDamage()
        {
            labelNextReward.Text = LocalizeKey._40024.ToText() // 다음 보상 [FFFF00]{VALUE}%[-]
                .Replace(ReplaceKey.VALUE, progress);
        }
    }
}