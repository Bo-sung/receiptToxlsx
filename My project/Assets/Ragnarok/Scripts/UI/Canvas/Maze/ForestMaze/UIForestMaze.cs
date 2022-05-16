using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIForestMaze : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] SimplePopupView simplePopupView;
        [SerializeField] ForestMazeView forestMazeView;

        ForestMazePresenter presenter;

        protected override void OnInit()
        {
            presenter = new ForestMazePresenter();

            simplePopupView.OnExit += CloseUI;
            forestMazeView.OnSelectEnter += presenter.StartBattle;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            simplePopupView.OnExit -= CloseUI;
            forestMazeView.OnSelectEnter -= presenter.StartBattle;
        }

        protected override void OnShow(IUIData data = null)
        {
            forestMazeView.Initialize(presenter.GetData(), presenter.GetTicketItemIcon());
            forestMazeView.SetRecommandPower(presenter.GetBattleScore());
            forestMazeView.SetFreeEntryCount(presenter.GetFreeEntryCount());
            forestMazeView.SetTicketCount(presenter.GetTicketCount());
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            simplePopupView.MainTitleLocalKey = LocalizeKey._39600; // 미궁숲 입장
        }

        void CloseUI()
        {
            UI.Close<UIForestMaze>();
        }
    }
}