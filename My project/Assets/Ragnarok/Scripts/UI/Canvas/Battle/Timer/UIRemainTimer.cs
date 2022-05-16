using UnityEngine;

namespace Ragnarok
{
    public sealed class UIRemainTimer : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UILabel labelTime;

        public event System.Action OnFinished;

        private RemainTime remainTime;
        private bool isRun;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
            StopTimer();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        void Update()
        {
            if (!isRun)
                return;

            float time = remainTime.ToRemainTime();
            if (time > 0f)
            {
                labelTime.text = time.ToStringTime(@"mm\:ss\:ff");
                return;
            }

            StopTimer();
            OnFinished?.Invoke();
        }

        public void StartTimer(float remainTime)
        {
            this.remainTime = remainTime;
            isRun = true;
        }

        private void StopTimer()
        {
            isRun = false;
            labelTime.text = "00:00:00";
        }
    }
}