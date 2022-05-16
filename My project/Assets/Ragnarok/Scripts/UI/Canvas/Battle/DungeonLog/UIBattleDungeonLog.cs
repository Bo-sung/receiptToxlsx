using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleDungeonLog : UICanvas
    {
        public enum Type
        {
            Zeny,
            Exp,
        }

        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        const string TimerGageName1 = "Ui_Common_Icon_TimeGage1";
        const string TimerGageName2 = "Ui_Common_Icon_TimeGage2";

        [SerializeField] UILabel labelTime;
        //[SerializeField] UISprite timerGage;
        [SerializeField] GameObject iconZeny, iconExp;
        //[SerializeField] UIWidget itemTarget;
        //[SerializeField] UILabel labelItemCount;
        [SerializeField] UILabel labelCubeCount;

        private CumulativeTime playTime;
        private bool isRun;
        private float endTime; // 밀리초
        private bool isReverse;
        int maxCube;
        Type type;

        /// <summary>
        /// endTime까지 다 쟀을 때 호출.
        /// </summary>
        public event System.Action OnTimeOver;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
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

        public void SetType(Type type)
        {
            this.type = type;
            iconZeny.SetActive(type == Type.Zeny);
            iconExp.SetActive(type == Type.Exp);
        }

        public void Initialize(int value, int curCube, int maxCube)
        {
            this.maxCube = maxCube;
            SetCube(curCube);
            SetValue(value);
        }

        public void SetCube(int curCube)
        {
            labelCubeCount.text = $"{curCube}/{maxCube}";
        }

        public void SetValue(int value)
        {
            //labelItemCount.text = value.ToString("N0");
        }

        void Update()
        {
            if (!isRun)
                return;

            var span = playTime.ToCumulativeTime().ToTimeSpan();
            if (span.TotalMilliseconds < endTime)
            {
                //timerGage.fillAmount = MathUtils.GetProgress((float)span.TotalMilliseconds, endTime);
                //if(timerGage.fillAmount < 0.75f)
                //{
                //    timerGage.spriteName = TimerGageName1;
                //}
                //else
                //{
                //    timerGage.spriteName = TimerGageName2;
                //}
                Refresh();
                return;
            }

            //timerGage.fillAmount = 1f;
            Refresh();
            StopTimer();

            // 제한시간 타이머인 경우 00으로 세팅.
            if (isReverse)
                labelTime.text = "00:00:00";

            OnTimeOver?.Invoke();
        }

        public void Initialize(CumulativeTime playTime, float endTimd = 3600, bool isReverse = false)
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
                var reverseTime = endTime - playTime.ToCumulativeTime();
                labelTime.text = reverseTime.ToStringTime(@"mm\:ss\:ff");
            }
            else
            {
                labelTime.text = playTime.ToStringTime(@"mm\:ss\:ff");
            }
        }

        public UIWidget GetWidget(Type type)
        {
            if (this.type == type)
            {
                switch (type)
                {
                    case Type.Zeny:
                        return iconZeny.GetComponent<UIWidget>();

                    case Type.Exp:
                        return iconExp.GetComponent<UIWidget>();
                }
            }

            return default;
        }
    }
}