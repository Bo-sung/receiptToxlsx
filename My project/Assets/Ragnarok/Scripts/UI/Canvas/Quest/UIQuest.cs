using UnityEngine;

namespace Ragnarok
{
    public sealed class UIQuest : UICanvas<QuestPresenter>, QuestPresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        public enum ViewType
        {
            Main,
            Daily,
            Achieve,
            Guild,
        }

        public static ViewType view = ViewType.Main;

        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] NPCStyle npc;
        [SerializeField] UILabelHelper notiLabel;
        [SerializeField] UITabHelper tab;
        [SerializeField] MainQuestView mainQuestView;
        [SerializeField] DailyQuestView dailyQuestView;
        [SerializeField] AchievementView achievementView;
        [SerializeField] GuildQuestView guildQuestView;

        /// <summary>
        /// optional : 바로가기
        /// </summary>
        public class Input : IUIData
        {
            public ViewType viewType;

            public Input(ViewType viewType)
            {
                this.viewType = viewType;
            }
        }

        UISubCanvas currentSubCanvas;

        protected override void OnInit()
        {
            presenter = new QuestPresenter(this);

            mainQuestView.Initialize(presenter);
            dailyQuestView.Initialize(presenter);
            achievementView.Initialize(presenter);
            guildQuestView.Initialize(presenter);

            presenter.AddEvent();
            EventDelegate.Add(btnClose.OnClick, CloseUI);
            EventDelegate.Add(tab.OnChange[0], ShowMainQuestView);
            EventDelegate.Add(tab.OnChange[1], ShowDailyQuestView);
            EventDelegate.Add(tab.OnChange[2], ShowAchievementView);
            EventDelegate.Add(tab.OnChange[3], ShowGuildQuestView);

            npc.SetStyle();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            EventDelegate.Remove(btnClose.OnClick, CloseUI);
            EventDelegate.Remove(tab.OnChange[0], ShowMainQuestView);
            EventDelegate.Remove(tab.OnChange[1], ShowDailyQuestView);
            EventDelegate.Remove(tab.OnChange[2], ShowAchievementView);
            EventDelegate.Remove(tab.OnChange[3], ShowGuildQuestView);
        }

        protected override void OnShow(IUIData data = null)
        {
            if (data is Input input)
            {
                view = input.viewType;
            }

            tab[view.ToIntValue()].Value = true;
        }

        protected override void OnHide()
        {
            tab[view.ToIntValue()].Set(false);
            if (currentSubCanvas == null)
                return;
            currentSubCanvas.Hide();
        }

        protected override void OnLocalize()
        {
            notiLabel.LocalKey = LocalizeKey._10000; // Job Exp는 메인 퀘스트에서 획득 가능합니다.
            tab[0].LocalKey = LocalizeKey._10001; // 메인
            tab[1].LocalKey = LocalizeKey._10003; // 일일
            tab[2].LocalKey = LocalizeKey._10004; // 업적
            tab[3].LocalKey = LocalizeKey._10025; // 길드
        }

        public void UpdateView()
        {
            if (currentSubCanvas == null)
                return;

            currentSubCanvas.Show();
        }

        private void ShowMainQuestView()
        {
            if (!UIToggle.current.value)
                return;
            view = ViewType.Main;
            ShowSubCanvas(mainQuestView);
        }

        private void ShowDailyQuestView()
        {
            if (!UIToggle.current.value)
                return;
            view = ViewType.Daily;
            ShowSubCanvas(dailyQuestView);
        }

        private void ShowAchievementView()
        {
            if (!UIToggle.current.value)
                return;
            view = ViewType.Achieve;
            ShowSubCanvas(achievementView);
        }

        private void ShowGuildQuestView()
        {
            if (!UIToggle.current.value)
                return;
            view = ViewType.Guild;
            ShowSubCanvas(guildQuestView);
        }

        public void ShowNewView()
        {
            var stardByReward = presenter.GetStandByRewards();

            tab[0].SetNotice(!presenter.GetMainQuest().IsInvalidData && presenter.GetMainQuest().CompleteType == QuestInfo.QuestCompleteType.StandByReward);
            tab[1].SetNotice(stardByReward[QuestCategory.DailyStart] > 0);
            tab[2].SetNotice(stardByReward[QuestCategory.Achieve] > 0);
            tab[3].SetNotice(presenter.HasGuildQuestReward());
        }

        private void ShowSubCanvas(UISubCanvas subCanvas)
        {
            currentSubCanvas = subCanvas;

            HideAllSubCanvas();
            UpdateView();
            ShowNewView();
            npc.PlayTalk();
        }

        public void CloseUI()
        {
            UI.Close<UIQuest>();
        }
    }
}