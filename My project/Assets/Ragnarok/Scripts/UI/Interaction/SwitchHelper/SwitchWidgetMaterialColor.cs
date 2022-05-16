using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(UIWidget))]
    public sealed class SwitchWidgetMaterialColor : SwitchHelper<Color>
    {
        [SerializeField] string shaderColorName = "_Color";

        UIWidget widget;

        private int shaderColorNameId;

        protected override void Awake()
        {
            base.Awake();

            widget = GetComponent<UIWidget>();
            shaderColorNameId = Shader.PropertyToID(shaderColorName);
        }

        protected override void Reset()
        {
            base.Reset();

            on = Color.white;
            off = Color.white;
        }

        protected override void Execute(Color value)
        {
            widget.material.SetColor(shaderColorNameId, value);
            widget.RemoveFromPanel();
            widget.MarkAsChanged();
        }
    }
}