using UnityEngine;
using System;

namespace Ragnarok
{
    public class UIAgentEquipmentSlot : MonoBehaviour
    {
        [SerializeField] UIButtonHelper slotButton;
        [SerializeField] UIButtonHelper slotCancelButton;

        [SerializeField] UIAgentIconHelper agentProfileSprite;
        [SerializeField] UILabelHelper jobChangeRequired;

        [SerializeField] GameObject selectionEffect;
        [SerializeField] TweenScale plusMarkTween;

        private int slotIndex;

        public event Action<int> OnClickSlot;
        public event Action<int> OnClickCancel;

        public void Init(int index)
        {
            slotIndex = index;
            selectionEffect.SetActive(false);
        }

        public void OnLocalize()
        {
            jobChangeRequired.Text = LocalizeKey._47313.ToText().Replace(ReplaceKey.INDEX, slotIndex); // {INDEX}차\n전직\n필요
        }

        public void AddEvents()
        {
            EventDelegate.Add(slotButton.OnClick, OnClickSlotButton);
            EventDelegate.Add(slotCancelButton.OnClick, OnClickSlotCancelButton);
        }

        public void RemoveEvents()
        {
            EventDelegate.Remove(slotButton.OnClick, OnClickSlotButton);
            EventDelegate.Remove(slotCancelButton.OnClick, OnClickSlotCancelButton);
        }

        public void SetSlotState(AgentListPresenter.EquipmentSlotState slotState, IAgent equippedAgent = null)
        {
            plusMarkTween.gameObject.SetActive(false);

            if (slotState == AgentListPresenter.EquipmentSlotState.Equipped)
            {
                slotButton.SetActive(true);
                agentProfileSprite.gameObject.SetActive(true);
                slotCancelButton.SetActive(true);
                jobChangeRequired.SetActive(false);
                agentProfileSprite.SetData(equippedAgent.AgentData);
            }
            else if (slotState == AgentListPresenter.EquipmentSlotState.Unequipped)
            {
                slotButton.SetActive(true);
                agentProfileSprite.gameObject.SetActive(false);
                slotCancelButton.SetActive(false);
                jobChangeRequired.SetActive(false);
            }
            else if (slotState == AgentListPresenter.EquipmentSlotState.Invalid)
            {
                slotButton.SetActive(false);
                agentProfileSprite.gameObject.SetActive(false);
                slotCancelButton.SetActive(false);
                jobChangeRequired.SetActive(true);
            }
            else if (slotState == AgentListPresenter.EquipmentSlotState.Plus)
            {
                slotButton.SetActive(true);
                agentProfileSprite.gameObject.SetActive(false);
                slotCancelButton.SetActive(false);
                jobChangeRequired.SetActive(false);

                plusMarkTween.gameObject.SetActive(true);
                plusMarkTween.PlayForward();
                plusMarkTween.ResetToBeginning();
            }
        }

        public void SetSelect(bool isSelected)
        {
            selectionEffect.SetActive(isSelected);
        }

        private void OnClickSlotButton()
        {
            OnClickSlot?.Invoke(slotIndex);
        }

        private void OnClickSlotCancelButton()
        {
            OnClickCancel?.Invoke(slotIndex);
        }
    }
}