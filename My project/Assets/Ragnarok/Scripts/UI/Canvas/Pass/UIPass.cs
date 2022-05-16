using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIPass : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        private const int TAB_PASS_REWARD = 0; // 패스 보상 탭
        private const int TAB_PASS_QUEST = 1; // 패스 퀘스트 탭

        [SerializeField] SimplePopupView simplePopupView;
        [SerializeField] UITabHelper tab;
        [SerializeField] PassView passView;
        [SerializeField] PassRewardView passRewardView;
        [SerializeField] PassQuestView passQuestView;
        [SerializeField] UILabelHelper labelNotice;

        PassPresenter presenter;

        protected override void OnInit()
        {
            presenter = new PassPresenter(PassType.Labyrinth);

            tab.OnSelect += OnSelectTab;

            simplePopupView.OnExit += OnBack;
            passView.OnSelectBuyPass += presenter.BuyPass;
            passRewardView.OnSelectBuyExp += presenter.RequestBuyPassExp;
            passRewardView.OnSelectReceive += presenter.RequestPassReward;
            passQuestView.OnSelectDailyQuest += presenter.RequestPassDailyQuestReward;
            passQuestView.OnSelectSeasonQuest += presenter.RequestPassSeasonQuestReward;

            presenter.OnUpdatePass += UpdatePass;
            presenter.OnUpdateBuyPassExp += UpdateBuyPassExp;
            presenter.OnStandByReward += UpdateQuests;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            tab.OnSelect -= OnSelectTab;

            simplePopupView.OnExit -= OnBack;
            passView.OnSelectBuyPass -= presenter.BuyPass;
            passRewardView.OnSelectBuyExp -= presenter.RequestBuyPassExp;
            passRewardView.OnSelectReceive -= presenter.RequestPassReward;
            passQuestView.OnSelectDailyQuest -= presenter.RequestPassDailyQuestReward;
            passQuestView.OnSelectSeasonQuest -= presenter.RequestPassSeasonQuestReward;

            presenter.OnUpdatePass -= UpdatePass;
            presenter.OnUpdateBuyPassExp -= UpdateBuyPassExp;
            presenter.OnStandByReward -= UpdateQuests;

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            tab.Value = TAB_PASS_REWARD;
            UpdateSeason();
            UpdatePassButtons();
            UpdateSeasonRemainTime();
            UpdateLastRewardIcon();
            UpdatePassExp();
            UpdatePassRewards();
            UpdateQuests();
            MoveTo();
            UpdateRewardTabNotice();
            SetTexture();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            simplePopupView.MainTitleLocalKey = LocalizeKey._39800; // 라비린스 패스
            tab[TAB_PASS_REWARD].LocalKey = LocalizeKey._39805; // 패스 보상
            tab[TAB_PASS_QUEST].LocalKey = LocalizeKey._39806; // 패스 퀘스트
            labelNotice.Text = LocalizeKey._39814.ToText()
                .Replace(ReplaceKey.COUNT, presenter.GetLastPassExp()); // 라비린스 패스를 완료하면, 패스 경험치 {COUNT}마다 패스 완료 선물을 획득할 수 있습니다!
        }

        private void OnSelectTab(int index)
        {
            passRewardView.SetActive(index == TAB_PASS_REWARD);
            passQuestView.SetActive(index == TAB_PASS_QUEST);
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
            passView.SetIsActivePass(presenter.IsActivePass());
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
            passQuestView.SetDailyQuestData(presenter.GetPassDailyQuests());
            passQuestView.SetSeasonQuestsData(presenter.GetPassSeasonQuests());
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

        private void SetTexture()
        {
            passView.SetTexture(presenter.GetTextureName());
        }
    }
}