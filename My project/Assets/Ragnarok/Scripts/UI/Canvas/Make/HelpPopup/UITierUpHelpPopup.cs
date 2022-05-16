using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UITierUpHelpPopup : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] PopupView popupView;
        [SerializeField] TierUpHelpView tierUpHelpView;

        TierUpHelpPopupPresenter presenter;

        protected override void OnInit()
        {
            presenter = new TierUpHelpPopupPresenter();

            popupView.OnConfirm += OnBack;
            popupView.OnExit += OnBack;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            popupView.OnConfirm -= OnBack;
            popupView.OnExit -= OnBack;
        }

        protected override void OnShow(IUIData data = null)
        {
            UITierUpHelpElement.IInput[] arrayInfo = presenter.GetArrayInfo();
            tierUpHelpView.SetData(arrayInfo);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._5500; // 초월 정보
            popupView.ConfirmLocalKey = LocalizeKey._1; // 확인
        }
    }
}