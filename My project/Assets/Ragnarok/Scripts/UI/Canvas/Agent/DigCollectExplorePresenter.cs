using System.Collections.Generic;

namespace Ragnarok
{
    public class DigCollectExplorePresenter : ViewPresenter
    {
        /// <summary>
        /// 파견 동료 최대수
        /// </summary>
        private const int AGENT_MAX = 4;

        private readonly AgentModel agentModel;

        private UIExploreDigCollectView view;
        private List<ExploreAgent> selectedAgents;
        private StageData stageData;

        public DigCollectExplorePresenter(UIExploreDigCollectView view)
        {
            this.view = view;
            selectedAgents = new List<ExploreAgent>();
            agentModel = Entity.player.Agent;
        }

        public void OnShow(StageData stageData)
        {
            selectedAgents.Clear();
            this.stageData = stageData;
            List<ExploreAgent> agents = new List<ExploreAgent>();

            foreach (var each in agentModel.GetExploreAgents())
                if (each.ExploreTypeBit.HasFlag(stageData.agent_explore_type.ToEnum<ExploreType>()))
                    agents.Add(each);

            agents.Sort((a, b) => b.AgentData.agent_rating - a.AgentData.agent_rating); // 랭크 내림차순으로 정렬

            view.ShowAgents(agents.ToArray());
            view.SetStageDependantInfo(stageData);
            view.SetRewardCount(0);
            view.UpdatedSelectedAgents(selectedAgents);
        }

        public override void AddEvent()
        {
            agentModel.OnExploreStart += OnExploreStart;
        }

        public override void RemoveEvent()
        {
            agentModel.OnExploreStart -= OnExploreStart;
        }

        public void ViewEventHandler(UIExploreDigCollectView.EventType eventType, object data)
        {
            if (eventType == UIExploreDigCollectView.EventType.OnClickSlot)
            {
                ExploreAgent agent = data as ExploreAgent;

                if (agent.IsExploring)
                    return;

                var selected = selectedAgents.Find(v => v.ID == agent.ID);

                if (selected != null)
                {
                    selectedAgents.Remove(agent);
                    view.UpdateSelection(agent, false);
                }
                else
                {
                    if (selectedAgents.Count == AGENT_MAX)
                    {
                        UI.ShowToastPopup(LocalizeKey._47408.ToText()); // 최대 4명까지 선택 가능합니다.
                        return;
                    }

                    selectedAgents.Add(agent);
                    view.UpdateSelection(agent, true);
                }

                view.UpdatedSelectedAgents(selectedAgents);

                int totalRewardCount = 0;
                for (int i = 0; i < selectedAgents.Count; ++i)
                    totalRewardCount += stageData.GetExploreRewardCountRating(selectedAgents[i].AgentData.agent_rating);

                view.SetRewardCount(totalRewardCount);
            }
            else if (eventType == UIExploreDigCollectView.EventType.OnClickSendExplore)
            {
                if (selectedAgents.Count == 0)
                {
                    UI.ShowToastPopup(LocalizeKey._47409.ToText()); // 파견을 보낼 동료를 선택해주세요.
                    return;
                }

                RequestAgentExploreStart();
            }
        }

        /// <summary>
        /// 파견 요청 (발굴 & 채집)
        /// </summary>
        private void RequestAgentExploreStart()
        {
            agentModel.RequestAgentExploreStart(selectedAgents.ToArray(), stageData.id, stageData.agent_explore_type.ToEnum<ExploreType>()).WrapNetworkErrors();
        }

        /// <summary>
        /// 파견 성공 (발굴 & 채집)
        /// </summary>
        void OnExploreStart(ExploreType exploreType)
        {
            if (stageData == null || stageData.agent_explore_type.ToEnum<ExploreType>() != exploreType)
                return;

            UI.Close<UIExplore>();
            UI.ShowToastPopup(LocalizeKey._47410.ToText());  // 파견을 보냈습니다.
        }
    }
}