using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleBossFury : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UIWidget anchors;
        [SerializeField] UIHourglass hourglass;

        public event System.Action OnFinished;
        public event System.Action<double> OnUpdate;

        protected override void OnInit()
        {
            hourglass.OnFinished += OnFinishedTimer;
            hourglass.OnUpdate += OnUpdateTimer;
        }

        protected override void OnClose()
        {
            hourglass.OnFinished -= OnFinishedTimer;
            hourglass.OnUpdate -= OnUpdateTimer;
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
            OnFinished?.Invoke();
        }

        void OnUpdateTimer(double totalSeconds)
        {
            OnUpdate?.Invoke(totalSeconds);
        }

        public void Initialize(float limitTime)
        {
            hourglass.SetLimitTime(limitTime);
        }

        public void Show(GameObject target)
        {
            Show();

            anchors.SetAnchor(target, 0, 0, 0, 0);
        }

        public void RestartTimer()
        {
            hourglass.StartTimer();
        }

        public void ResetTimer()
        {
            hourglass.ResetTimer();
        }

        public void StopTimer()
        {
            hourglass.StopTimer();
        }
    }
}