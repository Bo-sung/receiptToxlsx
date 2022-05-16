using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGate : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] GatePartySelectView gatePartySelectView;
        [SerializeField] TicketBuyView ticketBuyView;
        [SerializeField] GatePartyJoinView gatePartyJoinView;
        [SerializeField] GatePartyReadyView gatePartyReadyView;

        GatePresenter presenter;
        GatePartySelectView.SelectType savedType;

        private int savedChannelId;

        protected override void OnInit()
        {
            presenter = new GatePresenter();

            gatePartySelectView.OnSelect += OnSelectPartySelectView;
            ticketBuyView.OnSelectEnter += OnSelectBuyTicket;
            gatePartyJoinView.OnSelectRefresh += presenter.RequestGetAllRoomList;
            gatePartyJoinView.OnSelectUserInfo += presenter.ShowOtherUserInfo;
            gatePartyJoinView.OnSelectJoin += OnSelectJoin;
            gatePartyReadyView.OnSelectExit += presenter.RequestExitParty;
            gatePartyReadyView.OnSelectStart += presenter.RequestStartParty;
            gatePartyReadyView.OnSelectUserInfo += presenter.ShowOtherUserInfo;
            gatePartyReadyView.OnSelectUserBan += presenter.RequestUserBan;

            presenter.OnPartyReady += UpdatePartyReadyView;
            presenter.OnPartyList += UpdatePartyJoinView;
            presenter.OnPartyExit += UpdatePartyExit;
            presenter.OnUpdateMultiMazeTicket += UpdateRemainTicket;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnPartyReady -= UpdatePartyReadyView;
            presenter.OnPartyList -= UpdatePartyJoinView;
            presenter.OnPartyExit -= UpdatePartyExit;
            presenter.OnUpdateMultiMazeTicket -= UpdateRemainTicket;

            gatePartySelectView.OnSelect -= OnSelectPartySelectView;
            ticketBuyView.OnSelectEnter -= OnSelectBuyTicket;
            gatePartyJoinView.OnSelectRefresh -= presenter.RequestGetAllRoomList;
            gatePartyJoinView.OnSelectUserInfo -= presenter.ShowOtherUserInfo;
            gatePartyJoinView.OnSelectJoin -= OnSelectJoin;
            gatePartyReadyView.OnSelectExit -= presenter.RequestExitParty;
            gatePartyReadyView.OnSelectStart -= presenter.RequestStartParty;
            gatePartyReadyView.OnSelectUserInfo -= presenter.ShowOtherUserInfo;
            gatePartyReadyView.OnSelectUserBan -= presenter.RequestUserBan;
        }

        protected override void OnShow(IUIData data = null)
        {
            ticketBuyView.Hide();
            gatePartyJoinView.Hide();
            gatePartyReadyView.Hide();

            UpdateRemainTicket();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void Show(int id)
        {
            presenter.SetGateId(id);

            int titleLocalKey = presenter.GetNameId();
            int titleValueLocalKey = presenter.GetDescriptionId();
            string stageIconName = presenter.GetStageIcon();
            string monsterIconName = presenter.GetMonsterIcon();
            gatePartyJoinView.Initialize(titleLocalKey, titleValueLocalKey, stageIconName, monsterIconName);
            gatePartyReadyView.Initialize(titleLocalKey, titleValueLocalKey, stageIconName, monsterIconName);
        }

        void OnSelectPartySelectView(GatePartySelectView.SelectType type)
        {
            savedType = type;

            switch (savedType)
            {
                case GatePartySelectView.SelectType.Exit:
                    CloseUI();
                    break;

                case GatePartySelectView.SelectType.Create:
                    RequestCreateParty();
                    break;

                case GatePartySelectView.SelectType.Join:
                    RequestGetAllRoomList();
                    break;

                case GatePartySelectView.SelectType.Help:
                    UI.Show<UIGateReward>().Show(presenter.GetGateId());
                    break;
            }
        }

        void OnSelectBuyTicket()
        {
            ticketBuyView.Hide();

            int needCoin = presenter.GetNeedCatCoint();
            if (!CoinType.CatCoin.Check(needCoin))
                return;

            switch (savedType)
            {
                case GatePartySelectView.SelectType.Create:
                    presenter.RequestCreateParty(); // 방생성 요청
                    break;

                case GatePartySelectView.SelectType.Join:
                    presenter.RequestJoinParty(savedChannelId); // 방입장 요청
                    break;
            }
        }

        void OnSelectJoin(int channelId)
        {
            savedChannelId = channelId;
            if (!CanEnterGate())
                return;

            presenter.RequestJoinParty(savedChannelId);
        }

        private void UpdatePartyReadyView()
        {
            if (!gatePartyReadyView.IsShow)
                gatePartyReadyView.Show();

            gatePartyReadyView.SetData(presenter.GetMyPartyInfos());
            gatePartyReadyView.SetLeader(presenter.IsLeader());
        }

        private void UpdatePartyJoinView()
        {
            if (!gatePartyJoinView.IsShow)
                gatePartyJoinView.Show();

            gatePartyJoinView.SetData(presenter.GetPartyInfos());
        }

        private void UpdatePartyExit()
        {
            gatePartyReadyView.Hide();

            // 파티 가입을 통해서 들어왔을 경우
            if (savedType == GatePartySelectView.SelectType.Join)
            {
                RequestGetAllRoomList();
            }
        }

        private void UpdateRemainTicket()
        {
            gatePartySelectView.UpdateTicketCount(presenter.GetTicketCount(), presenter.GetTicketMaxCount());
        }

        private void RequestCreateParty()
        {
            if (!CanEnterGate())
                return;

            presenter.RequestCreateParty(); // 방생성 요청
        }

        private void RequestGetAllRoomList()
        {
            presenter.RequestGetAllRoomList(); // 방목록 요청
        }

        private void CloseUI()
        {
            UI.Close<UIGate>();
        }

        private bool CanEnterGate()
        {
            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return false;
            }

            // 무료 티켓 보유
            if (presenter.GetTicketCount() > 0)
                return true;

            ticketBuyView.Set(RewardType.MultiMazeTicket, presenter.GetNeedCatCoint());
            ticketBuyView.Show();
            return false;
        }

        protected override void OnBack()
        {
            if (ticketBuyView.IsShow)
            {
                ticketBuyView.Hide();
                return;
            }

            if (gatePartyJoinView.IsShow)
            {
                gatePartyJoinView.Hide();
                return;
            }

            if (gatePartyReadyView.IsShow)
            {
                gatePartyReadyView.TryExit();
                return;
            }

            base.OnBack();
        }

        #region Tutorial
        public UIWidget GetBtnCreateWidget()
        {
            return gatePartySelectView.GetBtnCreateWidget();
        }

        public UIWidget GetBtnJoinWidget()
        {
            return gatePartySelectView.GetBtnJoinWidget();
        }
        #endregion
    }
}