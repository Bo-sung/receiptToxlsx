using UnityEngine;
using System;

namespace Ragnarok
{
    public class UIAgentMaterialSlot : MonoBehaviour
    {
        [SerializeField] UIButtonHelper slotButton;
        [SerializeField] UIAgentIconHelper agentProfileSprite;
        [SerializeField] GameObject plus;
        [SerializeField] TweenScale scaleTween;
        [SerializeField] GameObject selectionEffect;

        public event Action<int> OnClick;

        private IAgent curShowingAgent;
        private int index;

        public void Init(int index)
        {
            this.index = index;
        }

        public void AddEvent()
        {
            EventDelegate.Add(slotButton.OnClick, OnClickButton);
        }

        public void RemoveEvent()
        {
            EventDelegate.Remove(slotButton.OnClick, OnClickButton);
        }

        public void SetSlot(IAgent agent, bool isNew)
        {
            if (agent != null)
            {
                slotButton.enabled = true;
                agentProfileSprite.gameObject.SetActive(true);
                agentProfileSprite.SetData(agent.AgentData);
                plus.SetActive(false);
                selectionEffect.SetActive(true);

                if (isNew)
                {
                    scaleTween.PlayForward();
                    scaleTween.ResetToBeginning();
                }
            }
            else
            {
                slotButton.enabled = false;
                agentProfileSprite.gameObject.SetActive(false);
                plus.SetActive(true);
                selectionEffect.SetActive(false);
            }

            curShowingAgent = agent;
        }

        private void OnClickButton()
        {
            OnClick?.Invoke(index);
        }
    }
}