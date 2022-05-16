using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIGuildMain : UICanvas, GuildMainPresenter.IView, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy | UIType.Single;

        [SerializeField] TitleView titleView;
        [SerializeField] UITabHelper tabMain;

        [SerializeField] GuildInfoSubView guildInfoView;
        [SerializeField] GuildMemberBaseView guildMemberView;
        [SerializeField] GuildBoardSubView guildBoardView;
        [SerializeField] GuildSkillSubView guildSkillView;

        // 하단
        [SerializeField] UIButtonHelper btnGuildLobby;

        GuildMainPresenter presenter;
        UISubCanvas currentSubCanvas;

        protected override void OnInit()
        {
            presenter = new GuildMainPresenter(this);
            guildMemberView.Initialize(presenter);
            guildSkillView.Initialize(presenter);
            presenter.AddEvent();

            presenter.OnTamingMazeOpen += UpdateGuildLobbyNotice;
            presenter.OnPurchaseSuccess += UpdateGuildLobbyNotice;
            presenter.OnResetFreeItemBuyCount += UpdateGuildLobbyNotice;

            titleView.Initialize(TitleView.FirstCoinType.GuildCoin, TitleView.SecondCoinType.CatCoin);

            EventDelegate.Add(tabMain[0].OnChange, ShowGuildInfoView);
            EventDelegate.Add(tabMain[1].OnChange, ShowGuildMemberView);
            EventDelegate.Add(tabMain[2].OnChange, ShowGuildBoardView);
            EventDelegate.Add(tabMain[3].OnChange, ShowGuildSkillView);
            EventDelegate.Add(btnGuildLobby.OnClick, OnClickedBtnGuildLobby);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnTamingMazeOpen -= UpdateGuildLobbyNotice;
            presenter.OnPurchaseSuccess -= UpdateGuildLobbyNotice;
            presenter.OnResetFreeItemBuyCount -= UpdateGuildLobbyNotice;

            EventDelegate.Remove(tabMain[0].OnChange, ShowGuildInfoView);
            EventDelegate.Remove(tabMain[1].OnChange, ShowGuildMemberView);
            EventDelegate.Remove(tabMain[2].OnChange, ShowGuildBoardView);
            EventDelegate.Remove(tabMain[3].OnChange, ShowGuildSkillView);
            EventDelegate.Remove(btnGuildLobby.OnClick, OnClickedBtnGuildLobby);
        }

        protected override void OnShow(IUIData data = null)
        {
            currentSubCanvas = guildInfoView;
            titleView.ShowGuildCoin(presenter.GuildCoin);
            titleView.ShowCatCoin(presenter.CatCoin);

            if (tabMain[0].Value != true)
            {
                tabMain[0].Value = true;
            }
            else
            {
                ShowSubCanvas(currentSubCanvas);
            }
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            titleView.ShowTitle(LocalizeKey._33000.ToText()); // 길드
            tabMain[0].LocalKey = LocalizeKey._33030; // 길드 정보
            tabMain[1].LocalKey = LocalizeKey._33031; // 길드원
            tabMain[2].LocalKey = LocalizeKey._33032; // 게시판
            tabMain[3].LocalKey = LocalizeKey._33033; // 스킬
            btnGuildLobby.LocalKey = LocalizeKey._33133; // 스퀘어
        }

        public void Refresh()
        {
            if (currentSubCanvas == null)
                return;

            currentSubCanvas.Show();
            UpdateGuildLobbyNotice();
        }

        private void UpdateGuildLobbyNotice()
        {
            btnGuildLobby.SetNotice(presenter.HasGuildLobbyNotice());
        }

        private void OnClickedBtnGuildLobby()
        {
            presenter.EnterGuildLobby().WrapNetworkErrors();
        }

        private void ShowGuildInfoView()
        {
            if (!UIToggle.current.value)
                return;

            ShowSubCanvas(guildInfoView);
        }

        private async void ShowGuildMemberView()
        {
            if (!UIToggle.current.value)
                return;

            await presenter.RequestJoinSubmitUserList();

            ShowSubCanvas(guildMemberView);
        }

        private async void ShowGuildBoardView()
        {
            if (!UIToggle.current.value)
                return;

            await presenter.RequestGuildBoardList();

            ShowSubCanvas(guildBoardView);
        }

        public async void ShowGuildSkillView()
        {
            if (!UIToggle.current.value)
                return;

            await presenter.RequestSkillList();
            ShowSubCanvas(guildSkillView);
        }

        private void ShowSubCanvas(UISubCanvas subCanvas)
        {
            currentSubCanvas = subCanvas;
            HideAllSubCanvas();
            Refresh();
        }

        protected override void HideAllSubCanvas()
        {
            guildInfoView.Hide();
            guildMemberView.Hide();
            guildBoardView.Hide();
            guildSkillView.Hide();
        }

        void GuildMainPresenter.IView.SetGuildCoin(long value)
        {
            titleView.ShowGuildCoin(value);
        }

        void GuildMainPresenter.IView.SetCatCoin(long value)
        {
            titleView.ShowCatCoin(value);
        }
    }
}