using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View.EquipmentView
{
    public class CardOptionView : UIView, IInspectorFinder
    {
        [SerializeField] UIScrollView scrollView;
        [SerializeField] CardOptionSlotView[] slots;

        public event UIEquipmentInfo.SelectCardSlotEvent OnSelect;

        protected override void Awake()
        {
            base.Awake();
            foreach (var slot in slots)
            {
                slot.OnSelect += OnSelectCardSlot;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var slot in slots)
            {
                slot.OnSelect -= OnSelectCardSlot;
            }
            Timing.KillCoroutines(gameObject);
        }

        protected override void OnLocalize()
        {

        }

        public void SetData(ItemInfo info, string itemName)
        {
            int maxSlot = info.GetMaxCardSlot();
            for (int i = 0; i < slots.Length; i++)
            {
                if (i < maxSlot)
                {
                    slots[i].SetActive(true);
                    CardSlotState slotState = CardSlotState.Lock;
                    bool isOpen = info.IsOpenCardSlot(i);
                    bool isValid = info.GetCardItem(i) != null;

                    if (isOpen)
                        slotState = CardSlotState.Empty;

                    if (isValid)
                        slotState = CardSlotState.Use;

                    if (info.IsShadow)
                    {
                        if (i == info.CardSlotCount)
                        {
                            slotState = CardSlotState.ShadowLock;
                        }
                        else if (i > info.CardSlotCount)
                        {
                            slotState = CardSlotState.Shadow;
                        }
                    }

                    slots[i].SetData(slotState, info.GetCardItem(i), itemName);
                }
                else
                {
                    slots[i].SetActive(false);
                }
            }
        }

        public void ResetPosition()
        {
            Timing.RunCoroutine(YieldResetPotition(), gameObject);
        }

        IEnumerator<float> YieldResetPotition()
        {
            yield return Timing.WaitForOneFrame;
            yield return Timing.WaitForOneFrame;
            scrollView.ResetPosition();
        }

        private void OnSelectCardSlot(byte slotIndex, CardSlotEvent cardSlotEvent)
        {
            OnSelect?.Invoke(slotIndex, cardSlotEvent);
        }

        bool IInspectorFinder.Find()
        {
            slots = GetComponentsInChildren<CardOptionSlotView>();
            return true;
        }
    }
}