using UnityEngine;
using System.Collections;
using System;

namespace Ragnarok
{
    public class UIAgentExploringStateSlot : MonoBehaviour
    {
        public enum Event { OnClickSlot }

        [SerializeField] UIButton slotButton;
        [SerializeField] UIAgentIconHelper agent;
        [SerializeField] UISprite exploreTypeSprite;
        [SerializeField] UILabelHelper exploreTypeLabel;
        [SerializeField] UILabelHelper stageNameLabel;
        [SerializeField] UIRewardHelper reward;
        [SerializeField] GameObject completeMark;
        [SerializeField] UILabelHelper epxloringNotice;
        [SerializeField] GameObject exploreMarkRoot;
        [SerializeField] GameObject exploreMark;
        [SerializeField] GameObject exploreEffect;
        [SerializeField] GameObject unLockPanel;
        [SerializeField] GameObject lockPanel;
        [SerializeField] UILabelHelper lockLabel;
        [SerializeField] GameObject noExploringProfile;
        [SerializeField] UILabelHelper remainTimeLabel;

        private StageData stageData;
        private AgentExploreState exploreState;
        private bool canExplore;
        private Action<StageData, Event> slotEventHandler;
        private bool isLock = false;

        private float timer = 0;

        private void Start()
        {
            EventDelegate.Add(slotButton.onClick, OnClickSlot);
        }

        public int StageID { get { return stageData == null ? -1 : (int)stageData.id; } }

        public void SetData(StageData stageData, AgentExploreState exploreState, bool canExplore, Action<StageData, Event> slotEventHandler)
        {
            isLock = false;
            this.stageData = stageData;
            this.exploreState = exploreState;
            this.canExplore = canExplore;
            this.slotEventHandler = slotEventHandler;

            unLockPanel.SetActive(true);
            lockPanel.SetActive(false);

            exploreTypeSprite.spriteName = ((ExploreType)(byte)stageData.agent_explore_type).ToSpriteName();
            exploreTypeLabel.Text = ((ExploreType)(byte)stageData.agent_explore_type).ToExploreName();
            stageNameLabel.Text = stageData.name_id.ToText();
            reward.SetData(stageData.GetExploreReward().data);

            noExploringProfile.SetActive(exploreState == null);
            agent.gameObject.SetActive(exploreState != null);

            if (exploreState != null)
            {
                AgentData firstAgent = null;
                foreach (var each in exploreState.Participants)
                {
                    firstAgent = each.AgentData;
                    break;
                }

                agent.SetData(firstAgent);
            }

            epxloringNotice.Text = stageData.agent_explore_type.ToEnum<ExploreType>().ToExploreLoadingName();

            completeMark.SetActive(exploreState != null && exploreState.RemainTime <= 0);
            epxloringNotice.gameObject.SetActive(exploreState != null && exploreState.RemainTime > 0);
            exploreMarkRoot.SetActive(exploreState == null);
            exploreMark.SetActive(!canExplore);
            exploreEffect.SetActive(canExplore);

            remainTimeLabel.gameObject.SetActive(exploreState != null && exploreState.RemainTime > 0);

            if (exploreState != null)
            {
                SetRequiredTimeLabel(exploreState.RemainTime);
            }
        }

        private void Update()
        {
            if (exploreState == null || !remainTimeLabel.gameObject.activeSelf)
                return;

            timer -= Time.deltaTime;
            if (timer > 0)
                return;
            timer = 1;

            SetRequiredTimeLabel(exploreState.RemainTime);

            if (exploreState.RemainTime <= 0)
                SetData(stageData, exploreState, canExplore, slotEventHandler);
        }

        private void SetRequiredTimeLabel(RemainTime time)
        {
            TimeSpan span = TimeSpan.FromMilliseconds(time);
            int totalHours = (int)span.TotalHours;
            int minutes = span.Minutes;
            int seconds = span.Seconds;
            remainTimeLabel.Text = $"{totalHours:00}:{minutes:00}:{seconds:00}";
        }

        public void SetLock(StageData prevStage)
        {
            stageData = null;
            exploreState = null;
            slotEventHandler = null;

            if (prevStage == null)
                return;

            isLock = true;
            noExploringProfile.SetActive(true);
            agent.gameObject.SetActive(false);
            unLockPanel.SetActive(false);
            lockPanel.SetActive(true);

            lockLabel.Text = LocalizeKey._47346.ToText() // {NAME} 클리어 후 이용 가능
                .Replace(ReplaceKey.NAME, prevStage.name_id.ToText());
        }

        private void OnClickSlot()
        {
            if (isLock)
                return;
            slotEventHandler(stageData, Event.OnClickSlot);
        }
    }
}

