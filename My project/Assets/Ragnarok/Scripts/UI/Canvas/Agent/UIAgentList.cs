using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public class AgentSlotViewInfo : IInfo
    {
        public AgentData AgentData { get; private set; }
        public IAgent OwningAgent { get; private set; }

        public bool IsSelected
        {
            get { return isSelected; }

            set
            {
                if (isSelected == value)
                    return;
                isSelected = value;
                OnUpdateEvent?.Invoke();
            }
        }

        private bool isSelected = false;

        public bool IsInvalidData => false;

        public int DuplicationCount { get; set; }
        public bool HideDuplicationCountOnZero { get; set; }
        public bool ShowNoDuplicationMask { get; set; }
        public bool ShowAgentStatePanel { get; set; }
        public bool ShowAgentTypePanel { get; set; }
        public bool ShowAgentNewIcon { get; set; }

        public event Action OnUpdateEvent;
        public event Action<IAgent> OnClick;
        public event Action OnFocus;

        public AgentSlotViewInfo(AgentData agentData, IAgent agent)
        {
            AgentData = agentData;
            OwningAgent = agent;
            DuplicationCount = agent != null ? agent.DuplicationCount : 0;
        }

        public void InvokeOnClick()
        {
            OnClick?.Invoke(OwningAgent);
        }

        public void Update()
        {
            OnUpdateEvent?.Invoke();
        }

        public void Focus()
        {
            OnFocus?.Invoke();
        }
    }

    public sealed class UIAgentList : MonoBehaviour
    {
        public enum EventType
        {
            OnClickEquipmentSlot,
            OnClickExploreSlot,
            OnClickAgent,
            OnChangeAgentListTab,
            OnClickSlotCancel
        }

        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] GameObject equipmentSlotRoot;
        [SerializeField] GameObject exploreSlotRoot;
        [SerializeField] UIAgentEquipmentSlot[] agentEquipmentSlots;
        [SerializeField] UIAgentExploreSlot[] agentExploreSlots;
        [SerializeField] UIGridHelper agentExploreSlotGrid;
        [SerializeField] UIButtonHelper storeButton;

        private AgentListPresenter presenter;
        private List<AgentSlotViewInfo> agentSlotViewInfos = new List<AgentSlotViewInfo>();

        private int curTab;
        private float slotHeight;
        private float listPanelHeight;
        private float listHeight;

        public void OnInit(AgentListPresenter presenter)
        {
            slotHeight = Mathf.Abs(wrapper.Grid.cellHeight);
            listPanelHeight = Mathf.Abs(wrapper.Panel.height);
            listHeight = 0;

            this.presenter = presenter;

            wrapper.SpawnNewList(prefab, 0, 0);
            wrapper.SetRefreshCallback(OnElementRefresh);

            EventDelegate.Add(storeButton.OnClick, GoToStore);

            for (int i = 0; i < agentEquipmentSlots.Length; ++i)
            {
                agentEquipmentSlots[i].Init(i);
                agentEquipmentSlots[i].OnClickSlot += OnClickEquipmentSlot;
                agentEquipmentSlots[i].OnClickCancel += OnClickEquipmentSlotCancel;
                agentEquipmentSlots[i].AddEvents();
            }

            for (int i = 0; i < agentExploreSlots.Length; ++i)
            {
                agentExploreSlots[i].OnClickSlot += OnClickExploreSlot;
                agentExploreSlots[i].AddEvents();
            }

            curTab = 0;
        }

        public void OnClose()
        {
            EventDelegate.Remove(storeButton.OnClick, GoToStore);

            for (int i = 0; i < agentEquipmentSlots.Length; ++i)
            {
                agentEquipmentSlots[i].OnClickSlot -= OnClickEquipmentSlot;
                agentEquipmentSlots[i].OnClickCancel -= OnClickEquipmentSlotCancel;
                agentEquipmentSlots[i].RemoveEvents();
            }

            for (int i = 0; i < agentExploreSlots.Length; ++i)
            {
                agentExploreSlots[i].OnClickSlot -= OnClickExploreSlot;
                agentExploreSlots[i].RemoveEvents();
            }
        }

        private void OnElementRefresh(GameObject go, int index)
        {
            var slot = go.GetComponent<UIAgentSlot>();
            slot.SetData(agentSlotViewInfos[index]);
        }

        public void OnLocalize()
        {
            for (int i = 0; i < agentEquipmentSlots.Length; ++i)
                agentEquipmentSlots[i].OnLocalize();

            storeButton.LocalKey = LocalizeKey._47348; // 동료 상점
        }

        public void ShowAgents(AgentType agentTypeToShow, AgentData[] agentDatas, IAgent[] agents)
        {
            agentSlotViewInfos.Clear();

            for (int i = 0; i < agentDatas.Length; ++i)
            {
                if (agentDatas[i].agent_type != (int)agentTypeToShow)
                    continue;

                IAgent found = agents.FirstOrDefault(v => v.ID == agentDatas[i].id);
                var newInfo = new AgentSlotViewInfo(agentDatas[i], found);
                newInfo.ShowNoDuplicationMask = false;
                newInfo.HideDuplicationCountOnZero = true;
                newInfo.ShowAgentStatePanel = true;
                newInfo.ShowAgentTypePanel = false;
                newInfo.ShowAgentNewIcon = true;
                if (found != null)
                    newInfo.OnClick += OnClickAgentSlot;
                agentSlotViewInfos.Add(newInfo);
            }

            agentSlotViewInfos.Sort((a, b) =>
            {
                int aPriority = 0;
                aPriority += a.AgentData.agent_rating;
                if (a.OwningAgent != null)
                    aPriority += 10;
                if (a.OwningAgent != null && a.OwningAgent.IsNew)
                    aPriority += 100;

                int bPriority = 0;
                bPriority += b.AgentData.agent_rating;
                if (b.OwningAgent != null)
                    bPriority += 10;
                if (b.OwningAgent != null && b.OwningAgent.IsNew)
                    bPriority += 100;


                if (aPriority == bPriority)
                    return b.AgentData.id - a.AgentData.id;
                else
                    return -(aPriority - bPriority);
            });

            wrapper.Resize(agentSlotViewInfos.Count);

            listHeight = agentSlotViewInfos.Count * slotHeight;
        }

        public void OpenSubCombatAgent()
        {
            presenter.ViewEventHandler(EventType.OnChangeAgentListTab, AgentType.CombatAgent);
        }

        public void OpenSubExploreAgent()
        {
            presenter.ViewEventHandler(EventType.OnChangeAgentListTab, AgentType.ExploreAgent);
        }

        private void OnClickEquipmentSlot(int slot)
        {
            presenter.ViewEventHandler(EventType.OnClickEquipmentSlot, slot);
        }

        private void OnClickExploreSlot(ExploreType slotExploreType)
        {
            presenter.ViewEventHandler(EventType.OnClickExploreSlot, slotExploreType);
        }

        private void OnClickEquipmentSlotCancel(int slot)
        {
            presenter.ViewEventHandler(EventType.OnClickSlotCancel, slot);
        }

        private void OnClickAgentSlot(IAgent agent)
        {
            presenter.ViewEventHandler(EventType.OnClickAgent, agent);
        }

        public void SetActiveEquipmentSlot(bool value)
        {
            equipmentSlotRoot.SetActive(value);
        }

        public void SetActiveExploreSlot(bool value)
        {
            exploreSlotRoot.SetActive(value);
        }

        public void SetEquipmentSlotState(int slotIndex, AgentListPresenter.EquipmentSlotState slotState, IAgent equippedAgent = null)
        {
            agentEquipmentSlots[slotIndex].SetSlotState(slotState, equippedAgent);
        }

        public void SetExploreSlotState(int slotIndex, AgentListPresenter.ExploreSlotState slotState, ExploreType exploreType, float remainTime = 0f)
        {
            agentExploreSlots[slotIndex].SetSlotState(slotState, exploreType, remainTime);
        }

        public void SetExploreSlotCount(int slotCount)
        {
            for (int i = 0; i < agentExploreSlots.Length; ++i)
                agentExploreSlots[i].gameObject.SetActive(i < slotCount);
            agentExploreSlotGrid.Reposition();
        }

        public void UpdateAgent(int agentID)
        {
            var viewInfo = agentSlotViewInfos.FirstOrDefault(v => v.OwningAgent.ID == agentID);
            if (viewInfo != null)
                viewInfo.Update();
        }

        public void SelectAgent(IAgent agent)
        {
            for (int i = 0; i < agentSlotViewInfos.Count; ++i)
                agentSlotViewInfos[i].IsSelected = agent == null ? false : (agentSlotViewInfos[i].OwningAgent != null && agentSlotViewInfos[i].OwningAgent.ID == agent.ID);
        }

        public void FocusAgent(IAgent agent)
        {
            for (int i = 0; i < agentSlotViewInfos.Count; ++i)
            {
                if (agentSlotViewInfos[i].OwningAgent != null && agentSlotViewInfos[i].OwningAgent.ID == agent.ID)
                {
                    if (listHeight > listPanelHeight)
                    {
                        float posCorToOne = listHeight - listPanelHeight;
                        float yPos = i * slotHeight;
                        float prog = Mathf.Clamp01(yPos / posCorToOne);
                        wrapper.SetProgress(prog);
                    }

                    agentSlotViewInfos[i].Focus();
                }
            }
        }

        public void ResetScrollProgress()
        {
            wrapper.SetProgress(0);
        }

        private void GoToStore()
        {
            UI.Show<UIShop>().Set(UIShop.ViewType.Default, ShopTabType.Box);
        }
    }
}