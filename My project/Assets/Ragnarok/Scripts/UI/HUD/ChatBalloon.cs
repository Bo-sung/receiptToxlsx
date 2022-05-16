using UnityEngine;

namespace Ragnarok
{
    public class ChatBalloon : HUDObject
    {
        [SerializeField] UILabel label;
        RelativeRemainTime remainTime;

        public virtual void Initialize(string text, float duration)
        {
            label.text = text;
            remainTime = duration;
        }

        protected override void Update()
        {
            base.Update();

            if (remainTime.GetRemainTime() == 0f)
                Release();
        }
    }
}