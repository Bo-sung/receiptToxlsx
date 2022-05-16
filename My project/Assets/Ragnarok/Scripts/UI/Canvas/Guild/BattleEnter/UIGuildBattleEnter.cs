using MEC;
using Ragnarok.View;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGuildBattleEnter : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single;

        private const int TAB_ENTER = 0; // 길드탐색
        private const int TAB_INFO = 1; // 전투진형
        private const int TAB_HISTORY = 2; // 전투정보

        [SerializeField] TitleView titleView;
        [SerializeField] UITabHelper tab;
        [SerializeField] UILabelValue labelRemainTime;
        [SerializeField] GuildBattleEnterView guildBattleEnterView;
        [SerializeField] GuildBattleInfoView guildBattleInfoView;
        [SerializeField] GuildBattleHistoryView guildBattleHistoryView;
        [SerializeField] GuildBattleEntryPopupView guildBattleEntryPopupView;
        [SerializeField] GuildBattleMyGuildPopupView guildBattleMyGuildPopupView;

        [Header("Bottom-Right")]
        [SerializeField] UIButtonHelper btnReward;
        [SerializeField] UIButtonHelper btnRank;
        [SerializeField] UIButtonHelper btnCupet;

        GuildBattleEnterPresenter presenter;

        protected override void OnInit()
        {
            presenter = new GuildBattleEnterPresenter();

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);
            guildBattleEnterView.Initialize(presenter.requestGuildListDelay, presenter.dailyEntryCount);

            tab.OnSelect += OnSelectTab;
            guildBattleEnterView.OnRefresh += presenter.RequestGuildList;
            guildBattleEnterView.OnSelect += presenter.SelectGuild;
            guildBattleInfoView.OnSelectTurretCupet += presenter.ChangeTurretCupet;
            guildBattleHistoryView.OnSelectDetail += guildBattleMyGuildPopupView.Show;
            guildBattleHistoryView.OnSelectAttackerInfo += presenter.ShowCharacterInfo;
            guildBattleEntryPopupView.OnCancel += guildBattleEntryPopupView.Hide;
            guildBattleEntryPopupView.OnExit += guildBattleEntryPopupView.Hide;
            guildBattleEntryPopupView.OnSelectAgent += ShowSelectAgent;
            guildBattleEntryPopupView.OnUnselectAgent += presenter.UnselectAgent;
            guildBattleEntryPopupView.OnSelectCupet += presenter.ChangeTurretCupet;
            guildBattleEntryPopupView.OnSelectOtherCupet += presenter.ShowOtherCupetInfo;
            guildBattleEntryPopupView.OnConfirm += presenter.RequestStartBattle;
            guildBattleMyGuildPopupView.OnConfirm += guildBattleMyGuildPopupView.Hide;
            guildBattleMyGuildPopupView.OnExit += guildBattleMyGuildPopupView.Hide;
            guildBattleMyGuildPopupView.OnShowMyDefenseInfo += ShowMyDefenseInfo;
            guildBattleMyGuildPopupView.OnShowMyRankInfo += ShowMyRankInfo;
            guildBattleMyGuildPopupView.OnSelectCharacterInfo += presenter.ShowCharacterInfo;
            EventDelegate.Add(btnReward.OnClick, OnClickedBtnReward);
            EventDelegate.Add(btnRank.OnClick, OnClickedBtnRank);
            EventDelegate.Add(btnCupet.OnClick, OnClickedBtnCupet);

            presenter.OnUpdateZeny += UpdateZeny;
            presenter.OnUpdateCatCoin += UpdateCatCoin;
            presenter.OnRequestGuildList += RefreshGuildList;
            presenter.OnRequestGuildBattleAttackPositionInfo += RefreshGuildInfo;
            presenter.OnRequestGuildBattleCupetSettings += RefreshCupetInfo;
            presenter.OnRequestGuildBattleDefenseInfo += RefreshGuildHistory;
            presenter.OnRequestGuildBattleListDetailInfo += ShowEntryPopup;
            presenter.OnSelectAgentInfo += RefreshAgentInfo;
            presenter.OnRequestMyGuildDefenseInfo += RefreshMyGuildBattleDefenseInfo;
            presenter.OnRequestMyGuildRankInfo += RefreshMyGuildBattleRankInfo;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnUpdateZeny -= UpdateZeny;
            presenter.OnUpdateCatCoin -= UpdateCatCoin;
            presenter.OnRequestGuildList -= RefreshGuildList;
            presenter.OnRequestGuildBattleAttackPositionInfo -= RefreshGuildInfo;
            presenter.OnRequestGuildBattleCupetSettings -= RefreshCupetInfo;
            presenter.OnRequestGuildBattleDefenseInfo -= RefreshGuildHistory;
            presenter.OnRequestGuildBattleListDetailInfo -= ShowEntryPopup;
            presenter.OnSelectAgentInfo -= RefreshAgentInfo;
            presenter.OnRequestMyGuildDefenseInfo -= RefreshMyGuildBattleDefenseInfo;
            presenter.OnRequestMyGuildRankInfo -= RefreshMyGuildBattleRankInfo;

            tab.OnSelect -= OnSelectTab;
            guildBattleEnterView.OnRefresh -= presenter.RequestGuildList;
            guildBattleEnterView.OnSelect -= presenter.SelectGuild;
            guildBattleInfoView.OnSelectTurretCupet -= presenter.ChangeTurretCupet;
            guildBattleHistoryView.OnSelectDetail -= guildBattleMyGuildPopupView.Show;
            guildBattleHistoryView.OnSelectAttackerInfo -= presenter.ShowCharacterInfo;
            guildBattleEntryPopupView.OnCancel -= guildBattleEntryPopupView.Hide;
            guildBattleEntryPopupView.OnExit -= guildBattleEntryPopupView.Hide;
            guildBattleEntryPopupView.OnSelectAgent -= ShowSelectAgent;
            guildBattleEntryPopupView.OnUnselectAgent -= presenter.UnselectAgent;
            guildBattleEntryPopupView.OnSelectCupet -= presenter.ChangeTurretCupet;
            guildBattleEntryPopupView.OnSelectOtherCupet -= presenter.ShowOtherCupetInfo;
            guildBattleEntryPopupView.OnConfirm -= presenter.RequestStartBattle;
            guildBattleMyGuildPopupView.OnConfirm -= guildBattleMyGuildPopupView.Hide;
            guildBattleMyGuildPopupView.OnExit -= guildBattleMyGuildPopupView.Hide;
            guildBattleMyGuildPopupView.OnShowMyDefenseInfo -= ShowMyDefenseInfo;
            guildBattleMyGuildPopupView.OnShowMyRankInfo -= ShowMyRankInfo;
            guildBattleMyGuildPopupView.OnSelectCharacterInfo -= presenter.ShowCharacterInfo;
            EventDelegate.Remove(btnReward.OnClick, OnClickedBtnReward);
            EventDelegate.Remove(btnRank.OnClick, OnClickedBtnRank);
            EventDelegate.Remove(btnCupet.OnClick, OnClickedBtnCupet);
        }

        protected override void OnShow(IUIData data = null)
        {
            guildBattleEntryPopupView.Hide(); // 입장 팝업 닫기
            guildBattleMyGuildPopupView.Hide(); // 내길드정보 팝업 닫기

            presenter.RequestGuildCupetInfo(); // 큐펫 정보 목록 요청
            UpdateRemainTimeGuildBattle();

            UpdateEnterView();
            RefreshGuildList();
            RefreshGuildInfo();
            RefreshGuildHistory();
            RefreshAgentInfo();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            titleView.ShowTitle(LocalizeKey._33700.ToText()); // 길드전

            tab[TAB_ENTER].LocalKey = LocalizeKey._33701; // 길드 탐색
            tab[TAB_INFO].LocalKey = LocalizeKey._33702; // 전투 진형
            tab[TAB_HISTORY].LocalKey = LocalizeKey._33703; // 전투 정보

            labelRemainTime.TitleKey = LocalizeKey._33707; // 길드전 남은 시간

            btnReward.LocalKey = LocalizeKey._33705; // 보상
            btnRank.LocalKey = LocalizeKey._33706; // 랭킹
            btnCupet.LocalKey = LocalizeKey._33704; // 큐펫
        }

        void OnSelectTab(int index)
        {
            guildBattleEnterView.SetActive(index == TAB_ENTER);
            guildBattleInfoView.SetActive(index == TAB_INFO);
            guildBattleHistoryView.SetActive(index == TAB_HISTORY);

            switch (index)
            {
                case TAB_INFO:
                    presenter.RequestGuildBattleAttackPosition();
                    break;

                case TAB_HISTORY:
                    presenter.RequestGuildBattleDefInfo();
                    break;
            }
        }

        void ShowSelectAgent()
        {
            UI.Show<UIGuildSupportSelect>().Set(presenter.SelectAgent);
        }

        void OnClickedBtnReward()
        {
            UI.Show<UIGuildBattleReward>();
        }

        void OnClickedBtnRank()
        {
            UI.Show<UIGuildRank>().Set(UIGuildRank.GuildRankType.GuildBattle);
        }

        void OnClickedBtnCupet()
        {
            UI.Show<UICupet>();
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
        /// 길드 리스트 반환
        /// </summary>
        private void RefreshGuildList()
        {
            // 길드전 입장 실패로 인하여 길드 리스트를 반환할 때가 있음
            if (guildBattleEntryPopupView.IsShow)
            {
                guildBattleEntryPopupView.Hide();
            }

            guildBattleEnterView.SetData(presenter.GetGuildList());
        }

        /// <summary>
        /// 큐펫 세팅
        /// </summary>
        private void RefreshCupetInfo()
        {
            guildBattleInfoView.UpdateCupet(presenter.GetCupets());
            guildBattleEntryPopupView.UpdateCupet(presenter.GetCupets());
        }

        /// <summary>
        /// 길드 세팅
        /// </summary>
        private void RefreshGuildInfo()
        {
            RefreshCupetInfo();
            guildBattleInfoView.SetBuff(presenter.GetBuffs());
        }

        /// <summary>
        /// 길드 전투정보 세팅
        /// </summary>
        private void RefreshGuildHistory()
        {
            guildBattleHistoryView.SetGuild(presenter.GetMyGuildInfo());
            guildBattleHistoryView.SetHistories(presenter.GetHistories());
        }

        /// <summary>
        /// 동료 세팅
        /// </summary>
        private void RefreshAgentInfo()
        {
            guildBattleEntryPopupView.UpdateAgent(presenter.GetHasAgent(), presenter.GetAgentProfileName(), presenter.GetAgentJobIconName());
        }

        /// <summary>
        /// 타겟 길드 세팅
        /// </summary>
        private void ShowEntryPopup()
        {
            guildBattleEntryPopupView.SetTargetGuildData(presenter.GetTargetGuild(), presenter.GetLeftCupets(), presenter.GetRightCupets());
            guildBattleEntryPopupView.Show();
        }

        /// <summary>
        /// 내 길드 수비 진형 세팅
        /// </summary>
        private void RefreshMyGuildBattleDefenseInfo()
        {
            guildBattleMyGuildPopupView.SetMyDefenseInfo(presenter.GetMyGuildInfo(), presenter.GetMyLeftCupets(), presenter.GetMyRightCupets());
        }

        /// <summary>
        /// 내 길드 수비 진형 정보 보기
        /// </summary>
        private void ShowMyDefenseInfo()
        {
            RefreshMyGuildBattleDefenseInfo();
            presenter.RequestGuildBattleDefCupetInfo();
        }

        /// <summary>
        /// 내 길드원 랭킹 정보 세팅
        /// </summary>
        private void RefreshMyGuildBattleRankInfo()
        {
            guildBattleMyGuildPopupView.SetMyRankInfo(presenter.GetMyRanks());
        }

        /// <summary>
        /// 내 길드원 랭킹 정보 보기
        /// </summary>
        private void ShowMyRankInfo()
        {
            RefreshMyGuildBattleRankInfo();
            presenter.RequestGuildBattleGuildCharRank();
        }

        /// <summary>
        /// 길드전 남은 시간
        /// </summary>
        private void UpdateRemainTimeGuildBattle()
        {
            Timing.RunCoroutineSingleton(YieldGuildBattleRequestRemainTime(presenter.GetRemainTimeGuildBattle()).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        IEnumerator<float> YieldGuildBattleRequestRemainTime(RemainTime remainTime)
        {
            while (remainTime.ToRemainTime() > 0)
            {
                labelRemainTime.Value = remainTime.ToRemainTime().ToStringTimeConatinsDay();
                yield return Timing.WaitForSeconds(0.1f);
            }

            labelRemainTime.Value = "00:00:00";

            UI.ConfirmPopup(LocalizeKey._33819.ToText(), CloseUI); // 길드전이 종료되었습니다.
        }

        private void UpdateEnterView()
        {
            guildBattleEnterView.SetProfile(presenter.GetProfileName());
            guildBattleEnterView.SetJobIcon(presenter.GetJobIconName());
            guildBattleEnterView.SetCharacterName(presenter.GetJobLevel(), presenter.GetCharacterName());
            guildBattleEnterView.SetBattleScore(presenter.GetBattleScore());
            guildBattleEnterView.SetTotalDamage(presenter.GetTotalDamage());
            guildBattleEnterView.RemainCount(presenter.GetRemainCount());
        }

        private void CloseUI()
        {
            UI.Close<UIGuildBattleEnter>();
        }

        protected override void OnBack()
        {
            if (guildBattleEntryPopupView.IsShow)
            {
                guildBattleEntryPopupView.Hide();
                return;
            }

            if (guildBattleMyGuildPopupView.IsShow)
            {
                guildBattleMyGuildPopupView.Hide();
                return;
            }

            base.OnBack();
        }
    }
}