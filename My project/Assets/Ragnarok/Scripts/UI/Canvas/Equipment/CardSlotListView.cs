using UnityEngine;

namespace Ragnarok.View.EquipmentView
{
    public class CardSlotListView : UIView, IInspectorFinder
    {
        [SerializeField] CardSlotView[] cardSlots;

        public event UIEquipmentInfo.SelectCardSlotEvent OnSelect;

        protected override void Awake()
        {
            base.Awake();
            foreach (var slot in cardSlots)
            {
                slot.OnSelect += OnSelectCardSlot;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var slot in cardSlots)
            {
                slot.OnSelect -= OnSelectCardSlot;
            }
        }

        protected override void OnLocalize()
        {

        }

        public void SetData(ItemInfo info, bool isEditable, string iconName)
        {
            int maxSlot = info.GetMaxCardSlot();
            for (int i = 0; i < cardSlots.Length; i++)
            {
                if (i < maxSlot)
                {
                    cardSlots[i].SetActive(true);
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

                    cardSlots[i].SetData(slotState, info.GetCardItem(i), isEditable, iconName);
                }
                else
                {
                    cardSlots[i].SetActive(false);
                }
            }
        }

        private void OnSelectCardSlot(byte slotIndex, CardSlotEvent cardSlotEvent)
        {
            OnSelect?.Invoke(slotIndex, cardSlotEvent);
        }

        bool IInspectorFinder.Find()
        {
            cardSlots = GetComponentsInChildren<CardSlotView>();
            return true;
        }
    }
}