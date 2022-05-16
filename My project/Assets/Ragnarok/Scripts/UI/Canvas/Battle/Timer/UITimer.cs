using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UITimer : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] TimerView timerView;

        public event System.Action OnFinish;

        protected override void OnInit()
        {
            timerView.OnFinished += OnFinishedTimer;
        }

        protected override void OnClose()
        {
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
    }
}