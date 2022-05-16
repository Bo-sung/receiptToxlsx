using UnityEngine;

namespace Ragnarok
{
    public class PlainText : HUDObject
    {
        [SerializeField] UILabelHelper label;
        [SerializeField] UIPlayTween playTween;
        [SerializeField] TweenAlpha tweenAlpha;
        [SerializeField] TweenPosition tweenPosition;

        [SerializeField, Rename(displayName = "폰트 크기")]
        int fontSize = 25;

        [SerializeField, Rename(displayName = "시작 좌표")]
        Vector3 fromPosition = new Vector3(0f, 0f);

        [SerializeField, Rename(displayName = "끝 좌표")]
        Vector3 toPosition = new Vector3(0f, 100f);

        [SerializeField, Rename(displayName = "Duration")]
        float duration = 1f;

        [SerializeField, Rename(displayName = "색상")]
        Color textColor = Color.blue;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(playTween.onFinished, Release);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(playTween.onFinished, Release);
        }

        public void Initialize(string text)
        {
            tweenAlpha.duration = duration;
            tweenPosition.duration = duration;
            tweenPosition.from = fromPosition;
            tweenPosition.to = toPosition;
            label.uiLabel.color = textColor;
            label.uiLabel.fontSize = fontSize;

            ShowText(text);
        }

        public void SetDuration(float time)
        {
            tweenAlpha.duration = time;
            tweenPosition.duration = time;
        }

        public void SetLoopStyle(UITweener.Style style)
        {
            tweenAlpha.style = style;
            tweenPosition.style = style;
        }

        public void SetFontSize(int size)
        {
            label.uiLabel.fontSize = size;
        }

        public void SetTweenPosition_From(Vector3 offset)
        {
            tweenPosition.from = offset;
        }

        public void SetTweenPosition_To(Vector3 offset)
        {
            tweenPosition.to = offset;
        }

        public void SetTweenAlpha_From(float alpha)
        {
            tweenAlpha.from = alpha;
        }

        public void SetTweenAlpha_To(float alpha)
        {
            tweenAlpha.to = alpha;
        }

        /// <summary>
        /// 변화하지 않는 고정 텍스트로 설정 (모든 Tween Off)
        /// </summary>
        public void SetStatic()
        {
            SetLoopStyle(UITweener.Style.Once);

            tweenAlpha.Finish();
            tweenPosition.Finish();

            label.uiLabel.alpha = tweenAlpha.from;
            label.uiLabel.cachedTransform.localPosition = tweenPosition.from;
        }


        private void ShowText(string text)
        {
            label.Text = text;
            playTween.Play(true);
        }
    }
}