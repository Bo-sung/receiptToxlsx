using UnityEngine;

namespace Ragnarok
{
    public class NpcChatBalloon : HUDObject
    {
        const int SIZE_MAX = 200;

        [SerializeField] UILabel label;
        [SerializeField] UILabel labelSize;

        public void Initialize(string text)
        {
            labelSize.text = text;
            labelSize.ProcessText();

            label.SetDimensions(Mathf.Min(SIZE_MAX, labelSize.width), label.height);
            label.text = text;
        }
    }
}