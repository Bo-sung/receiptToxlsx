using UnityEngine;

namespace Ragnarok
{
    public sealed class UIPinchZoomCanvas : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] public UIPinchZoom uiPinchZoom;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }
    }
}