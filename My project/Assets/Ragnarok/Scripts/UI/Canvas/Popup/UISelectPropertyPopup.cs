using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UISelectPropertyPopup : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] SelectPropertyPopupView popupBase;
        [SerializeField] SelectPropertyWeaponView weaponView;
        [SerializeField] SelectPropertyCCView cCView;

        SelectPropertyPopupPresenter presenter;

        protected override void OnInit()
        {
            presenter = new SelectPropertyPopupPresenter();

            popupBase.HideUI += OnHideUI;
            weaponView.HideUI += OnHideUI;
            cCView.HideUI += OnHideUI;
        }

        protected override void OnClose()
        {
            popupBase.HideUI -= OnHideUI;
            weaponView.HideUI -= OnHideUI;
            cCView.HideUI -= OnHideUI;
        }

        protected override void OnShow(IUIData data = null)
        {
            popupBase.SetActive(false);
            weaponView.SetActive(false);
            cCView.SetActive(false);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        private void OnHideUI()
        {
            UI.Close<UISelectPropertyPopup>();
        }

        public void ShowElementView(ElementType type)
        {
            if (type == ElementType.None)
            {
                weaponView.SetActive(true);
            }
            else
            {
                popupBase.SetActive(true);
                popupBase.ShowElementDesc(type, presenter.GetPropertyInfos(type));
            }
        }

        public void ShowCCView(CrowdControlType type)
        {
            cCView.SetActive(true);
            cCView.ShowPropertyDesc(type);
        }

    }
}