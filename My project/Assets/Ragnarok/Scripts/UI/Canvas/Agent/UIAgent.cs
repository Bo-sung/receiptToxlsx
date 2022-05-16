using UnityEngine;

namespace Ragnarok
{
    public class UIAgent : UICanvas
    {
        public class Input : IUIData
        {
            public AgentType viewAgentType;
        }

        public enum ViewType : int { None = -1, Explore = 0, List = 1, Compose = 2, Book = 3 }

        public static ViewType view = ViewType.None;

        protected override UIType uiType => UIType.Back | UIType.Destroy | UIType.Single | UIType.Reactivation;

        [SerializeField] UIWidget[] tabWidgets;
        [SerializeField] UIGrid tabGrid;
        [SerializeField] UITabHelper mainTab;
        [SerializeField] UIToggleHelper[] mainTabButtons;
        [SerializeField] UIAgentExplore exploreView;    // 파견
        [SerializeField] UIAgentList listView;          // 목록
        [SerializeField] UIAgentCompose composeView;    // 합성
        [SerializeField] UIAgentBook bookView;          // 인연
        [SerializeField] UIButton closeButton;

        private AgentExplorePresenter explorePresenter;
        private AgentListPresenter listPresenter;
        private AgentComposePresenter composePresenter;
        private AgentBookPresenter bookPresenter;
        private QuestModel questModel;
        private AgentType viewAgentType;

        private bool onceListShowUp = false;

        protected override void OnInit()
        {
            questModel = Entity.player.Quest;
            explorePresenter = new AgentExplorePresenter(this, exploreView);
            listPresenter = new AgentListPresenter(this, listView);
            composePresenter = new AgentComposePresenter(composeView);
            bookPresenter = new AgentBookPresenter(this, bookView);

            exploreView.OnInit(explorePresenter);
            listView.OnInit(listPresenter);
            composeView.OnInit(composePresenter);
            bookView.OnInit(bookPresenter);

            explorePresenter.AddEvent();
            listPresenter.AddEvent();
            composePresenter.AddEvent();
            bookPresenter.AddEvent();

            mainTabButtons[0].Value = false;
            mainTabButtons[1].Value = false;
            mainTabButtons[2].Value = false;
            mainTabButtons[3].Value = false;

            EventDelegate.Add(mainTab.OnChange[0], OpenAgentExplore);
            EventDelegate.Add(mainTab.OnChange[1], OpenAgentList);
            EventDelegate.Add(mainTab.OnChange[2], OpenAgentCompose);
            EventDelegate.Add(mainTab.OnChange[3], OpenAgentBook);
            EventDelegate.Add(closeButton.onClick, OnClickClose);
        }

        protected override void OnShow(IUIData data = null)
        {
            // mainTab 의 초기화 이벤트로 OpenAgentList 가 발생하기에 아무것도 하지 않습니다.
            // OpenAgentList();

            Input input = data as Input;

            viewAgentType = input.viewAgentType;
            StartTutorial();

            if (viewAgentType == AgentType.CombatAgent)
            {
                questModel.RemoveNewOpenContent(ContentType.CombatAgent); // 신규 컨텐츠 플래그 제거 (전투 동료)

                tabGrid.cellWidth = 193;
                mainTabButtons[0].gameObject.SetActive(false);
                for (int i = 0; i < tabWidgets.Length; ++i)
                    tabWidgets[i].SetDimensions(189, 70);
                tabGrid.Reposition();
            }
            else
            {
                questModel.RemoveNewOpenContent(ContentType.Explore); // 신규 컨텐츠 플래그 제거 (파견)

                tabGrid.cellWidth = 142;
                mainTabButtons[0].gameObject.SetActive(true);
                for (int i = 0; i < tabWidgets.Length; ++i)
                    tabWidgets[i].SetDimensions(138, 70);
                tabGrid.Reposition();
            }

            onceListShowUp = false;
            UpdateNotice();

            mainTabButtons[0].Value = false;
            mainTabButtons[1].Value = false;
            mainTabButtons[2].Value = false;
            mainTabButtons[3].Value = false;

            if (view != ViewType.None)
            {
                mainTabButtons[(int)view].Value = true;
                view = ViewType.None;
            }
            else
            {
                if (viewAgentType == AgentType.CombatAgent)
                    mainTabButtons[(int)ViewType.List].Value = true;
                else
                    mainTabButtons[(int)ViewType.Explore].Value = true;
            }
        }

        protected override void OnClose()
        {
            explorePresenter.RemoveEvent();
            listPresenter.RemoveEvent();
            composePresenter.RemoveEvent();
            bookPresenter.RemoveEvent();

            exploreView.OnClose();
            listView.OnClose();
            composeView.OnClose();
            bookView.OnClose();

            EventDelegate.Remove(mainTab.OnChange[0], OpenAgentExplore);
            EventDelegate.Remove(mainTab.OnChange[1], OpenAgentList);
            EventDelegate.Remove(mainTab.OnChange[2], OpenAgentCompose);
            EventDelegate.Remove(mainTab.OnChange[3], OpenAgentBook);
            EventDelegate.Remove(closeButton.onClick, OnClickClose);
        }

        protected override void OnLocalize()
        {
            mainTab[0].LocalKey = LocalizeKey._47341; // 파견
            mainTab[1].LocalKey = LocalizeKey._47342; // 목록
            mainTab[2].LocalKey = LocalizeKey._47304; // 합성
            mainTab[3].LocalKey = LocalizeKey._47305; // 인연

            exploreView.OnLocalize();
            listView.OnLocalize();
            composeView.OnLocalize();
            bookView.OnLocalize();
        }

        protected override void OnHide()
        {
        }

        private void OnChangeTab()
        {
            if (onceListShowUp && !listView.gameObject.activeSelf)
                listPresenter.HideIsNewOnAgent();
        }

        private void OpenAgentExplore()
        {
            if (!UIToggle.current.value)
                return;

            exploreView.gameObject.SetActive(true);
            listView.gameObject.SetActive(false);
            composeView.gameObject.SetActive(false);
            bookView.gameObject.SetActive(false);

            explorePresenter.OnShow();
            OnChangeTab();
        }

        private void OpenAgentList()
        {
            if (!UIToggle.current.value)
                return;

            exploreView.gameObject.SetActive(false);
            listView.gameObject.SetActive(true);
            composeView.gameObject.SetActive(false);
            bookView.gameObject.SetActive(false);

            onceListShowUp = true;
            listPresenter.OnShow(viewAgentType);
            OnChangeTab();
        }

        private void OpenAgentCompose()
        {
            if (!UIToggle.current.value)
                return;

            exploreView.gameObject.SetActive(false);
            listView.gameObject.SetActive(false);
            composeView.gameObject.SetActive(true);
            bookView.gameObject.SetActive(false);

            composePresenter.OnShow(viewAgentType);
            OnChangeTab();
        }

        private void OpenAgentBook()
        {
            if (!UIToggle.current.value)
                return;

            exploreView.gameObject.SetActive(false);
            listView.gameObject.SetActive(false);
            composeView.gameObject.SetActive(false);
            bookView.gameObject.SetActive(true);

            bookPresenter.OnShow(viewAgentType);
            OnChangeTab();
        }

        private void OnClickClose()
        {
            UI.Close<UIAgent>();
        }

        public void UpdateNotice()
        {
            //mainTabButtons[1].SetNotice(agentModel.HasNewAgent());
            //mainTabButtons[3].SetNotice(agentModel.CanCompleteBook());
        }

        private void StartTutorial()
        {
            Tutorial.Run(TutorialType.Agent);
        }

        public void UpdateSubNotice(AgentType agentType = AgentType.None)
        {
            //mainTabButtons[1].SetNotice(Entity.player.Agent.HasNewAgent(agentType));
        }
    }
}