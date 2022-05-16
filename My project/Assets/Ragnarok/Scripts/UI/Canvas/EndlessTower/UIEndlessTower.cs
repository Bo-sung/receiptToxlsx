using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIEndlessTower : UICanvas
    {
        protected override UIType uiType => UIType.Single | UIType.Back | UIType.Hide;

        [SerializeField] TitleView titleView;
        [SerializeField] EndlessTowerView endlessTowerView;
        [SerializeField] EndlessTowerFloorSelectView endlessTowerFloorSelectView;
        [SerializeField] AgentSelectView agentSelectView;

        EndlessTowerPresenter presenter;

        protected override void OnInit()
        {
            presenter = new EndlessTowerPresenter();

            agentSelectView.OnSelectAgent += ShowAgentUI;
            endlessTowerFloorSelectView.OnSelectFloor += endlessTowerView.Move;
            endlessTowerFloorSelectView.OnSelectHelp += ShowHelpUI;
            endlessTowerFloorSelectView.OnSelectEnter += presenter.StartBattle;

            presenter.OnUpdateZeny += UpdateZeny;
            presenter.OnUpdateCatCoin += UpdateCatCoin;
            presenter.OnUpdateEquippedAgent += RefreshCombatAgents;
            presenter.OnUpdateEquippedAgent += RefreshAgentNotice;
            presenter.OnUpdateNewAgent += RefreshAgentNotice;
            presenter.OnUpdateEndlessTowerFreeTicket += RefreshFreeEntryCount;
            presenter.OnUpdateTicketCount += RefreshTicketCount;

            presenter.AddEvent();

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            agentSelectView.OnSelectAgent -= ShowAgentUI;
            endlessTowerFloorSelectView.OnSelectFloor -= endlessTowerView.Move;
            endlessTowerFloorSelectView.OnSelectHelp -= ShowHelpUI;
            endlessTowerFloorSelectView.OnSelectEnter -= presenter.StartBattle;

            presenter.OnUpdateZeny -= UpdateZeny;
            presenter.OnUpdateCatCoin -= UpdateCatCoin;
            presenter.OnUpdateEquippedAgent -= RefreshCombatAgents;
            presenter.OnUpdateEquippedAgent -= RefreshAgentNotice;
            presenter.OnUpdateNewAgent -= RefreshAgentNotice;
            presenter.OnUpdateEndlessTowerFreeTicket -= RefreshFreeEntryCount;
            presenter.OnUpdateTicketCount -= RefreshTicketCount;
        }

        protected override void OnShow(IUIData data = null)
        {
            endlessTowerView.SetData(presenter.GetClearedFloor(), presenter.GetFloorData()); // 층 정보 세팅
            endlessTowerFloorSelectView.Initialize(presenter.GetSkipFloorData(), presenter.GetTicketItemIcon()); // 층 별 스킵재료 세팅

            RefreshCombatAgents();
            RefreshAgentNotice();
            RefreshFreeEntryCount();
            RefreshTicketCount();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            titleView.ShowTitle(DungeonType.EnlessTower.ToText()); // 엔들리스 타워
        }

        void ShowAgentUI()
        {
            if (!presenter.IsOpenAgent())
                return;

            UI.Show<UIAgent>(new UIAgent.Input() { viewAgentType = AgentType.CombatAgent });
        }

        void ShowHelpUI()
        {
            int dungeonInfoId = DungeonInfoType.EndlessTower.GetDungeonInfoId();
            UI.Show<UIDungeonInfoPopup>().Show(dungeonInfoId);
        }

        /// <summary>
        /// 제니 업데이트
        /// </summary>
        private void UpdateZeny(long zeny)
        {
            titleView.ShowZeny(zeny);
        }

        /// <summary>
        /// 캣코인 업데이트
        /// </summary>
        private void UpdateCatCoin(long catCoin)
        {
            titleView.ShowCatCoin(catCoin);
        }

        /// <summary>
        /// 전투동료 업데이트
        /// </summary>
        private void RefreshCombatAgents()
        {
            agentSelectView.SetCombatAgents(presenter.GetCombatAgents(), presenter.GetCombatAgentSlotCount());
        }

        /// <summary>
        /// 전투동료 알림 업데이트
        /// </summary>
        private void RefreshAgentNotice()
        {
            bool isNotice = presenter.CanEquipAgent();
            agentSelectView.UpdateNotice(isNotice);
        }

        /// <summary>
        /// 무료입장 수 업데이트
        /// </summary>
        private void RefreshFreeEntryCount()
        {
            endlessTowerFloorSelectView.SetFreeEntryCount(presenter.GetFreeEntryCount(), presenter.GetMaxFreeEntryCount()); // 무료입장 수 세팅
            endlessTowerFloorSelectView.SetFreeEntryCooldownTime(presenter.GetFreeEntryCoolTime()); // 쿨타임 세팅
        }

        /// <summary>
        /// 입장권 수 업데이트
        /// </summary>
        private void RefreshTicketCount()
        {
            endlessTowerFloorSelectView.SetTicketCount(presenter.GetTicketCount());
        }

        protected override void OnBack()
        {
            UIDungeon.viewType = UIDungeon.ViewType.None;
            UI.Show<UIDungeon>();
        }
    }
}