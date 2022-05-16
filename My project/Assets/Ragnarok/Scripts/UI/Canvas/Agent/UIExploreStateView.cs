using UnityEngine;
using System.Collections;
using System;

namespace Ragnarok
{
    public class UIExploreStateView : MonoBehaviour
    {
        public enum EventType { OnClickComplete, OnClickRetire }

        [SerializeField] UIAgentIconHelper[] participantIcons;
        [SerializeField] UILabelHelper requiredTimeLabel;
        [SerializeField] UILabelHelper requiredTimePanelLabel;
        [SerializeField] UILabelHelper exploreTypeLabel;
        [SerializeField] UISprite exploreTypeSprite;
        [SerializeField] UILabelHelper stageNameLabel;
        [SerializeField] UIRewardHelper reward;
        [SerializeField] UILabelHelper rewardLabel;
        [SerializeField] UILabelHelper rewardCountLabel;
        [SerializeField] UICostButtonHelper completeButton;
        [SerializeField] UIButtonHelper retireButton;
        [SerializeField] UILabelHelper exploreStateNoti;
        [SerializeField] UILabelHelper labelMainTitle;

        private ExploreStatePresenter presenter;
        private ExploreAgent[] participants;
        private float timer = 1;
        private AgentExploreState exploreState;

        public void Init(ExploreStatePresenter presenter)
        {
            this.presenter = presenter;

            EventDelegate.Add(completeButton.OnClick, OnClickComplete);
            EventDelegate.Add(retireButton.OnClick, OnClickRetire);
        }

        public void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._47400; // 파견 정보
            retireButton.LocalKey = LocalizeKey._47414; // 포기
            completeButton.LocalKey = LocalizeKey._47415; // 즉시 완료
        }

        public void SetData(StageData stageData, AgentExploreState exploreState)
        {
            this.exploreState = exploreState;
            completeButton.SetCostCount(presenter.GetCatcoinFastClear(exploreState.RemainTime));

            var type = stageData.agent_explore_type.ToEnum<ExploreType>();
            string exploreTypeName = type.ToExploreName();
            requiredTimePanelLabel.Text = LocalizeKey._47403.ToText().Replace(ReplaceKey.NAME, exploreTypeName);
            exploreTypeLabel.Text = exploreTypeName;
            exploreTypeSprite.spriteName = type.ToSpriteName();
            stageNameLabel.Text = stageData.name_id.ToText();
            reward.SetData(stageData.GetExploreReward().data);
            rewardLabel.Text = LocalizeKey._47404.ToText().Replace(ReplaceKey.NAME, exploreTypeName);

            int index = 0;
            ExploreAgent firstAgent = null;
            int agentCount = 0;

            foreach (var each in exploreState.Participants)
            {
                if (firstAgent == null)
                    firstAgent = each;
                ++agentCount;

                // 방어코드
                if (index >= participantIcons.Length)
                {
                    Debug.LogError($"파견 동료 슬롯수보다 많이 들어옴= {nameof(index)}={index}, {nameof(each.ID)}={each.ID}, {each.AgentData.name_id.ToText()}");
                    continue;
                }

                participantIcons[index].SetActive(true);
                participantIcons[index].SetData(each.AgentData);
                ++index;
            }

            for (int i = index; i < participantIcons.Length; ++i)
            {
                participantIcons[i].SetActive(false);
            }

            SetRequiredTimeLabel(exploreState.RemainTime);

            if (stageData.agent_explore_type.ToEnum<ExploreType>() == ExploreType.Trade)
            {
                rewardCountLabel.Text = stageData.agent_trade_ropoint.ToString("N0");
            }
            else if (stageData.agent_explore_type.ToEnum<ExploreType>() == ExploreType.Production)
            {
                rewardCountLabel.Text = stageData.GetExploreReward().data.Count.ToString("N0");
            }
            else
            {
                int totalRewardCount = 0;

                foreach (ExploreAgent each in exploreState.Participants)
                {
                    totalRewardCount += stageData.GetExploreRewardCountRating(each.AgentData.agent_rating);
                }

                rewardCountLabel.Text = totalRewardCount.ToString("n0");
            }

            string c = LocalizeKey._47324.ToText(); // 이
            string d = LocalizeKey._47325.ToText(); // 가
            string e = LocalizeKey._47326.ToText(); // 과
            string f = LocalizeKey._47327.ToText(); // 와

            string stageName = stageData.name_id.ToText();
            KLUtil.Divide(stageName[stageName.Length - 1], out char _, out char _, out char jong1);

            string agentName = firstAgent.AgentData.name_id.ToText();
            KLUtil.Divide(agentName[agentName.Length - 1], out char _, out char _, out char jong2);

            string text = null;

            if (agentCount == 1)
            {
                text = LocalizeKey._47405.ToText() // {NAME1}{POSTPOSITION1} {NAME2}{POSTPOSITION2} {NAME3}을 하러 떠납니다.
                    .Replace(ReplaceKey.NAME, 1, agentName)
                    .Replace(ReplaceKey.POSTPOSITION, jong2 == ' ' ? d : c)
                    .Replace(ReplaceKey.NAME, 2, stageName)
                    .Replace(ReplaceKey.NAME, 3, stageData.agent_explore_type.ToEnum<ExploreType>().ToExploreName());
            }
            else
            {
                text = LocalizeKey._47406.ToText() // {NAME1}{POSTPOSITION1} {NAME2}{POSTPOSITION2} 동료 {COUNT}명이 {NAME3}을 하러 떠납니다.
                    .Replace(ReplaceKey.NAME, 1, agentName)
                    .Replace(ReplaceKey.POSTPOSITION, jong2 == ' ' ? f : e)
                    .Replace(ReplaceKey.NAME, 2, stageName)
                    .Replace(ReplaceKey.COUNT, agentCount - 1)
                    .Replace(ReplaceKey.NAME, 3, stageData.agent_explore_type.ToEnum<ExploreType>().ToExploreName());
            }

            exploreStateNoti.Text = text;
        }

        private void Update()
        {
            if (exploreState == null)
                return;

            timer -= Time.deltaTime;

            if (timer > 0)
                return;

            timer = 1f;

            SetRequiredTimeLabel(exploreState.RemainTime);
            completeButton.SetCostCount(presenter.GetCatcoinFastClear(exploreState.RemainTime));
        }

        private void SetRequiredTimeLabel(RemainTime time)
        {
            TimeSpan span = TimeSpan.FromMilliseconds(time);
            int totalHours = (int)span.TotalHours;
            int minutes = span.Minutes;
            int seconds = span.Seconds;
            requiredTimeLabel.Text = $"{totalHours:00}:{minutes:00}:{seconds:00}";
        }

        private void OnClickComplete()
        {
            presenter.ViewEventHandler(EventType.OnClickComplete);
        }

        private void OnClickRetire()
        {
            presenter.ViewEventHandler(EventType.OnClickRetire);
        }
    }
}

