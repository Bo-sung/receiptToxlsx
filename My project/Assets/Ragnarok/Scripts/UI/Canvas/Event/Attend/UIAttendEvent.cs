using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIAttendEvent : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] PopupView popupView;
        [SerializeField] AttendEventView attendEventView;

        AttendEventPresenter presenter;

        protected override void OnInit()
        {
            presenter = new AttendEventPresenter();

            popupView.OnExit += OnBack;
            popupView.OnConfirm += OnBack;
            attendEventView.OnSelect += presenter.RequestAttendEventReward;

            presenter.OnUpdateAttendEventReward += UpdateAttendEventView;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            popupView.OnExit -= OnBack;
            popupView.OnConfirm -= OnBack;
            attendEventView.OnSelect -= presenter.RequestAttendEventReward;

            presenter.OnUpdateAttendEventReward -= UpdateAttendEventView;

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            if(!presenter.HasEventData())
            {
                UI.Close<UIAttendEvent>();
                return;
            }
            UpdateAttendEventView();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._6800; // 14일 출석 이벤트
            popupView.ConfirmLocalKey = LocalizeKey._1; // 확인
        }

        private void UpdateAttendEventView()
        {
            attendEventView.SetData(presenter.GetData());
            attendEventView.SetData(presenter.GetArrayData());
        }
    }
}