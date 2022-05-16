using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIDarkTreeRewardSelect : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] SelectPopupView selectPopupView;
        [SerializeField] DarkTreeRewardView darkTreeRewardView;

        DarkTreeRewardSelectPresenter presenter;

        protected override void OnInit()
        {
            presenter = new DarkTreeRewardSelectPresenter();

            selectPopupView.OnExit += CloseUI;
            selectPopupView.OnCancel += CloseUI;
            selectPopupView.OnConfirm += presenter.RequestSelectReward;
            darkTreeRewardView.OnSelect += presenter.SetSelectRewardId;

            presenter.OnSelectReward += RefreshSelectReward;
            presenter.OnFinished += CloseUI;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnSelectReward -= RefreshSelectReward;
            presenter.OnFinished -= CloseUI;

            selectPopupView.OnExit -= CloseUI;
            selectPopupView.OnCancel -= CloseUI;
            selectPopupView.OnConfirm -= presenter.RequestSelectReward;
            darkTreeRewardView.OnSelect -= presenter.SetSelectRewardId;
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.Initialize(); // 초기화
            darkTreeRewardView.SetData(presenter.GetArrayData());
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            selectPopupView.MainTitleLocalKey = LocalizeKey._9031; // 보상 선택
            selectPopupView.ConfirmLocalKey = LocalizeKey._9032; // 보상 결정
            selectPopupView.CancelLocalKey = LocalizeKey._2; // 취소
        }

        private void RefreshSelectReward()
        {
            darkTreeRewardView.Refresh(presenter.GetSelectedData());
        }

        private void CloseUI()
        {
            UI.Close<UIDarkTreeRewardSelect>();
        }
    }
}