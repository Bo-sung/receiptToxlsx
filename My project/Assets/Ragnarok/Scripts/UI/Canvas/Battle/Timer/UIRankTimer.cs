using UnityEngine;

namespace Ragnarok
{
    public sealed class UIRankTimer : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UILabel labelTime;
        [SerializeField] UIButtonHelper btnInfo;

        private CumulativeTime playTime;
        private bool isRun;
        private int endTime;
        private bool isReverse;

        /// <summary>
        /// endTime까지 다 쟀을 때 호출.
        /// </summary>
        public event System.Action OnTimeOver;

        protected override void OnInit()
        {
            EventDelegate.Add(btnInfo.OnClick, OnClickedBtnInfo);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnInfo.OnClick, OnClickedBtnInfo);
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

            var span = playTime.ToCumulativeTime().ToTimeSpan();
            if (span.TotalSeconds < endTime)
            {
                Refresh();
                return;
            }

            Refresh();
            StopTimer();

            // 제한시간 타이머인 경우 00으로 세팅.
            if (isReverse)
                labelTime.text = "00:00:00";

            OnTimeOver?.Invoke();
        }

        public void Initialize(CumulativeTime playTime, int endTimd = 3600, bool isReverse = false)
        {
            this.playTime = playTime;
            this.endTime = endTimd;
            this.isReverse = isReverse;

            Refresh();
        }

        public void StartTimer()
        {
            isRun = true;
        }

        public void StopTimer()
        {
            isRun = false;
        }

        private void Refresh()
        {
            if (isReverse)
            {
                var reverseTime = endTime * 1000f - playTime.ToCumulativeTime();
                labelTime.text = reverseTime.ToStringTime(@"mm\:ss\:ff");
            }
            else
            {
                labelTime.text = playTime.ToStringTime(@"mm\:ss\:ff");
            }
        }

        void OnClickedBtnInfo()
        {
            UI.ShowToastPopup(LocalizeKey._90111.ToText()); // 보물 상자 획득까지 소요되는 시간입니다.\n소요 시간이 1시간이 넘거나 보물 상자를 열면 정지됩니다.
        }
    }
}