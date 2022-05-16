using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Ragnarok
{
    public class UIExploreDigCollectView : MonoBehaviour
    {
        public enum EventType { OnClickSlot, OnClickSendExplore }

        [SerializeField] GameObject prefab;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIButtonHelper sendButton;
        [SerializeField] UILabelHelper noAgentNoti;
        [SerializeField] UIAgentIconHelper[] selectedAgents;
        [SerializeField] UILabelHelper requiredTimeLabel;
        [SerializeField] UILabelHelper requiredTimePanelLabel;
        [SerializeField] UILabelHelper exploreTypeLabel;
        [SerializeField] UISprite exploreTypeSprite;
        [SerializeField] UILabelHelper stageNameLabel;
        [SerializeField] UIRewardHelper reward;
        [SerializeField] UILabelHelper rewardLabel;
        [SerializeField] UILabelHelper rewardCountLabel;
        [SerializeField] UILabelHelper labelMainTitle;

        private UIExploreAgentSlot.Info[] infos;
        private DigCollectExplorePresenter presenter;

        public void Init(DigCollectExplorePresenter presenter)
        {
            this.presenter = presenter;

            EventDelegate.Add(sendButton.OnClick, OnClickSend);
        }

        public void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._47401; // 동료 선택
            sendButton.LocalKey = LocalizeKey._47318; // 파견 보내기
            noAgentNoti.LocalKey = LocalizeKey._47402; // 보유한 동료가 없습니다.
        }

        public void ShowAgents(ExploreAgent[] exploreAgents)
        {
            noAgentNoti.SetActive(exploreAgents.Length == 0);

            infos = new UIExploreAgentSlot.Info[exploreAgents.Length];
            for (int i = 0; i < infos.Length; ++i)
                infos[i] = new UIExploreAgentSlot.Info()
                {
                    agent = exploreAgents[i],
                    isSelected = false,
                };

            wrapper.SpawnNewList(prefab, 0, 0);
            wrapper.SetRefreshCallback(OnElementRefresh);

            wrapper.Resize(infos.Length);
            wrapper.SetProgress(0);
        }

        public void UpdateSelection(ExploreAgent agent, bool value)
        {
            for (int i = 0; i < infos.Length; ++i)
                if (infos[i].agent == agent)
                {
                    infos[i].isSelected = value;
                    infos[i].UpdateSlot();
                    break;
                }
        }

        public void UpdatedSelectedAgents(List<ExploreAgent> agents)
        {
            for (int i = 0; i < selectedAgents.Length; ++i)
            {
                if (i < agents.Count)
                {
                    selectedAgents[i].SetActive(true);
                    selectedAgents[i].SetData(agents[i].AgentData);
                }
                else
                {
                    selectedAgents[i].SetActive(false);
                }
            }
        }

        public void SetStageDependantInfo(StageData stageData)
        {
            var type = stageData.agent_explore_type.ToEnum<ExploreType>();
            string exploreTypeName = type.ToExploreName();
            requiredTimePanelLabel.Text = LocalizeKey._47403.ToText().Replace(ReplaceKey.NAME, exploreTypeName); // 예상 {NAME} 시간
            exploreTypeLabel.Text = exploreTypeName;
            exploreTypeSprite.spriteName = type.ToSpriteName();
            stageNameLabel.Text = stageData.name_id.ToText();
            reward.SetData(stageData.GetExploreReward().data);
            rewardLabel.Text = LocalizeKey._47404.ToText().Replace(ReplaceKey.NAME, exploreTypeName); // 예상 {NAME} 수량

            int totalSeconds = stageData.GetExploreRequiredTime();
            TimeSpan span = TimeSpan.FromSeconds(totalSeconds);
            int totalHours = (int)span.TotalHours;
            int minutes = span.Minutes;
            int seconds = span.Seconds;
            requiredTimeLabel.Text = $"{totalHours:00}:{minutes:00}:{seconds:00}";
        }

        public void SetRewardCount(int count)
        {
            rewardCountLabel.Text = count.ToString("n0");
        }

        private void OnElementRefresh(GameObject go, int index)
        {
            var slot = go.GetComponent<UIExploreAgentSlot>();
            slot.SetAgent(infos[index], OnClickSlot);
        }

        private void OnClickSlot(ExploreAgent agent)
        {
            presenter.ViewEventHandler(EventType.OnClickSlot, agent);
        }

        private void OnClickSend()
        {
            presenter.ViewEventHandler(EventType.OnClickSendExplore, null);
        }
    }
}