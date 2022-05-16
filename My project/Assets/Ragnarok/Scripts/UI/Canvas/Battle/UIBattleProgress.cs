using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleProgress : UICanvas
    {
        protected override UIType uiType => UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        public enum IconType
        {
            /// <summary>
            /// 아이콘 없음
            /// </summary>
            None,
            /// <summary>
            /// 터치 아이콘
            /// </summary>
            Touch,
        }

        [SerializeField] UIPlayTween tween;
        [SerializeField] GameObject loop;
        [SerializeField] UISpriteAnimation icon;
        [SerializeField] UIProgressBar progressBar;
        [SerializeField] UILabelHelper labelMessage;

        public event System.Action OnFinished;

        private float maxTime;
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
            NGUITools.SetActive(tween.tweenTarget, false);
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
                progressBar.value = MathUtils.GetProgress(time, maxTime);
                return;
            }

            StopTimer();
            OnFinished?.Invoke();
        }

        public void StartTimer(float remainTime, IconType type = IconType.None, string message = null)
        {
            // Tweener Reset
            NGUITools.SetActive(tween.tweenTarget, true);
            UITweener[] tweeners = tween.tweenTarget.GetComponents<UITweener>();
            for (int i = 0; i < tweeners.Length; i++)
            {
                tweeners[i].ResetToBeginning();
            }

            // Icon
            string spriteNamePrefix = GetSpriteNamePrefix(type);
            if (string.IsNullOrEmpty(spriteNamePrefix))
            {
                NGUITools.SetActive(loop, false);
            }
            else
            {
                NGUITools.SetActive(loop, true);
                icon.namePrefix = spriteNamePrefix;
                icon.framesPerSecond = GetSpriteFramerate(type);
            }

            // Text
            if (string.IsNullOrEmpty(message))
            {
                labelMessage.SetActive(false);
            }
            else
            {
                labelMessage.SetActive(true);
                labelMessage.Text = message;
            }

            // Timer
            maxTime = remainTime;
            SetRemainTime(remainTime);
            progressBar.value = 1f;
            isRun = true;
        }

        public void SetRemainTime(float remainTime)
        {
            this.remainTime = remainTime;
        }

        public void DecreaseRemainTime(float decreaseTime)
        {
            this.remainTime -= decreaseTime;
        }

        public void StopTimer()
        {
            isRun = false;
            progressBar.value = 0f;

            tween.Play();
        }

        private string GetSpriteNamePrefix(IconType type)
        {
            switch (type)
            {
                case IconType.Touch:
                    return "Ui_Battle_Touch";
            }

            return null;
        }

        private int GetSpriteFramerate(IconType type)
        {
            switch (type)
            {
                case IconType.Touch:
                    return 10;
            }

            return 0;
        }
    }
}