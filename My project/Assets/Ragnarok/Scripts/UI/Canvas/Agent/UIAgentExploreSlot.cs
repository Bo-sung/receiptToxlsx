using UnityEngine;
using System;

namespace Ragnarok
{
    public class UIAgentExploreSlot : MonoBehaviour
    {
        [SerializeField] UIButtonHelper slotButton;
        [SerializeField] UISprite bgSprite;
        [SerializeField] UISprite exploreTypeIcon;
        [SerializeField] UILabelHelper exploreTypeLabel;
        [SerializeField] GameObject exploringEffect;
        [SerializeField] UILabelHelper remainTimeLabel;
        [SerializeField] GameObject exploreFinishEffect;
        [SerializeField] Color iconColorOnDisabled;
        [SerializeField] Color bgColorOnDisabled;
        
        private float timeToEnd;
        private float updateTimer = 0;
        private AgentListPresenter.ExploreSlotState curSlotState;
        private ExploreType curExploreType;

        public event Action<ExploreType> OnClickSlot;

        public void AddEvents()
        {
            EventDelegate.Add(slotButton.OnClick, OnClickSlotButton);
        }

        public void RemoveEvents()
        {
            EventDelegate.Remove(slotButton.OnClick, OnClickSlotButton);
        }

        public void SetSlotState(AgentListPresenter.ExploreSlotState slotState, ExploreType exploreType, float remainTime)
        {
            curSlotState = slotState;
            curExploreType = exploreType;

            exploreTypeIcon.spriteName = exploreType.ToSpriteName();
            exploreTypeLabel.Text = exploreType.ToExploreName();

            if (slotState == AgentListPresenter.ExploreSlotState.ExploreFinished)
            {
                exploreTypeIcon.color = iconColorOnDisabled;
                bgSprite.color = Color.white;
                exploringEffect.SetActive(true);
                remainTimeLabel.SetActive(false);
                exploreFinishEffect.SetActive(true);
            }
            else if (slotState == AgentListPresenter.ExploreSlotState.Exploring)
            {
                timeToEnd = Time.realtimeSinceStartup + remainTime / 1000.0f;
                
                exploreTypeIcon.color = iconColorOnDisabled;
                bgSprite.color = Color.white;
                exploringEffect.SetActive(true);
                remainTimeLabel.SetActive(true);
                exploreFinishEffect.SetActive(false);

                UpdateRemainTimeLabel();
            }
            else if (slotState == AgentListPresenter.ExploreSlotState.NoExploring)
            {
                exploreTypeIcon.color = iconColorOnDisabled;
                bgSprite.color = Color.white;
                exploringEffect.SetActive(false);
                remainTimeLabel.SetActive(false);
                exploreFinishEffect.SetActive(false);
            }
            else if (slotState == AgentListPresenter.ExploreSlotState.CantExplore)
            {
                exploreTypeIcon.color = iconColorOnDisabled;
                bgSprite.color = bgColorOnDisabled;
                exploringEffect.SetActive(false);
                remainTimeLabel.SetActive(false);
                exploreFinishEffect.SetActive(false);
            }
        }

        private void OnClickSlotButton()
        {
            OnClickSlot?.Invoke(curExploreType);
        }

        private void Update()
        {
            if (updateTimer > 0)
            {
                updateTimer -= Time.deltaTime;
                return;
            }

            updateTimer = 1f;

            if (curSlotState == AgentListPresenter.ExploreSlotState.Exploring)
                UpdateRemainTimeLabel();
        }

        private void UpdateRemainTimeLabel()
        {
            int remainTime = Math.Max(0, (int)(timeToEnd - Time.realtimeSinceStartup));
            int hour = remainTime / 3600;
            remainTime %= 3600;
            int minute = remainTime / 60;
            remainTime %= 60;
            int sec = remainTime;

            remainTimeLabel.Text = string.Format("{0:D2}:{1:D2}:{2:D2}", hour, minute, sec);

            if (timeToEnd < Time.realtimeSinceStartup)
                SetSlotState(AgentListPresenter.ExploreSlotState.ExploreFinished, curExploreType, 0.0f);
        }
    }
}