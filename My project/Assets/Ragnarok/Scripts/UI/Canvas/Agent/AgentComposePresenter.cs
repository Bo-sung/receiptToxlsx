using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ragnarok
{
    public class AgentComposePresenter : ViewPresenter
    {
        private class Proxy
        {
            public IAgent agent;
            public int dupCount;
        }

        private UIAgentCompose view;

        private AgentType viewAgentType;
        private List<Proxy> selectedAgentProxyList = new List<Proxy>();
        private List<Proxy> showingAgentProxyList = new List<Proxy>();
        private int curComposingRank;
        private List<int> composePriceList = new List<int>();

        public AgentComposePresenter(UIAgentCompose view)
        {
            this.view = view;
            var keys = BasisType.AGENT_COMPOSE_ZENY.GetKeyList();
            for (int i = 0; i < keys.Count; ++i)
                composePriceList.Add(BasisType.AGENT_COMPOSE_ZENY.GetInt(keys[i]));
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void ViewEventHandler(UIAgentCompose.EventType eventType, object data)
        {
            if (eventType == UIAgentCompose.EventType.OnClickAgent)
            {
                var agent = (IAgent)data;

                if (curComposingRank == 0)
                    SetComposeRankAndUpdateList(agent.AgentData.agent_rating);

                var proxy = showingAgentProxyList.Find(v => v.agent.ID == agent.ID);

                if (proxy.dupCount == 0)
                    return;

                if (selectedAgentProxyList.Count % 3 == 0)
                {
                    view.SetMaterialSlotAgent(0, agent, true);
                    view.SetMaterialSlotAgent(1, null, false);
                    view.SetMaterialSlotAgent(2, null, false);
                }
                else
                {
                    int nextIndex = selectedAgentProxyList.Count % 3;
                    view.SetMaterialSlotAgent(nextIndex, agent, true);
                }

                selectedAgentProxyList.Add(proxy);
                --proxy.dupCount;

                view.UpdateAgentCount(proxy.agent.ID, proxy.dupCount);
                view.SetComposeCount(selectedAgentProxyList.Count / 3);
                view.SetComposeButtonView(CalculateComposePrice(), CanCompose());
                view.UpdateNoticeText(selectedAgentProxyList.Count);
            }
            else if (eventType == UIAgentCompose.EventType.OnClickBulkSelectRank)
            {
                int rank = (int)data;

                if (curComposingRank != rank)
                {
                    var agents = GetAgents(rank);
                    int dupCount = 0;
                    for (int i = 0; i < agents.Length; ++i)
                        dupCount += agents[i].DuplicationCount;

                    if (dupCount < 3)
                    {
                        UI.ShowToastPopup(LocalizeKey._47314.ToText()); // 동료 수가 부족합니다.
                        return;
                    }

                    SetComposeRankAndUpdateList(rank);
                }
                else
                {
                    for (int i = 0; i < selectedAgentProxyList.Count; ++i)
                        ++selectedAgentProxyList[i].dupCount;
                    selectedAgentProxyList.Clear();
                }

                bool canBulkSelect = false;
                int totalSelectable = 0;
                for (int i = 0; i < showingAgentProxyList.Count; ++i)
                {
                    totalSelectable += showingAgentProxyList[i].dupCount;
                    if (totalSelectable >= 3)
                        canBulkSelect = true;
                }

                if (canBulkSelect)
                {
                    int countToSelect = (totalSelectable - 3) / 3;
                    countToSelect = countToSelect * 3 + 3;

                    for (int i = 0; i < showingAgentProxyList.Count; ++i)
                    {
                        var each = showingAgentProxyList[i];
                        while (each.agent.AgentData.agent_rating == curComposingRank && each.dupCount > 0 && countToSelect > 0)
                        {
                            selectedAgentProxyList.Add(each);
                            --each.dupCount;
                            --countToSelect;
                        }

                        if (countToSelect == 0)
                            break;
                    }

                    for (int i = 0; i < showingAgentProxyList.Count; ++i)
                        view.UpdateAgentCount(showingAgentProxyList[i].agent.ID, showingAgentProxyList[i].dupCount);
                    view.SetMaterialSlotAgent(0, selectedAgentProxyList[selectedAgentProxyList.Count - 3].agent, true);
                    view.SetMaterialSlotAgent(1, selectedAgentProxyList[selectedAgentProxyList.Count - 2].agent, true);
                    view.SetMaterialSlotAgent(2, selectedAgentProxyList[selectedAgentProxyList.Count - 1].agent, true);
                    view.SetComposeCount(selectedAgentProxyList.Count / 3);
                    view.SetComposeButtonView(CalculateComposePrice(), CanCompose());
                    view.UpdateNoticeText(selectedAgentProxyList.Count);
                }
                else
                {
                    UI.ShowToastPopup(LocalizeKey._47314.ToText()); // 동료 수가 부족합니다.
                }
            }
            else if (eventType == UIAgentCompose.EventType.OnClickCompose)
            {
                if (!CanCompose())
                    return;

                RequestCompose().WrapNetworkErrors();
            }
            else if (eventType == UIAgentCompose.EventType.OnClickMaterialSlot)
            {
                if (selectedAgentProxyList.Count == 0)
                    return;

                int index = (int)data;
                int curSelectedSlotCount = selectedAgentProxyList.Count % 3;
                if (curSelectedSlotCount == 0)
                    curSelectedSlotCount = 3;

                if (index < curSelectedSlotCount)
                {
                    int startIndex = selectedAgentProxyList.Count - curSelectedSlotCount;

                    var proxy = selectedAgentProxyList[startIndex + index];
                    ++proxy.dupCount;
                    selectedAgentProxyList.RemoveAt(startIndex + index);

                    if (curSelectedSlotCount == 1 && selectedAgentProxyList.Count >= 3) // 전부 취소하면 이전에 선택한 동료들을 보여준다.
                        startIndex = selectedAgentProxyList.Count - 3;

                    for (int i = 0; i < 3; ++i)
                    {
                        IAgent agentToShow = null;
                        if (startIndex + i < selectedAgentProxyList.Count)
                            agentToShow = selectedAgentProxyList[startIndex + i].agent;
                        view.SetMaterialSlotAgent(i, agentToShow, false);
                    }

                    view.UpdateAgentCount(proxy.agent.ID, proxy.dupCount);
                    view.SetComposeCount(selectedAgentProxyList.Count / 3);
                    view.SetComposeButtonView(CalculateComposePrice(), CanCompose());
                    view.UpdateNoticeText(selectedAgentProxyList.Count);

                    if (selectedAgentProxyList.Count == 0)
                        SetComposeRankAndUpdateList(0);
                }
                else
                {
                    return;
                }
            }
        }

        public void OnShow(AgentType viewAgentType)
        {
            this.viewAgentType = viewAgentType;
            Init();
        }

        private void Init()
        {
            selectedAgentProxyList.Clear();
            SetComposeRankAndUpdateList(0);
            view.SetMaterialSlotAgent(0, null, false);
            view.SetMaterialSlotAgent(1, null, false);
            view.SetMaterialSlotAgent(2, null, false);
            view.SetComposeCount(0);
            view.SetComposeButtonView(0, false);
            view.UpdateNoticeText(0);
        }

        private void SetComposeRankAndUpdateList(int rank)
        {
            curComposingRank = rank;
            showingAgentProxyList.Clear();

            var agents = GetAgents(curComposingRank);
            for (int i = 0; i < agents.Length; ++i)
                showingAgentProxyList.Add(new Proxy()
                {
                    agent = agents[i],
                    dupCount = agents[i].DuplicationCount
                });

            view.ShowAgents(agents);
        }

        List<IAgent> result = new List<IAgent>();
        private IAgent[] GetAgents(int rank)
        {
            result.Clear();

            foreach (var each in Entity.player.Agent.GetAllAgents())
                if (each.DuplicationCount > 0 && each.AgentType == viewAgentType && (rank == 0 || each.AgentData.agent_rating == rank))
                    result.Add(each);

            return result.ToArray();
        }

        private bool CanCompose()
        {
            if (selectedAgentProxyList.Count == 0 || selectedAgentProxyList.Count % 3 != 0)
                return false;

            int price = CalculateComposePrice();

            if (Entity.player.Goods.Zeny < price)
                return false;

            return true;
        }

        private int CalculateComposePrice()
        {
            return composePriceList[curComposingRank - 1] * (selectedAgentProxyList.Count / 3);
        }

        private async Task RequestCompose()
        {
            int prevRequestData = -1;
            List<AgentModel.ComposeRequestData> requestData = new List<AgentModel.ComposeRequestData>();

            selectedAgentProxyList.Sort((a, b) =>
            {
                return a.agent.ID - b.agent.ID;
            });

            for (int i = 0; i < selectedAgentProxyList.Count; ++i)
            {
                var each = selectedAgentProxyList[i];

                if (i % 3 == 0)
                    prevRequestData = -1; // 3 개 단위로 잘라서 서버에 보내야합니다.

                if (prevRequestData != -1 && requestData[prevRequestData].agentID == each.agent.ID)
                {
                    var temp = requestData[prevRequestData];
                    ++temp.count;
                    requestData[prevRequestData] = temp;
                }
                else
                {
                    prevRequestData = requestData.Count;
                    requestData.Add(new AgentModel.ComposeRequestData()
                    {
                        agentID = each.agent.ID,
                        count = 1
                    });
                }
            }

            await Entity.player.Agent.RequestAgentCompose(requestData.ToArray(), viewAgentType == AgentType.CombatAgent);

            SoundManager.Instance.PlayUISfx(Sfx.UI.ChangeCard);


            Init();
        }
    }
}