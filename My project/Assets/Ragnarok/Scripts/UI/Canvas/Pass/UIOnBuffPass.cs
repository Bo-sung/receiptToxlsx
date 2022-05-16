using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIOnBuffPass : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        private const int TAB_PASS_WALLET = 0; // 패스 지갑 탭
        private const int TAB_PASS_REWARD = 1; // 패스 보상 탭
        private const int TAB_PASS_QUEST = 2; // 패스 퀘스트 탭

        [SerializeField] SimplePopupView simplePopupView;
        [SerializeField] UITabHelper tab;
        [SerializeField] OnBuffPassView passView;
        [SerializeField] PassWalletView passWalletView;
        [SerializeField] OnBuffPassNoticeView onBuffPassNoticeView;
        [SerializeField] OnBuffPassRewardView passRewardView;
        [SerializeField] OnBuffPassQuestView passDailyQuestView;
        [SerializeField] UILabelHelper labelNotice;

        PassPresenter presenter;

        private int selectedTabIndex;

        protected override void OnInit()
        {
            presenter = new PassPresenter(PassType.OnBuff);

            tab.OnSelect += OnSelectTab;

            simplePopupView.OnExit += OnBack;
            passView.OnSelectBuyPass += presenter.BuyPass;
            passWalletView.OnSelectGetPoint += OnSelectGetPoint;
            passWalletView.OnSelectRefreshPoint += presenter.RequestOnBuffMyPoint;
            onBuffPassNoticeView.OnConfirm += presenter.TryGetOnBuffPoint;
            onBuffPassNoticeView.OnSelectBuyPass += presenter.BuyPass;
            passRewardView.OnSelectBuyExp += presenter.RequestBuyPassExp;
            passRewardView.OnSelectReceive += presenter.RequestPassReward;
            passDailyQuestView.OnSelect += presenter.RequestPassDailyQuestReward;

            presenter.OnUpdatePass += UpdatePass;
            presenter.OnUpdateBuyPassExp += UpdateBuyPassExp;
            presenter.OnStandByReward += UpdateQuests;
            presenter.OnUpdateOnBuffPoint += UpdateOnBuffPoint;
            presenter.OnUpdateOnBuffMvpPoint += UpdateWalletTabNotice;
            presenter.OnUpdateReceivedOnBuffPoint += UpdateWalletTabNotice;

            presenter.AddEvent();

            Initialize();
        }

        protected override void OnClose()
        {
            tab.OnSelect -= OnSelectTab;

            simplePopupView.OnExit -= OnBack;
            passView.OnSelectBuyPass -= presenter.BuyPass;
            passWalletView.OnSelectGetPoint -= OnSelectGetPoint;
            passWalletView.OnSelectRefreshPoint -= presenter.RequestOnBuffMyPoint;
            onBuffPassNoticeView.OnConfirm -= presenter.TryGetOnBuffPoint;
            onBuffPassNoticeView.OnSelectBuyPass -= presenter.BuyPass;
            passRewardView.OnSelectBuyExp -= presenter.RequestBuyPassExp;
            passRewardView.OnSelectReceive -= presenter.RequestPassReward;
            passDailyQuestView.OnSelect -= presenter.RequestPassDailyQuestReward;

            presenter.OnUpdatePass -= UpdatePass;
            presenter.OnUpdateBuyPassExp -= UpdateBuyPassExp;
            presenter.OnStandByReward -= UpdateQuests;
            presenter.OnUpdateOnBuffPoint -= UpdateOnBuffPoint;
            presenter.OnUpdateOnBuffMvpPoint -= UpdateWalletTabNotice;
            presenter.OnUpdateReceivedOnBuffPoint -= UpdateWalletTabNotice;

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            tab.Value = TAB_PASS_WALLET;

            UpdateSeason();
            UpdatePassButtons();
            UpdateSeasonRemainTime();
            UpdateLastRewardIcon();
            UpdatePassExp();
            UpdatePassRewards();
            UpdateQuests();
            MoveTo();
            UpdateRewardTabNotice();
            UpdateWalletTabNotice();
            UpdateOnBuffPoint();

            onBuffPassNoticeView.Hide();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            simplePopupView.MainTitleLocalKey = LocalizeKey._39817; // OnBuff 패스
            tab[TAB_PASS_WALLET].LocalKey = LocalizeKey._39820; // OnBuff 지갑
            tab[TAB_PASS_REWARD].LocalKey = LocalizeKey._39821; // OnBuff 보상
            tab[TAB_PASS_QUEST].LocalKey = LocalizeKey._39822; // OnBuff 퀘스트

            onBuffPassNoticeView.MainTitleLocalKey = LocalizeKey._39826; // 포인트 받기
            onBuffPassNoticeView.CancelLocalKey = LocalizeKey._2; // 취소
            onBuffPassNoticeView.ConfirmLocalKey = LocalizeKey._39826; // 포인트 받기

            UpdateLabelNotice();
        }

        void OnSelectGetPoint()
        {
            if (presenter.IsActivePass())
            {
                presenter.TryGetOnBuffPoint();
                return;
            }

            // 팝업 창 띄우기
            onBuffPassNoticeView.Show();
        }

        private void Initialize()
        {
            passWalletView.SetRewards(presenter.GetOnBuffFreeReward(), presenter.GetOnBuffPremiumReward());
            passWalletView.SetActiveBtnRefresh(presenter.IsOnBuffAccountLink());
        }

        private void UpdateLabelNotice()
        {
            labelNotice.Text = GetNotice();
        }

        private string GetNotice()
        {
            if (selectedTabIndex == TAB_PASS_WALLET)
                return LocalizeKey._39828.ToText(); // 높은 스테이지의 MVP를 처치할 경우 더 많은 포인트를 얻을 수 있습니다.

            return LocalizeKey._39830.ToText(); // 획득하신 ONBUFF 포인트는 우편함을 통해 수령하실 수 있습니다.
        }

        private void OnSelectTab(int index)
        {
            selectedTabIndex = index;
            passWalletView.SetActive(index == TAB_PASS_WALLET);
            passRewardView.SetActive(index == TAB_PASS_REWARD);
            passDailyQuestView.SetActive(index == TAB_PASS_QUEST);
            UpdateLabelNotice();
        }

        private void UpdateWalletTabNotice()
        {
            int mvpPoint = presenter.GetOnBuffMvpPoint();
            bool canRequest = presenter.CanRequestOnBuffPoint();
            tab[TAB_PASS_WALLET].SetNotice(canRequest);
            passWalletView.SetProgress(mvpPoint, presenter.onBuffMvpMaxPoint, canRequest);
        }

        private void UpdateRewardTabNotice()
        {
            tab[TAB_PASS_REWARD].SetNotice(presenter.IsRewardTabNotice());
        }

        private void UpdateQuestTabNotice()
        {
            tab[TAB_PASS_QUEST].SetNotice(presenter.IsQuestTabNotice());
        }

        private void UpdateSeason()
        {
            passView.SetSeason(presenter.GetSeason());
        }

        private void UpdatePassButtons()
        {
            bool isActivePass = presenter.IsActivePass();
            passView.SetIsActivePass(isActivePass);
            passWalletView.SetActivePass(isActivePass);
        }

        private void UpdateSeasonRemainTime()
        {
            passView.SetRemainTime(presenter.GetSeasonRemainTime());
        }

        private void UpdateLastRewardIcon()
        {
            passView.SetLastRewardIconName(presenter.GetLastRewardIconName());
        }

        private void UpdatePassExp()
        {
            IPassLevel output = presenter.GetExpInfo();
            int nextLevel = Mathf.Min(output.Level + 1, presenter.GetLastPassLevel());
            int lastLevel = presenter.GetLastPassLevel();
            int curExp = output.Level == lastLevel ? output.CurExp + output.MaxExp : output.CurExp;
            passView.SetNextLevel(nextLevel);
            passView.SetExp(curExp, output.MaxExp);
            passView.SetIsLastReward(nextLevel == lastLevel);
        }

        private void UpdatePassRewards()
        {
            passRewardView.SetData(presenter.GetPassRewards());
        }

        private void MoveTo()
        {
            passRewardView.MoveTo(presenter.GetCurLevelIndex());
        }

        private void UpdateQuests()
        {
            passDailyQuestView.SetData(presenter.GetPassDailyQuests());
            UpdateQuestTabNotice();
        }

        private void UpdatePass()
        {
            UpdatePassExp();
            UpdatePassRewards();
            UpdateRewardTabNotice();
        }

        private void UpdateBuyPassExp()
        {
            UpdatePass();
            MoveTo();
        }

        private void UpdateOnBuffPoint()
        {
            passWalletView.SetTotalPoint(presenter.GetTotalOnBuffPoint());
        }
    }
}