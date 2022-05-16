using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleFreeFight : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] BattleWaveView battleWaveView;
        [SerializeField] BattleFreeFightRewardView battleFreeFightRewardView;

        public event System.Action OnFinish;

        BattleFreeFightPresenter presenter;

        protected override void OnInit()
        {
            presenter = new BattleFreeFightPresenter();

            battleWaveView.OnFinished += OnFinishedTimer;

            battleWaveView.SetTitle(LocalizeKey._40014); // 제 {INDEX} 라운드

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            battleWaveView.OnFinished -= OnFinishedTimer;
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
        public void Initialize(float milliseconds)
        {
            battleWaveView.Initialize(milliseconds);
        }

        public void RestartTimer()
        {
            battleWaveView.RestartTimer();
        }

        public void ResetTimer()
        {
            battleWaveView.ResetTimer();
        }

        public void StopTimer()
        {
            battleWaveView.StopTimer();
        }

        public void SetRound(int round)
        {
            battleWaveView.SetWave(round);
        }

        public void SetKillCount(int killCount)
        {
            RewardData[] arrReward = presenter.GetCurrentRewards(killCount);
            int nextRemainKillCount = presenter.GetNextRemainKillCount(killCount);

            battleFreeFightRewardView.SetData(arrReward);
            battleFreeFightRewardView.SetNextRemainKillCount(nextRemainKillCount);
        }

        public void SetFreeFightEventType(FreeFightEventType type)
        {
            presenter.SetFreeFightEventType(type);
        }
    }
}