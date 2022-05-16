using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleCountdown : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UILabelHelper labelText;
        [SerializeField] UIPlayTween tween;

        private bool isRun;
        private RemainTime remainTime;
        private int countdown;

        public event System.Action OnFinish;

        protected override void OnInit()
        {
            EventDelegate.Add(tween.onFinished, OnFinishedTween);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(tween.onFinished, OnFinishedTween);
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

        void Update()
        {
            if (!isRun)
                return;

            UpdateTime();
        }

        public void Show(float milliseconds)
        {
            Show();

            isRun = true;
            remainTime = milliseconds;

            UpdateTime();
        }

        private void UpdateTime()
        {
            float time = remainTime.ToRemainTime();
            if (time <= 0f)
            {
                Finish();
                return;
            }

            float seconds = (float)System.TimeSpan.FromMilliseconds(time).TotalSeconds;
            int countdown = Mathf.CeilToInt(seconds);
            SetCountdown(countdown);
        }

        private void SetCountdown(int countdown)
        {
            if (this.countdown == countdown)
                return;

            this.countdown = countdown;
            labelText.Text = this.countdown.ToString();
            tween.Play();
        }

        private void Finish()
        {
            isRun = false;
            labelText.Text = "START!";
            tween.Play();
        }

        void OnFinishedTween()
        {
            // 진행 중일 때에는 아무것도 하지 않음
            if (isRun)
                return;

            Hide();
            OnFinish?.Invoke();
        }
    }
}