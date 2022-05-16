using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGuildBattleRankEvent : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] SimplePopupView simplePopupView;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] GuildBattleRankView guildBattleRankView;
        [SerializeField] UIGuildBattleRankElement myRank;
        [SerializeField] UILabelHelper labelNoGuild;
        [SerializeField] UIButtonHelper btnReward;

        GuildBattleRankEventPresenter presenter;

        protected override void OnInit()
        {
            presenter = new GuildBattleRankEventPresenter();

            simplePopupView.OnExit += OnBack;
            guildBattleRankView.OnDragFinish += presenter.RequestNextPage;

            presenter.OnRankList += Refresh;
            presenter.AddEvent();

            EventDelegate.Add(btnReward.OnClick, OnClickedBtnReward);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            simplePopupView.OnExit -= OnBack;
            guildBattleRankView.OnDragFinish -= presenter.RequestNextPage;

            presenter.OnRankList -= Refresh;

            EventDelegate.Remove(btnReward.OnClick, OnClickedBtnReward);
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.RequestGuildRank();
            Refresh();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            simplePopupView.MainTitleLocalKey = LocalizeKey._38600; // 길드전 오픈 이벤트!
            labelDesc.LocalKey = LocalizeKey._38601; // 랭킹에 도전하고 특별한 보상을 받으세요!
            labelNoGuild.LocalKey = LocalizeKey._38605; // 길드에 가입되어 있지 않습니다.
            btnReward.LocalKey = LocalizeKey._38604; // 보상
        }

        private void Refresh()
        {
            guildBattleRankView.SetData(presenter.GetRankInfos());
            myRank.SetData(presenter.GetMyRankInfo());
            labelNoGuild.SetActive(!presenter.HasMyGuildRank());
        }

        private void OnClickedBtnReward()
        {
            UI.Show<UIGuildBattleRewardEvent>();
        }
    }
}