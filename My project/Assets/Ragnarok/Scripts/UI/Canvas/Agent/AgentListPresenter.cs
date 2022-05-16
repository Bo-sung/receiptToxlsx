using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class AgentListPresenter : ViewPresenter
    {
        public enum EquipmentSlotState { Equipped, Unequipped, Invalid, Plus }
        public enum ExploreSlotState { ExploreFinished, Exploring, NoExploring, CantExplore }

        private UIAgent mainView;
        private UIAgentList view;
        private AgentModel agentModel;

        private AgentData[] agentDatas;
        private List<AgentSlotInfoPacket> slotStateList;

        private AgentType curListAgentType = AgentType.CombatAgent;
        private IAgent curSelectedAgent = null;

        public AgentListPresenter(UIAgent mainView, UIAgentList view)
        {
            this.mainView = mainView;
            this.view = view;
            agentModel = Entity.player.Agent;
            slotStateList = new List<AgentSlotInfoPacket>();

            var agentDataRepo = AgentDataManager.Instance;
            List<AgentData> buffer = new List<AgentData>();
            foreach (var each in agentDataRepo.AgentDataList)
                buffer.Add(each);
            agentDatas = buffer.ToArray();
        }

        public void OnShow(AgentType viewAgentType)
        {
            curListAgentType = viewAgentType;
            Refresh();
        }

        public void HideIsNewOnAgent()
        {
            if (curListAgentType == AgentType.CombatAgent)
            {
                foreach (var each in agentModel.GetCombatAgents())
                    each.IsNew = false;
            }
            else
            {
                foreach (var each in agentModel.GetExploreAgents())
                    each.IsNew = false;
            }
        }

        private void Refresh()
        {
            view.ShowAgents(curListAgentType, agentDatas, GetAgentSlotViewInfos(curListAgentType));
            mainView.UpdateSubNotice();
            ResetSelection();

            if (curListAgentType == AgentType.CombatAgent)
            {
                view.SetActiveEquipmentSlot(true);
                view.SetActiveExploreSlot(false);
                UpdateEquipmentSlots();
            }
            else if (curListAgentType == AgentType.ExploreAgent)
            {
                view.SetActiveEquipmentSlot(false);
                view.SetActiveExploreSlot(true);
                UpdateExploreSlots();
            }

            view.ResetScrollProgress();
        }

        public override void AddEvent()
        {
            agentModel.OnGetNewAgent += Refresh;
            agentModel.OnUpdateAgentEquipmentState += UpdateEquipmentSlots;
            agentModel.OnExploreStateChanged += OnExploreStateChanged;
            agentModel.OnAgentExploreReward += OnAgentExploreReward;
        }

        public override void RemoveEvent()
        {
            agentModel.OnGetNewAgent -= Refresh;
            agentModel.OnUpdateAgentEquipmentState -= UpdateEquipmentSlots;
            agentModel.OnExploreStateChanged -= OnExploreStateChanged;
            agentModel.OnAgentExploreReward -= OnAgentExploreReward;
        }

        public void ViewEventHandler(UIAgentList.EventType eventType, object data)
        {
            if (eventType == UIAgentList.EventType.OnClickAgent)
            {
                IAgent agent = (IAgent)data;
                agent.IsNew = false;
                mainView.UpdateSubNotice(agent.AgentType);
                mainView.UpdateNotice();
                Entity.player.Agent.SendMessageNoticeUpdate();

                if (curListAgentType == AgentType.CombatAgent)
                {
                    if (curSelectedAgent == agent)
                    {
                        SelectAgent(null);
                    }
                    else
                    {
                        SelectAgent(agent);
                        var slotIndex = slotStateList.FindIndex(v => v != null && v.IsUsingSlot && v.AgentID == agent.ID);
                    }

                    UpdateEquipmentSlots();
                }
                else if (curListAgentType == AgentType.ExploreAgent)
                {
                    if (curSelectedAgent == agent)
                        SelectAgent(null);
                    else
                        SelectAgent(agent);

                    UpdateExploreSlots();
                }
            }
            else if (eventType == UIAgentList.EventType.OnClickEquipmentSlot)
            {
                int slotIndex = (int)data;
                if (slotStateList[slotIndex] == null)
                    return;

                if (curSelectedAgent == null)
                {
                    if (slotStateList[slotIndex].IsUsingSlot)
                        view.FocusAgent(GetAgent(slotStateList[slotIndex].AgentID));
                }
                else if (slotStateList[slotIndex].IsUsingSlot == false || curSelectedAgent.ID != slotStateList[slotIndex].AgentID)
                {
                    RequestEquipAgent(slotIndex, curSelectedAgent);
                    ResetSelection();
                }
            }
            else if (eventType == UIAgentList.EventType.OnClickExploreSlot)
            {
                ExploreType exploreType = (ExploreType)data;
                ExploreAgent exploreAgent = curSelectedAgent as ExploreAgent;

                if (exploreAgent.IsExploring && exploreAgent.ProgressingExplore.Type == exploreType && exploreAgent.ProgressingExplore.RemainTime == 0)
                {
                    RequestAgentExploreReward(exploreAgent.ProgressingExplore);
                }
            }
            else if (eventType == UIAgentList.EventType.OnClickSlotCancel)
            {
                int slotIndex = (int)data;
                if (slotStateList[slotIndex] == null || slotStateList[slotIndex].IsUsingSlot == false)
                {
                    Debug.LogError("UI 규칙에 어긋나는 동작을 수행하려고 하였습니다.");
                    return;
                }

                RequestEquipAgent(slotIndex, null);
            }
        }

        private void RequestAgentExploreReward(AgentExploreState exploreState)
        {
            agentModel.RequestAgentExploreReward(exploreState, false).WrapNetworkErrors();           
        }

        void OnAgentExploreReward(AgentExploreState exploreState)
        {
            UI.ShowToastPopup(LocalizeKey._47339.ToText()); // 파견 보상을 획득하였습니다.
            foreach (var each in exploreState.Participants)
                view.UpdateAgent(each.AgentData.id);
            UpdateExploreSlots();
        }

        private void SelectAgent(IAgent agent)
        {
            view.SelectAgent(agent);
            curSelectedAgent = agent;
        }

        private void ResetSelection()
        {
            view.SelectAgent(null);
            curSelectedAgent = null;
        }

        private void RequestEquipAgent(int slotIndex, IAgent agent)
        {
            if (agent != null && agent.AgentType == AgentType.ExploreAgent)
            {
                Debug.LogError("파견 동료를 슬롯에 장착하려고 하였습니다.");
                return;
            }

            agentModel.RequestAgentSlotUpdate(slotStateList[slotIndex].SlotNo, agent != null ? agent.ID : 0).WrapNetworkErrors();
        }

        private List<int> updateTargetBuffer = new List<int>();

        private void UpdateEquipmentSlots()
        {
            if (curListAgentType != AgentType.CombatAgent)
                return;

            int index = 0;

            updateTargetBuffer.Clear();
            for (int i = 0; i < slotStateList.Count; ++i)
                if (slotStateList[i] != null && slotStateList[i].AgentID != 0)
                    updateTargetBuffer.Add(slotStateList[i].AgentID);

            slotStateList.Clear();

            foreach (var each in agentModel.GetSlotStates())
            {
                if (each.IsUsingSlot)
                {
                    view.SetEquipmentSlotState(index, EquipmentSlotState.Equipped, GetAgent(each.AgentID));
                    if (updateTargetBuffer.Contains(each.AgentID) == false)
                        updateTargetBuffer.Add(each.AgentID);
                }
                else
                {
                    if (curSelectedAgent == null)
                        view.SetEquipmentSlotState(index, EquipmentSlotState.Unequipped);
                    else
                        view.SetEquipmentSlotState(index, EquipmentSlotState.Plus);
                }

                ++index;
                slotStateList.Add(each);
            }

            for (int i = index; i < 4; ++i)
            {
                view.SetEquipmentSlotState(i, EquipmentSlotState.Invalid);
                slotStateList.Add(null);
            }

            for (int i = 0; i < updateTargetBuffer.Count; ++i)
                view.UpdateAgent(updateTargetBuffer[i]);
        }

        private void UpdateExploreSlots()
        {
            if (curListAgentType != AgentType.ExploreAgent)
                return;

            if (curSelectedAgent != null)
            {
                ExploreAgent exploreAgent = curSelectedAgent as ExploreAgent;

                int count = 0;
                ExploreType curExploreType = ExploreType.None;
                if (exploreAgent.IsExploring)
                    curExploreType = exploreAgent.ProgressingExplore.Type;

                bool isExploring = false;
                foreach (var each in exploreAgent.GetExploreTypes())
                    if (curExploreType == each)
                        isExploring = true;

                foreach (var each in exploreAgent.GetExploreTypes())
                {
                    bool isExploreTypeMatched = curExploreType == each;

                    if (curExploreType == each)
                    {
                        if (exploreAgent.ProgressingExplore.RemainTime == 0f)
                            view.SetExploreSlotState(count, ExploreSlotState.ExploreFinished, each);
                        else
                            view.SetExploreSlotState(count, ExploreSlotState.Exploring, each, exploreAgent.ProgressingExplore.RemainTime);
                    }
                    else
                    {
                        view.SetExploreSlotState(count, isExploring ? ExploreSlotState.CantExplore : ExploreSlotState.NoExploring, each);
                    }

                    ++count;
                }

                view.SetExploreSlotCount(count);
            }
            else
            {
                view.SetExploreSlotCount(0);
            }
        }

        private IAgent GetAgent(int agentID)
        {
            return agentModel.GetAllAgents().First(v => v.ID == agentID);
        }

        List<IAgent> result = new List<IAgent>();
        private IAgent[] GetAgentSlotViewInfos(AgentType agentType)
        {
            result.Clear();

            if (agentType == AgentType.CombatAgent)
            {
                foreach (var each in Entity.player.Agent.GetCombatAgents())
                    result.Add(each);
            }
            else
            {
                foreach (var each in Entity.player.Agent.GetExploreAgents())
                    result.Add(each);
            }

            return result.ToArray();
        }

        private void OnExploreStateChanged(int stageID, AgentExploreState exploreState)
        {
            if (curListAgentType == AgentType.CombatAgent)
                return;

            // 파견 보상 UI 에서 다시 보내기를 누른 경우
            if (exploreState != null)
            {
                foreach (var each in exploreState.Participants)
                    view.UpdateAgent(each.AgentData.id);
                UpdateExploreSlots();
            }
        }
    }
}