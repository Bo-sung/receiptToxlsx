using UnityEngine;

namespace Ragnarok
{
    public class SpeechBalloon : HUDObject, IAutoInspectorFinder
    {
        public enum BalloonType
        {
            None,
            Shake,
        }

        [SerializeField] UISprite background;
        [SerializeField] UILabel label;
        [SerializeField] UIPlayTween tweenShake;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(tweenShake.onFinished, Release);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(tweenShake.onFinished, Release);
        }

        public void Initialize(BalloonType type, string text)
        {
            label.text = text;

            switch (type)
            {
                case BalloonType.Shake:
                    tweenShake.Play(true);
                    break;
            }
        }
    }
}