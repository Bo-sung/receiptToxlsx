using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleEndlessTower : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] BattleWaveView battleWaveView;
        [SerializeField] BattleRewardView battleRewardView;

        public event System.Action OnFinish;

        BattleEndlessTowerPresenter presenter;

        protected override void OnInit()
        {
            presenter = new BattleEndlessTowerPresenter();

            battleWaveView.OnFinished += OnFinishedTimer;

            battleWaveView.SetTitle(LocalizeKey._39300); // {INDEX} 층
            battleRewardView.SetTitle(LocalizeKey._39301); // 보스 보상

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

        public void SetFloor(int floor)
        {
            battleWaveView.SetWave(floor);

            if (presenter.IsBossFloor(floor))
            {
                battleRewardView.Show();
                battleRewardView.SetData(presenter.GetCurrentRewards(floor));
            }
            else
            {
                battleRewardView.Hide();
            }
        }
    }
}