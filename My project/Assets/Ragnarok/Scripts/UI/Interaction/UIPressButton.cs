using UnityEngine;

namespace Ragnarok
{
    public class UIPressButton : UIButton
    {
        [SerializeField, Rename(displayName = "이벤트가 호출되는 최대 시간 간격 (맨 처음)")]
        float maxInterval = 0.25f;

        [SerializeField, Rename(displayName = "이벤트가 호출되는 최소 시간 간격 (맨 마지막)")]
        float minInterval = 0.02f;

        [SerializeField, Rename(displayName = "시간 간격 감소율")]
        float reduceValue = 0.05f;

        private float interval;
        private float timer;

        public event System.Action OnDepressed;

        public bool IsPressing { get; private set; }

        protected virtual void Update()
        {
            if (state != State.Pressed)
            {
                IsPressing = false;
                OnDepressed?.Invoke();
                return;
            }

            timer += RealTime.deltaTime;

            if (timer < interval)
                return;

            IsPressing = true;
            OnClick();
            SetInterval(Mathf.Max(interval - reduceValue, minInterval));
        }

        public override void SetState(State state, bool immediate)
        {
            base.SetState(state, immediate);

            if (state == State.Pressed)
                SetInterval(maxInterval);
        }

        private void SetInterval(float interval)
        {
            this.interval = interval;
            timer = 0;
        }

#if UNITY_EDITOR
        void Reset()
        {
            if (tweenTarget)
            {
                UIWidget widget = tweenTarget.GetComponent<UIWidget>();

                if (widget)
                    widget.color = Color.white;
            }

            hover = Color.white;
            pressed = Color.white;
            disabledColor = Color.gray;

            UIButtonScale buttonScale = gameObject.AddMissingComponent<UIButtonScale>();
            buttonScale.tweenTarget = transform;
            buttonScale.hover = Vector3.one;
            buttonScale.pressed = Vector3.one * 1.1f;
            buttonScale.duration = 0.2f;
        }
#endif
    }
}