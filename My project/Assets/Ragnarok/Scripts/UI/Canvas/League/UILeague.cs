using MEC;
using Ragnarok.View;
using Ragnarok.View.League;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UILeague : UICanvas, ILeagueCanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single;

        [SerializeField] TitleView titleView;
        [SerializeField] NpcView npcView;
        [SerializeField] LeagueModelView leagueModelView;
        [SerializeField] LeagueModeSelectView leagueModeSelectView;
        [SerializeField] LeagueEntryPopupView leagueEntryPopupView;
        [SerializeField] LeagueResultPopupView leagueResultPopupView;
        [SerializeField] UIButtonHelper btnHelp;

        LeaguePresenter presenter;

        protected override void OnInit()
        {
            presenter = new LeaguePresenter(this);

            leagueModelView.OnSelect += OnSelect;
            leagueModelView.OnSelectEntry += OnSelectEntry;
            leagueModelView.OnSelectAgent += OnSelectAgent;
            leagueModelView.OnSelectRankTab += presenter.OnSelectRankTab;
            leagueModelView.OnShowNextRankingPage += OnShowNextRankingPage;
            leagueModelView.OnSelectProfile += presenter.RequestOtherCharacterInfo;
            leagueModelView.OnSelectRankRewardTab += OnSelectRankRewardTab;

            leagueModeSelectView.OnSelectMode += OnSelectMode;

            leagueEntryPopupView.OnConfirm += OnConfirm;

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);

            presenter.OnUpdateNewAgent += UpdateAgentNotice;
            presenter.OnUpdateEquippedAgent += UpdateAgentNotice;
            presenter.OnUpdateZeny += UpdateZeny;
            presenter.OnUpdateCatCoin += UpdateCatCoin;
            presenter.AddEvent();

            EventDelegate.Add(btnHelp.OnClick, OnClickBtnHelp);
        }

        protected override void OnClose()
        {
            leagueModelView.OnSelect -= OnSelect;
            leagueModelView.OnSelectEntry -= OnSelectEntry;
            leagueModelView.OnSelectAgent -= OnSelectAgent;
            leagueModelView.OnSelectRankTab -= presenter.OnSelectRankTab;
            leagueModelView.OnShowNextRankingPage -= OnShowNextRankingPage;
            leagueModelView.OnSelectProfile -= presenter.RequestOtherCharacterInfo;
            leagueModelView.OnSelectRankRewardTab -= OnSelectRankRewardTab;

            leagueModeSelectView.OnSelectMode -= OnSelectMode;

            leagueEntryPopupView.OnConfirm -= OnConfirm;

            presenter.OnUpdateNewAgent -= UpdateAgentNotice;
            presenter.OnUpdateEquippedAgent -= UpdateAgentNotice;
            presenter.OnUpdateZeny -= UpdateZeny;
            presenter.OnUpdateCatCoin -= UpdateCatCoin;
            presenter.RemoveEvent();

            EventDelegate.Remove(btnHelp.OnClick, OnClickBtnHelp);
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.RemoveNewOpenContent_Pvp(); // 신규 컨텐츠 플래그 제거

            leagueModelView.Show(); // 토글 시작 정보 포함
            leagueModeSelectView.Hide(); // Popup - 시작과 동시에 Hide
            leagueEntryPopupView.Hide(); // Popup - 시작과 동시에 Hide
            leagueResultPopupView.Hide(); // Popup - 시작과 동시에 Hide           

            UpdateAgentNotice();
        }

        protected override void OnHide()
        {
            leagueModelView.Hide(); // 토글 시작 정보 포함

            presenter.ResetServerInfo(); // 서버 정보 초기화
            Timing.KillCoroutines(gameObject);
        }

        protected override void OnLocalize()
        {
            // Title 언어 세팅
            titleView.ShowTitle(LocalizeKey._47000.ToText()); // 대전

            // Npc 언어 세팅
            npcView.ShowNpcName(LocalizeKey._82000.ToText()); // 대전 안내원
            npcView.AddTalkLocalKey(LocalizeKey._83000); // 상대와의 대전을 통해\n당신의 힘을 증명하세요!
        }

        private void OnClickBtnHelp()
        {
            int info_id = DungeonInfoType.League.GetDungeonInfoId();
            UI.Show<UIDungeonInfoPopup>().Show(info_id);
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
        /// 동료 알림 업데이트
        /// </summary>
        private void UpdateAgentNotice()
        {
            bool isNotice = presenter.CanEquipAgent();
            leagueModelView.SetActiveAgentNotice(isNotice);
        }

        /// <summary>
        /// 리그 메인 정보 세팅
        /// </summary>
        void ILeagueCanvas.SetData(UILeagueMainInfo.IInput input)
        {
            leagueModelView.SetData(input);

            if (input.SeasonCloseTime.ToRemainTime() > 0f)
            {
                Timing.RunCoroutine(YieldRefreshCloseTime(input.SeasonCloseTime), gameObject); // 시즌 종료까지 남은 시간 보여주기
            }
            else if (input.SeasonOpenTime.ToRemainTime() > 0f)
            {
                Timing.RunCoroutine(YieldRefreshOpenTime(input.SeasonOpenTime), gameObject); // 시즌 오픈까지 남은 시간 보여주기
            }
        }

        /// <summary>
        /// 리그 랭킹 정보 세팅
        /// </summary>
        void ILeagueCanvas.SetData(UILeagueRankInfo.IInput input)
        {
            leagueModelView.SetData(input);
        }

        /// <summary>
        /// 받은 보상 표시
        /// </summary>
        void ILeagueCanvas.ShowRewardPopup(LeagueResultPopupView.IInput input)
        {
            leagueResultPopupView.Show(input);
        }

        void OnSelect(LeagueModelView.ViewType viewType)
        {
            npcView.PlayTalk();

            switch (viewType)
            {
                case LeagueModelView.ViewType.Main:
                    presenter.RequestPveInfo(); // 대전 정보 호출
                    break;

                case LeagueModelView.ViewType.GradeReward:
                    leagueModelView.SetRewardInfo(presenter.GetGradeRewardInfos());
                    break;
            }
        }

        void OnSelectEntry()
        {
            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return;
            }

            // 미궁섬에서 대전 불가능
            if (presenter.IsCheckMultiLobby())
                return;

            // 대전 컨텐츠 오픈 체크
            if (!presenter.IsOpendLeague())
            {
                presenter.ShowOpenTimeMessage();
                return;
            }

            // 티켓 체크
            if (!presenter.IsTicket())
                return;

            leagueModeSelectView.Show();
        }

        void OnSelectAgent()
        {
            ShowCombatAgentUI();
        }

        void OnShowNextRankingPage()
        {
            presenter.RequestNextPveRank(); // 랭킹 정보 호출 - 다음 페이지
        }

        void OnSelectRankRewardTab(int index)
        {
            leagueModelView.SetRankRewardInfo(presenter.GetRankRewardInfos(index));
        }

        void OnSelectMode(bool isSingle)
        {
            presenter.SetSelectMode(isSingle);
            CheckEquipAgent().WrapNetworkErrors();
        }

        void OnConfirm(int count)
        {
            presenter.StartBattlePve(count);
        }

        private async Task CheckEquipAgent()
        {
            leagueModeSelectView.Hide();

            // 동료 장착이 가능한 경우
            if (presenter.CanEquipAgent() && !presenter.IsSingle())
            {
                // 장착 가능한 PVP 동료가 있습니다.\n\n현재 상태로 진행하시겠습니까?
                // [동료 장착 바로가기]
                if (!await UI.SelectShortCutPopup(LocalizeKey._90173.ToText(), LocalizeKey._90174.ToText(), ShowCombatAgentUI))
                    return;
            }

            leagueEntryPopupView.Show(presenter.LeagueTicketCount, presenter.LeagueFreeCount, presenter.LeagueFreeMaxCount);
        }

        private void ShowCombatAgentUI()
        {
            UI.Show<UIAgent>(new UIAgent.Input() { viewAgentType = AgentType.CombatAgent });
        }

        IEnumerator<float> YieldRefreshCloseTime(RemainTime seasonCloseTime)
        {
            while (true)
            {
                float remainTime = seasonCloseTime.ToRemainTime();
                bool isRemainTime = remainTime > 0f;
                TimeSpan timeSpan = isRemainTime ? remainTime.ToTimeSpan() : TimeSpan.Zero;

                string notice = LocalizeKey._47001.ToText() // 시즌 종료일 까지: {DAYS}일 {HOURS}시간 {MINUTES}분 {SECONDS}초
                    .Replace(ReplaceKey.DAYS, timeSpan.Days)
                    .Replace(ReplaceKey.HOURS, timeSpan.Hours)
                    .Replace(ReplaceKey.MINUTES, timeSpan.Minutes)
                    .Replace(ReplaceKey.SECONDS, timeSpan.Seconds);

                npcView.ShowNotice(notice); // 공지사항 세팅

                if (!isRemainTime)
                    break;

                yield return Timing.WaitForSeconds(1f);
            }

            presenter.ResetServerInfo(); // 기존 서버 정보 초기화
        }

        IEnumerator<float> YieldRefreshOpenTime(RemainTime seasonOpenTime)
        {
            while (true)
            {
                float remainTime = seasonOpenTime.ToRemainTime();
                bool isRemainTime = remainTime > 0f;
                TimeSpan timeSpan = isRemainTime ? remainTime.ToTimeSpan() : TimeSpan.Zero;

                string notice = LocalizeKey._47002.ToText() // 시즌 오픈일 까지: {DAYS}일 {HOURS}시간 {MINUTES}분 {SECONDS}초
                    .Replace(ReplaceKey.DAYS, timeSpan.Days)
                    .Replace(ReplaceKey.HOURS, timeSpan.Hours)
                    .Replace(ReplaceKey.MINUTES, timeSpan.Minutes)
                    .Replace(ReplaceKey.SECONDS, timeSpan.Seconds);

                npcView.ShowNotice(notice); // 공지사항 세팅

                if (!isRemainTime)
                    break;

                yield return Timing.WaitForSeconds(1f);
            }

            presenter.ResetServerInfo(); // 기존 서버 정보 초기화
        }

        protected override void OnBack()
        {
            if (leagueResultPopupView.IsShow)
            {
                leagueResultPopupView.Hide();
                return;
            }

            if (leagueEntryPopupView.IsShow)
            {
                leagueEntryPopupView.Hide();
                return;
            }

            base.OnBack();
        }

        void ILeagueCanvas.CloseUI()
        {
            UI.Close<UILeague>();
        }
    }
}