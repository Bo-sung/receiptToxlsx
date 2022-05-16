using UnityEngine;

namespace Ragnarok.View
{
    public class CardOptionSlotView : UIView
    {
        [SerializeField] byte index;
        [SerializeField] UITextureHelper cardOptionPanel;
        [SerializeField] CardOptionListView cardOptionListView;
        [SerializeField] GameObject lockInfo;
        [SerializeField] GameObject empty;
        [SerializeField] UILabelHelper labelLock;
        [SerializeField] UILabelHelper labelEmpty;
        [SerializeField] UIButtonHelper btnInfo;

        public event UIEquipmentInfo.SelectCardSlotEvent OnSelect;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnInfo.OnClick, OnClickedBtnInfo);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnInfo.OnClick, OnClickedBtnInfo);
        }

        protected override void OnLocalize()
        {
            labelEmpty.LocalKey = LocalizeKey._16005; // 장착된 카드가 없습니다.
        }

        public void SetData(CardSlotState slotState, ItemInfo info, string itemName)
        {
            cardOptionListView.SetActive(false);
            lockInfo.SetActive(false);
            empty.SetActive(false);
            btnInfo.SetActive(false);
            btnInfo.SetActive(false);

            switch (slotState)
            {
                case CardSlotState.Lock:
                    lockInfo.SetActive(true);
                    labelLock.Text = LocalizeKey._16014.ToText()
                           .Replace(ReplaceKey.VALUE, BasisType.EQUIPMENT_CARD_SLOT_UNLOCK_LEVEL.GetInt(index + 1)); // [+VALUE강] 달성 시, 카드 슬롯 해금
                    break;

                case CardSlotState.Empty:
                    empty.SetActive(true);
                    break;

                case CardSlotState.Use:
                    cardOptionPanel.SetCardOptionPanel(info.CardOptionPanelName);
                    cardOptionListView.SetData(info);
                    cardOptionListView.SetActive(true);
                    btnInfo.SetActive(true);
                    break;

                case CardSlotState.Shadow:
                case CardSlotState.ShadowLock:
                    lockInfo.SetActive(true);
                    labelLock.Text = LocalizeKey._16019.ToText() // [c][84A2EC][{NAME}][-][/c] 아이템 필요
                            .Replace(ReplaceKey.NAME, itemName);
                    break;
            }
        }

        void OnClickedBtnInfo()
        {
            OnSelect?.Invoke(index, CardSlotEvent.CardInfo);
        }
    }
}