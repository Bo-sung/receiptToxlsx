using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGuide : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] PopupView popupView;
        [SerializeField] GuideView guideView;

        GuidePresenter presenter;

        protected override void OnInit()
        {
            presenter = new GuidePresenter();

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
            UIGuideElement.IInput[] arrayInfo = presenter.GetArrayInfo();
            guideView.SetData(arrayInfo);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._5400; // 가이드
            popupView.ConfirmLocalKey = LocalizeKey._1; // 확인
        }
    }
}