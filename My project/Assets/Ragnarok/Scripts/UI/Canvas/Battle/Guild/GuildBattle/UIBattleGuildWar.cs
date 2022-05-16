using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleGuildWar : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] TimerView timerView;
        [SerializeField] BattleGuildWarRewardView battleGuildWarRewardView;

        public event System.Action OnFinish;

        BattleGuildWarPresenter presenter;

        protected override void OnInit()
        {
            presenter = new BattleGuildWarPresenter();

            timerView.OnFinished += OnFinishedTimer;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            timerView.OnFinished -= OnFinishedTimer;
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        void OnFinishedTimer()
        {
            OnFinish?.Invoke();
        }

        /// <summary>
        /// 남은 시간 세팅 (밀리초)
        /// </summary>
        public void Initialize(long limitTime)
        {
            timerView.Initialize(limitTime);
        }

        public void RestartTimer()
        {
            timerView.RestartTimer();
        }

        public void ResetTimer()
        {
            timerView.ResetTimer();
        }

        public void StopTimer()
        {
            timerView.StopTimer();
        }

        public void SetDamage(int damage)
        {
            RewardData[] arrReward = presenter.GetCurrentRewards(damage);
            int progress = presenter.GetDamagePercentProgress(damage);
            int remainProgress = presenter.GetNextRemainDamageProgress(damage);

            battleGuildWarRewardView.SetData(arrReward);
            battleGuildWarRewardView.SetDamage(damage, progress);
            battleGuildWarRewardView.SetNextRemainDamageProgress(remainProgress);
        }
    }
}