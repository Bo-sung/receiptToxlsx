using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIDarkMaze : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] PopupView popupView;
        [SerializeField] DarkMazeView darkMazeView;

        DarkMazePresenter presenter;

        protected override void OnInit()
        {
            presenter = new DarkMazePresenter();

            popupView.OnConfirm += presenter.StartEventMaze;
            popupView.OnExit += OnBack;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            popupView.OnConfirm -= presenter.StartEventMaze;
            popupView.OnExit -= OnBack;
        }

        protected override void OnShow(IUIData data = null)
        {
            darkMazeView.Initialize(presenter.GetDailyReward(), presenter.GetClearReward(), presenter.GetIsReceivedDailyReward());
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._39700; // 이벤트 미궁 입장
            popupView.ConfirmLocalKey = LocalizeKey._39709; // 입장
        }

        public void Show(int mazeId)
        {
            presenter.SetMazeId(mazeId);
        }
    }
}