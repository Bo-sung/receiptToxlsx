using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIProfileSelect : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] SelectPopupView selectPopupView;
        [SerializeField] ProfileSelectView profileSelectView;

        ProfileSelectPresenter presenter;

        protected override void OnInit()
        {
            presenter = new ProfileSelectPresenter();

            selectPopupView.OnExit += CloseUI;
            selectPopupView.OnCancel += CloseUI;
            selectPopupView.OnConfirm += presenter.RequestSelectProfile;
            profileSelectView.OnSelect += presenter.SetSelectProfileId;

            presenter.OnSelectProfile += RefreshSelectProfile;
            presenter.OnUpdateMileage += RefreshMileage;
            presenter.OnFinished += CloseUI;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnSelectProfile -= RefreshSelectProfile;
            presenter.OnUpdateMileage -= RefreshMileage;
            presenter.OnFinished -= CloseUI;

            selectPopupView.OnExit -= CloseUI;
            selectPopupView.OnCancel -= CloseUI;
            selectPopupView.OnConfirm -= presenter.RequestSelectProfile;
            profileSelectView.OnSelect -= presenter.SetSelectProfileId;
        }

        protected override void OnShow(IUIData data = null)
        {
            RefreshMileage();

            presenter.Initialize(); // 초기화
            profileSelectView.SetData(presenter.GetArrayData());
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            selectPopupView.MainTitleLocalKey = LocalizeKey._4059; // 프로필 변경
            selectPopupView.ConfirmLocalKey = LocalizeKey._4061; // 확인
            selectPopupView.CancelLocalKey = LocalizeKey._2; // 취소
        }

        private void RefreshSelectProfile()
        {
            profileSelectView.Refresh(presenter.GetSelectedData());
        }

        private void RefreshMileage()
        {
            profileSelectView.Initialize(presenter.GetCurMileage());
        }

        private void CloseUI()
        {
            UI.Close<UIProfileSelect>();
        }
    }
}