using UnityEngine;

namespace Ragnarok
{
    public class TotalDamage : HUDObject
    {
        [SerializeField] UILabel label;
        [SerializeField] UIPlayTween playTween;
        [SerializeField] TweenPosition tweenPosition;

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

        public void Initialize(int totalValue, int fontSize)
        {
            label.fontSize = fontSize;
            ShowText(totalValue.ToString());
        }

        private void ShowText(string text)
        {
            label.text = text;
            playTween.Play(true);
        }
    }
}