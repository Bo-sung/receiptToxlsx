using MEC;
using Ragnarok.View;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGuildBattleReady : UICanvas
    {
        private const int TAB_GUILD_BATTLE_READY = 0; // 길드전 신청 탭
        private const int TAB_GUILD_BATTLE_BUFF = 1; // 길드전 버프 탭

        protected override UIType uiType => UIType.Back | UIType.Destroy | UIType.Single;

        [SerializeField] TitleView titleView;
        [SerializeField] UITabHelper tab;
        [SerializeField] UILabelValue labelRemainTime;
        [SerializeField] GuildBattleReadyView guildBattleReadyView;
        [SerializeField] GuildBattleBuffView guildBattleBuffView;

        [Header("Bottom-Right")]
        [SerializeField] UIButtonHelper btnReward;
        [SerializeField] UIButtonHelper btnRank;
        [SerializeField] UIButtonHelper btnCupet;

        GuildBattleReadyPresenter presenter;

        protected override void OnInit()
        {
            presenter = new GuildBattleReadyPresenter();

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);

            tab.OnSelect += OnSelectTab;
            guildBattleReadyView.OnSelectBtnRequest += presenter.ReqeustGuildBattle;
            guildBattleReadyView.OnSelectLeftTurretCupet += presenter.OnSelectLeftTurretCupet;
            guildBattleReadyView.OnSelectRightTurretCupet += presenter.OnSelectRightTurretCupet;
            guildBattleBuffView.OnSelect += presenter.ShowBuffMaterialSelect;

            presenter.OnUpdateZeny += UpdateZeny;
            presenter.OnUpdateCatCoin += UpdateCatCoin;
            presenter.OnUpdateGuildBattleInfo += OnUpdateGuildBattleInfo;
            presenter.OnUpdateGuildBattleRequest += OnUpdateGuildBattleInfo;
            presenter.OnUpdateCupetList += UpdateTurret;
            presenter.OnUpdateGuildBattleBuff += UpdateGuildBattleBuff;

            presenter.AddEvent();

            EventDelegate.Add(btnReward.OnClick, OnClickedBtnReward);
            EventDelegate.Add(btnRank.OnClick, OnClickedBtnRank);
            EventDelegate.Add(btnCupet.OnClick, OnClickedBtnCupet);
        }

        protected override void OnClose()
        {
            tab.OnSelect -= OnSelectTab;
            guildBattleReadyView.OnSelectBtnRequest -= presenter.ReqeustGuildBattle;
            guildBattleReadyView.OnSelectLeftTurretCupet -= presenter.OnSelectLeftTurretCupet;
            guildBattleReadyView.OnSelectRightTurretCupet -= presenter.OnSelectRightTurretCupet;
            guildBattleBuffView.OnSelect -= presenter.ShowBuffMaterialSelect;

            presenter.OnUpdateZeny -= UpdateZeny;
            presenter.OnUpdateCatCoin -= UpdateCatCoin;
            presenter.OnUpdateGuildBattleInfo -= OnUpdateGuildBattleInfo;
            presenter.OnUpdateGuildBattleRequest -= OnUpdateGuildBattleInfo;
            presenter.OnUpdateCupetList -= UpdateTurret;
            presenter.OnUpdateGuildBattleBuff -= UpdateGuildBattleBuff;

            presenter.RemoveEvent();

            EventDelegate.Remove(btnReward.OnClick, OnClickedBtnReward);
            EventDelegate.Remove(btnRank.OnClick, OnClickedBtnRank);
            EventDelegate.Remove(btnCupet.OnClick, OnClickedBtnCupet);
        }

        protected override void OnShow(IUIData data = null)
        {
            SetTab(TAB_GUILD_BATTLE_READY);

            presenter.RequestGuildCupetInfo(); // 큐펫 정보 목록 요청
            presenter.RequestGuildBattleBuffInfo(); // 버프 정보
            UpdateRemainTimeGuildBattleRequest();
            RefreshState();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            titleView.ShowTitle(LocalizeKey._33800.ToText()); // 길드전
            tab[TAB_GUILD_BATTLE_READY].LocalKey = LocalizeKey._33801; // 길드전 신청
            tab[TAB_GUILD_BATTLE_BUFF].LocalKey = LocalizeKey._33802; // 버프 강화
            labelRemainTime.TitleKey = LocalizeKey._33803; // 길드전 신청 가능 시간
            btnReward.LocalKey = LocalizeKey._33705; // 보상
            btnRank.LocalKey = LocalizeKey._33706; // 랭킹
            btnCupet.LocalKey = LocalizeKey._33704; // 큐펫
        }

        public void SetTab(int index)
        {
            tab.Value = index;
        }

        private void OnSelectTab(int index)
        {
            guildBattleReadyView.SetActive(index == TAB_GUILD_BATTLE_READY);
            guildBattleBuffView.SetActive(index == TAB_GUILD_BATTLE_BUFF);
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
        /// 길드전 신청 탭 업데이트
        /// </summary>
        private void OnUpdateGuildBattleInfo()
        {
            UpdateRemainTimeGuildBattleRequest();
            UpdateTurret();
            guildBattleReadyView.SetBtnRequest(presenter.GetBtnRequestState());
            RefreshState();
        }

        /// <summary>
        /// 길드전 신청 상태
        /// </summary>
        private void RefreshState()
        {
            guildBattleReadyView.SetRequestState(presenter.IsReqeustState());
        }

        /// <summary>
        /// 길드전 신청 남은 시간
        /// </summary>
        private void UpdateRemainTimeGuildBattleRequest()
        {
            Timing.RunCoroutineSingleton(YieldGuildBattleRequestRemainTime(presenter.GetRemainTimeGuildBattleRequest()).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        IEnumerator<float> YieldGuildBattleRequestRemainTime(RemainTime remainTime)
        {
            // 무료 입장이 없을 경우
            while (remainTime.ToRemainTime() > 0)
            {
                labelRemainTime.Value = remainTime.ToRemainTime().ToStringTimeConatinsDay();
                yield return Timing.WaitForSeconds(0.1f);
            }

            labelRemainTime.Value = "00:00:00";

            UI.ConfirmPopup(LocalizeKey._33818.ToText(), CloseUI); // 길드전이 시작되었습니다.
        }

        /// <summary>
        /// 포탑 업데이트
        /// </summary>
        private void UpdateTurret()
        {
            guildBattleReadyView.SetLeftTurret(presenter.GetLeftCupetArray());
            guildBattleReadyView.SetRightTurret(presenter.GetRightCupetArray());
        }

        private void UpdateGuildBattleBuff()
        {
            GuildBattleBuffSelectElement.IInput[] buffs = presenter.GetBuffArray();
            guildBattleBuffView.SetData(buffs);
        }

        private void OnClickedBtnReward()
        {
            UI.Show<UIGuildBattleReward>();
        }

        private void OnClickedBtnRank()
        {
            UI.Show<UIGuildRank>().Set(UIGuildRank.GuildRankType.GuildBattle);
        }

        private void OnClickedBtnCupet()
        {
            UI.Show<UICupet>();
        }

        private void CloseUI()
        {
            UI.Close<UIGuildBattleReady>();
        }
    }
}