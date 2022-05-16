using UnityEngine;

namespace Ragnarok
{
    public sealed class UIEscape : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_Popup; // 채팅UI에 묻히지 않게 하기 위해 ...

        [SerializeField] UIButton btnEscape;

        protected override void OnInit()
        {
            EventDelegate.Add(btnEscape.onClick, OnClickedBtnEscape);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnEscape.onClick, OnClickedBtnEscape);
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

        void OnClickedBtnEscape()
        {
            UIManager.Instance.Escape();
        }
    }
}