using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(UILabel))]
    public sealed class SwitchLabelOutlineColor : SwitchHelper<Color>
    {
        UILabel label;

        protected override void Awake()
        {
            base.Awake();

            label = GetComponent<UILabel>();
        }

        protected override void Reset()
        {
            base.Reset();

            on = Color.white;
            off = Color.white;
        }

        protected override void Execute(Color value)
        {
            label.effectColor = value;
        }
    }
}