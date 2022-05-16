using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    class UIClickPressButton : UIButton
    {
        [SerializeField, Rename(displayName = "클릭 이벤트가 푸시 이벤트로 전환 전 까지의 시간")]
        float Interval = 0.13f;
        private float timer;

        public event System.Action OnDepressed;
        public List<EventDelegate> OnLongPress;
        public List<EventDelegate> OnClicked;

        public bool IsPressing { get; private set; }
        void Awake()
        {
            timer = 0;
            OnDepressed += CheckIsLongPress;
        }

        protected virtual void Update()
        {
            if (state != State.Pressed && IsPressing)
            {
                IsPressing = false;
                OnDepressed?.Invoke();
                return;
            }
            else if(state == State.Pressed)
            {
                IsPressing = true;
                timer += RealTime.deltaTime;
                Debug.Log($"timer ({timer})");
                return;
            }
        }

        public override void SetState(State state, bool immediate)
        {
            base.SetState(state, immediate);
        }
        public void CheckIsLongPress()
        {
            if (timer > Interval)
                Debug.Log($"LongPress ({timer})");
            else
                Debug.Log($"Clicked ({timer})");

            timer = 0;
            IsPressing = false;
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
