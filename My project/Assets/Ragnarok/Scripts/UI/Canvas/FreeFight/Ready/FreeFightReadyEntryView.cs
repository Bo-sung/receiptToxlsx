using UnityEngine;

namespace Ragnarok.View
{
    public class FreeFightReadyEntryView : UIView
    {
        [SerializeField] UIButtonHelper btnEntry;
        [SerializeField] UILabelHelper labelDescription;

        public event System.Action OnSelectEnter;
        public event System.Action OnFinished;

        private bool isRun;
        private RemainTime remainTime;
        private int countdown;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnEntry.OnClick, OnClickedBtnEntry);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnEntry.OnClick, OnClickedBtnEntry);
        }

        public override void Hide()
        {
            base.Hide();

            isRun = false;
        }

        protected override void OnLocalize()
        {
            btnEntry.LocalKey = LocalizeKey._40012; // 전투 참여
        }

        void Update()
        {
            if (!isRun)
                return;

            UpdateTime();
        }

        void OnClickedBtnEntry()
        {
            OnSelectEnter?.Invoke();
        }

        public void Run(float remainTicks)
        {
            isRun = true;
            remainTime = remainTicks;
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
            labelDescription.Text = LocalizeKey._40013.ToText() // {SECONDS}초 뒤 자동 참여
                .Replace(ReplaceKey.SECONDS, countdown);
        }

        private void Finish()
        {
            isRun = false;
            OnFinished?.Invoke();
        }
    }
}