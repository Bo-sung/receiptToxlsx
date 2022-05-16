using UnityEngine;
using System;
using Ragnarok.View;

namespace Ragnarok
{
    public class UIExploreAgentSlot : UIView
    {
        public class Info
        {
            public ExploreAgent agent;
            public bool isSelected;

            public Action UpdateEventHandler;

            public void UpdateSlot()
            {
                UpdateEventHandler?.Invoke();
            }
        }

        [SerializeField] UIAgentIconHelper agent;
        [SerializeField] GameObject selectionMark;
        [SerializeField] UILabelHelper remainTime;
        [SerializeField] GameObject completeMark;
        [SerializeField] GameObject mask;
        [SerializeField] UIButtonHelper button;
        [SerializeField] UILabelHelper labelComplete;

        private Info info;
        private Action<ExploreAgent> onClick;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(button.OnClick, OnClickButton);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(button.OnClick, OnClickButton);
        }

        protected override void OnLocalize()
        {
            labelComplete.LocalKey = LocalizeKey._47335; // 완료
        }

        public void SetAgent(Info info, Action<ExploreAgent> onClick)
        {
            if (this.info != null && this.info.UpdateEventHandler == Refresh)
                this.info.UpdateEventHandler = null;
            this.info = info;
            this.info.UpdateEventHandler = Refresh;
            this.onClick = onClick;
            Refresh();
        }

        private void OnClickButton()
        {
            onClick(info.agent);
        }

        private void Refresh()
        {
            ExploreAgent agentData = info.agent;

            agent.SetData(agentData.AgentData);

            if (agentData.IsExploring)
            {
                mask.SetActive(true);
                if (agentData.ProgressingExplore.RemainTime == 0)
                {
                    remainTime.gameObject.SetActive(false);
                    completeMark.SetActive(true);
                }
                else
                {                    
                    remainTime.gameObject.SetActive(true);
                    remainTime.Text = agentData.ProgressingExplore.RemainTime.ToStringTime();// $"{h:D2}:{m:D2}";
                    completeMark.SetActive(false);
                }
            }
            else
            {
                mask.SetActive(false);
                remainTime.gameObject.SetActive(false);
                completeMark.SetActive(false);
            }

            selectionMark.SetActive(info.isSelected);
        }
    }
}