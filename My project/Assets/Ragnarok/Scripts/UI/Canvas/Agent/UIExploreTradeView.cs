using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Ragnarok
{
    public class UIExploreTradeView : MonoBehaviour
    {
        public enum EventType { OnClickAgent, OnClickSend }

        [SerializeField] GameObject prefab;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIButtonHelper sendButton;
        [SerializeField] UILabelHelper exploreTypeLabel;
        [SerializeField] UISprite exploreTypeSprite;
        [SerializeField] UILabelHelper stageNameLabel;
        [SerializeField] UIRewardHelper reward;
        [SerializeField] UILabelHelper rewardLabel;
        [SerializeField] UILabelHelper rewardCountLabel;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UILabelHelper noAgentNotice;
        [SerializeField] UILabelHelper remainTradeCountLabel;
        [SerializeField] UILabelHelper requiredTimeLabel;
        [SerializeField] UILabelHelper requiredTimePanelLabel;

        private TradeExplorePresenter presenter;
        private StageData stageData;

        private UIExploreAgentSlot.Info[] infos;

        public void Init(TradeExplorePresenter presenter)
        {
            this.presenter = presenter;
            EventDelegate.Add(sendButton.OnClick, OnClickSend);
        }

        public void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._47401; // 동료 선택
            sendButton.LocalKey = LocalizeKey._47318; // 파견 보내기
            noAgentNotice.LocalKey = LocalizeKey._47402; // 보유한 동료가 없습니다.
        }

        public void SetStageDependantInfo(StageData stageData)
        {
            this.stageData = stageData;
            var type = stageData.agent_explore_type.ToEnum<ExploreType>();
            string exploreTypeName = type.ToExploreName();
            requiredTimePanelLabel.Text = LocalizeKey._47403.ToText().Replace(ReplaceKey.NAME, exploreTypeName); // 예상 {NAME} 시간
            exploreTypeLabel.Text = exploreTypeName;
            exploreTypeSprite.spriteName = type.ToSpriteName();
            stageNameLabel.Text = stageData.name_id.ToText();
            reward.SetData(stageData.GetExploreReward().data);
            rewardLabel.Text = LocalizeKey._47418.ToText().Replace(ReplaceKey.NAME, exploreTypeName); // {NAME} 보상
            rewardCountLabel.Text = stageData.GetExploreReward().data.Count.ToString("n0");
            requiredTimeLabel.Text = "-";
        }

        public void SetRemainTradeCount(int curCount, int maxCount)
        {
            remainTradeCountLabel.Text = string.Format("{0} / {1}", curCount, maxCount);
        }

        public void SetAgents(List<ExploreAgent> agents)
        {
            noAgentNotice.gameObject.SetActive(agents.Count == 0);

            infos = new UIExploreAgentSlot.Info[agents.Count];
            for (int i = 0; i < agents.Count; ++i)
                infos[i] = new UIExploreAgentSlot.Info()
                {
                    agent = agents[i],
                    isSelected = false
                };

            wrapper.SpawnNewList(prefab, 0, 0);
            wrapper.SetRefreshCallback(OnElementRefresh);
            wrapper.Resize(infos.Length); // 총 5개의 등급이 있다.
            wrapper.SetProgress(0);
        }

        public void SelectAgent(ExploreAgent agent)
        {
            for (int i = 0; i < infos.Length; ++i)
            {
                var each = infos[i];

                if (each.isSelected && each.agent != agent)
                {
                    each.isSelected = false;
                    each.UpdateSlot();
                }
                else if (!each.isSelected && each.agent == agent)
                {
                    each.isSelected = true;
                    each.UpdateSlot();
                    int totalSeconds = stageData.GetExploreRequiredTime(agent.AgentData.agent_rating);
                    TimeSpan span = TimeSpan.FromSeconds(totalSeconds);
                    int totalHours = (int)span.TotalHours;
                    int minutes = span.Minutes;
                    int seconds = span.Seconds;
                    requiredTimeLabel.Text = $"{totalHours:00}:{minutes:00}:{seconds:00}";
                }
            }
        }

        private void OnElementRefresh(GameObject go, int index)
        {
            var slot = go.GetComponent<UIExploreAgentSlot>();
            slot.SetAgent(infos[index], OnClickAgent);
        }

        private void OnClickSend()
        {
            presenter.ViewEventHandler(EventType.OnClickSend, null);
        }

        private void OnClickAgent(ExploreAgent agent)
        {
            presenter.ViewEventHandler(EventType.OnClickAgent, agent);
        }
    }
}