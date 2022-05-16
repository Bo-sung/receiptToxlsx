using System.Threading.Tasks;

namespace Ragnarok
{
    public class AgentExplorePresenter : ViewPresenter
    {
        private UIAgent mainView;
        private UIAgentExplore view;
        private AgentModel agentModel;
        private DungeonModel dungeonModel;

        private AdventureData[] adventures;
        private int curShowingChapter = 0;
        private int lastOpenedChapter = 1;

        public AgentExplorePresenter(UIAgent mainView, UIAgentExplore view)
        {
            this.mainView = mainView;
            this.view = view;
            agentModel = Entity.player.Agent;
            dungeonModel = Entity.player.Dungeon;
            adventures = AdventureDataManager.Instance.GetArrData();
        }

        public void OnShow()
        {
            int clearStageID = dungeonModel.FinalStageId;

            for (int i = 0; i < adventures.Length; ++i)
            {
                var each = adventures[i];

                if (each.link_type == 2 && each.link_value <= clearStageID)
                    lastOpenedChapter = each.chapter;
            }

            if (curShowingChapter == 0)
            {
                curShowingChapter = lastOpenedChapter;
                view.SelectChapter(curShowingChapter, adventures, true);
            }
            else
            {
                view.SelectChapter(curShowingChapter, adventures);
            }

            view.UpdateChapterView(lastOpenedChapter);
        }

        public override void AddEvent()
        {
            agentModel.OnExploreStateChanged += OnExploreStateChanged;
            agentModel.OnUpdateRemainTradeCount += OnUpdateRemainTradeCount;
            agentModel.OnAgentExploreReward += OnAgentExploreReward;
        }

        public override void RemoveEvent()
        {
            agentModel.OnExploreStateChanged -= OnExploreStateChanged;
            agentModel.OnUpdateRemainTradeCount -= OnUpdateRemainTradeCount;
            agentModel.OnAgentExploreReward -= OnAgentExploreReward;
        }

        public void ViewEventHandler(UIAgentExplore.Event eventType, object data)
        {
            if (eventType == UIAgentExplore.Event.OnClickSlot)
            {
                StageData stageData = data as StageData;

                var exploreState = agentModel.GetExploreState(stageData.id);

                if (exploreState != null && exploreState.RemainTime <= 0)
                {
                    RequestGetRewardExplore(exploreState);
                }
                else
                {
                    UI.Show<UIExplore>(new UIExplore.Input() { stageData = stageData });
                }
            }
            else if (eventType == UIAgentExplore.Event.OnClickChapter)
            {
                int chapter = (int)data;

                if (curShowingChapter == chapter || lastOpenedChapter < chapter)
                    return;

                curShowingChapter = chapter;
                view.SelectChapter(curShowingChapter, adventures);
            }
        }

        private void RequestGetRewardExplore(AgentExploreState exploreState)
        {
            agentModel.RequestAgentExploreReward(exploreState, false).WrapNetworkErrors();
        }

        void OnAgentExploreReward(AgentExploreState exploreState)
        {
            view.SelectChapter(curShowingChapter, adventures);
        }

        private void OnExploreStateChanged(int stageID, AgentExploreState exploreState)
        {
            view.SelectChapter(curShowingChapter, adventures);
        }

        private void OnUpdateRemainTradeCount()
        {
            view.SelectChapter(curShowingChapter, adventures);
        }

        public bool CanSendExplore(StageData stageData)
        {
            var exploreType = stageData.agent_explore_type.ToEnum<ExploreType>();

            if (exploreType == ExploreType.Trade)
                if (agentModel.GetTradeProductionRemainCount(stageData.id) == 0)
                    return false;

            var exploreState = agentModel.GetExploreState(stageData.id);

            if (exploreState != null)
                return false;

            foreach (var each in agentModel.GetExploreAgents())
                if (each.ProgressingExplore == null && each.ExploreTypeBit.HasFlag(exploreType))
                    return true;

            return false;
        }

        public bool IsCompletedExplore(StageData stageData)
        {
            var exploreState = agentModel.GetExploreState(stageData.id);

            if (exploreState != null && exploreState.RemainTime <= 0)
            {
                return true;
            }

            return false;
        }
    }
}