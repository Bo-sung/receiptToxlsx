using UnityEngine;

namespace Ragnarok
{
    public class LobbyChatBallon : HUDObject
    {
        const int SIZE_MAX = 200;
        [SerializeField] UILabel label;
        RelativeRemainTime remainTime;
        [SerializeField] UILabel labelSize;

        public void Initialize(string text, float duration)
        {            
            labelSize.text = text;
            labelSize.ProcessText();

            label.SetDimensions(Mathf.Min(SIZE_MAX, labelSize.width), label.height);
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
