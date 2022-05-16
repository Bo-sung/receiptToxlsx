using UnityEngine;

namespace Ragnarok
{
    public class UITimePressButton : UIButton
    {
        [SerializeField, Rename(displayName = "이벤트 호출되는 딜레이")]
        float delay = 2f;

        private float timer;
        private bool isClicked;

        protected virtual void Update()
        {
            if (state != State.Pressed)
            {
                timer = 0f;
                isClicked = false;
                return;
            }

            if (isClicked)
                return;

            timer += RealTime.deltaTime;
            if (timer < delay)
                return;

            isClicked = true;
            base.OnClick();
        }

        protected override void OnClick()
        {
            // Do Nothing
        }

        protected override void OnPress(bool isPressed)
        {
            base.OnPress(isPressed);

            if (!isPressed)
            {
                timer = 0f;
                return;
            }

            timer += RealTime.deltaTime;

            if (timer < delay)
                return;

            base.OnClick();
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