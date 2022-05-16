using UnityEngine;

namespace Ragnarok
{
    public sealed class UITempNotice : UICanvas, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UILabel labelDescription;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
            labelDescription.text = LocalizeKey._3500.ToText(); // 업데이트 예정
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {

        }
    }
}