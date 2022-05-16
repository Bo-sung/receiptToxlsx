using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(UIWidget))]
    public sealed class SwitchWidgetColor : SwitchHelper<Color>
    {
        UIWidget widget;

        protected override void Awake()
        {
            base.Awake();

            widget = GetComponent<UIWidget>();
        }

        protected override void Reset()
        {
            base.Reset();

            on = Color.white;
            off = Color.white;
        }

        protected override void Execute(Color value)
        {
            widget.color = value;
        }
    }
}