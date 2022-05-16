using System.Collections.Generic;

namespace Ragnarok
{
    public class TradeExplorePresenter : ViewPresenter
    {
        private readonly AgentModel agentModel;
        private readonly ShopModel shopModel;
        private readonly IronSourceManager ironSourceManager;

        private UIExploreTradeView view;
        private ExploreAgent curSelectedAgent;
        private StageData stageData;
        private bool isCompleteAd;

        public TradeExplorePresenter(UIExploreTradeView view)
        {
            this.view = view;
            agentModel = Entity.player.Agent;
            shopModel = Entity.player.ShopModel;
            ironSourceManager = IronSourceManager.Instance;
        }

        public void OnShow(StageData stageData)
        {
            curSelectedAgent = null;
            this.stageData = stageData;
            view.SetStageDependantInfo(stageData);
            view.SetRemainTradeCount(agentModel.GetTradeProductionRemainCount(stageData.id), agentModel.GetTradeProductionMaxCount());

            List<ExploreAgent> buffer = new List<ExploreAgent>();

            foreach (var each in agentModel.GetExploreAgents())
            {
                if (each.ExploreTypeBit.HasFlag(stageData.agent_explore_type.ToEnum<ExploreType>()))
                    buffer.Add(each);
            }

            buffer.Sort((a, b) =>
            {
                if (a.AgentData.agent_rating != b.AgentData.agent_rating)
                    return b.AgentData.agent_rating - a.AgentData.agent_rating;
                else
                    return a.AgentData.id - b.AgentData.id;
            });

            view.SetAgents(buffer);
        }

        public override void AddEvent()
        {
            agentModel.OnResetTradeCount += OnResetTradeCount;
            agentModel.OnExploreStart += OnExploreStart;
        }

        public override void RemoveEvent()
        {
            agentModel.OnResetTradeCount -= OnResetTradeCount;
            agentModel.OnExploreStart -= OnExploreStart;
        }

        public void ViewEventHandler(UIExploreTradeView.EventType eventType, ExploreAgent agent)
        {
            if (eventType == UIExploreTradeView.EventType.OnClickAgent)
            {
                if (agent.IsExploring)
                    return;

                if (curSelectedAgent == agent)
                    return;

                view.SelectAgent(agent);
                curSelectedAgent = agent;
            }
            else if (eventType == UIExploreTradeView.EventType.OnClickSend)
            {
                if (curSelectedAgent == null)
                {
                    UI.ShowToastPopup(LocalizeKey._47409.ToText()); // 파견을 보낼 동료를 선택해주세요.
                    return;
                }

                if (agentModel.GetTradeProductionRemainCount(stageData.id) == 0)
                {
                    if (agentModel.IsTradeProuctionAd(stageData.id))
                    {
                        ShowRewardedVideo();
                    }
                    else
                    {
                        OpenCostPopup();
                    }
                    return;
                }

                RequestAgentExploreStart();
            }
        }

        /// <summary>
        /// 광고 보고 파견 횟수 초기화
        /// </summary>
        private void ShowRewardedVideo()
        {
            bool isFree = shopModel.IsSkipTradeProductionAd();
            // 광고를 시청하시면 무료 1회 재파견이 가능합니다.\n광고를 시청하시겠습니까?\n(횟수는 매일 자정(GMT+8)에 초기화 됩니다.)
            ironSourceManager.ShowRewardedVideo(IronSourceManager.PlacementNameType.None, isFree, isBeginner: false, OnCompleteRewardVideo, descriptionId: 90276);
        }

        private void OnCompleteRewardVideo()
        {
            if (agentModel.GetTradeProductionRemainCount(stageData.id) != 0)
            {
                // 광고 보는 도중에 하루 초기화가 된 상태
                UI.ShowToastPopup(LocalizeKey._90281.ToText()); // 자정(GMT+8)이 지나, 파견 횟수가 초기화되었습니다.
                return;
            }

            isCompleteAd = true;
            agentModel.RequestAgentResetTradeCount(stageData.id).WrapNetworkErrors();
        }

        /// <summary>
        /// 냥다래 소모하여 파견 횟수 초기화
        /// </summary>
        private async void OpenCostPopup()
        {
            var result = await UI.CostPopup(CoinType.CatCoin, BasisType.AGENT_TRADE_RESET_PRICE.GetInt(),
                LocalizeKey._47330.ToText(), // 초기화
                LocalizeKey._90278.ToText()); // [9E9B9E][c]해당 구역의 일일 파견 가능 횟수를[/c][-] [69B2E6][c]초기화[/c][-] [9E9B9E][c]하시겠습니까?[/c][-]\n[A9A9A9][c](횟수는 매일 자정(GMT+8)에 초기화 됩니다.)[/c][-]

            if (result)
            {
                agentModel.RequestAgentResetTradeCount(stageData.id).WrapNetworkErrors();
            }
        }

        /// <summary>
        /// 파견 요청 (교역 | 생산)
        /// </summary>
        private void RequestAgentExploreStart()
        {            
            agentModel.RequestAgentExploreStart(new ExploreAgent[] { curSelectedAgent }, stageData.id, stageData.agent_explore_type.ToEnum<ExploreType>()).WrapNetworkErrors();
        }

        /// <summary>
        /// 교역 횟수 초기화 성공
        /// </summary>
        void OnResetTradeCount()
        {
            if (isCompleteAd && shopModel.IsSkipTradeProductionAd())
            {
                UI.ShowToastPopup(LocalizeKey._90277.ToText()); // 결제 마일리지가 충족되어 광고 시청 없이 초기화 되었습니다.
            }
            else
            {
                UI.ShowToastPopup(LocalizeKey._90279.ToText()); // 파견 횟수가 초기화되었습니다.
            }
            isCompleteAd = false;
            view.SetRemainTradeCount(agentModel.GetTradeProductionRemainCount(stageData.id), agentModel.GetTradeProductionMaxCount());
        }

        /// <summary>
        /// 파견 성공 (교역)
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