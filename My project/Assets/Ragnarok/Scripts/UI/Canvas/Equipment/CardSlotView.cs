using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class CardSlotView : UIView
    {
        [SerializeField] byte index;
        [SerializeField] UICardProfile cardProfile;
        [SerializeField] UIButtonHelper btnSmelt;
        [SerializeField] UIButtonHelper btnAutoEquip;
        [SerializeField] UIButtonHelper btnUnEquip;
        [SerializeField] UIButtonHelper btnEmpty;
        [SerializeField] UIButtonHelper btnInfo;
        [SerializeField] GameObject lockInfo;
        [SerializeField] UILabelHelper labelLock;
        [SerializeField] GameObject goShadowLock;
        [SerializeField] UIButtonWithIconHelper btnUnLock;

        public event UIEquipmentInfo.SelectCardSlotEvent OnSelect;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnSmelt.OnClick, OnClickedBtnSmelt);
            EventDelegate.Add(btnAutoEquip.OnClick, OnClickedBtnAutoEquip);
            EventDelegate.Add(btnUnEquip.OnClick, ClickedBtnUnEquip);
            EventDelegate.Add(btnEmpty.OnClick, ClickedBtnEmpty);
            EventDelegate.Add(btnInfo.OnClick, ClickedBtnInfo);
            EventDelegate.Add(btnUnLock.OnClick, ClickedBtnUnLock);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnSmelt.OnClick, OnClickedBtnSmelt);
            EventDelegate.Remove(btnAutoEquip.OnClick, OnClickedBtnAutoEquip);
            EventDelegate.Remove(btnUnEquip.OnClick, ClickedBtnUnEquip);
            EventDelegate.Remove(btnEmpty.OnClick, ClickedBtnEmpty);
            EventDelegate.Remove(btnInfo.OnClick, ClickedBtnInfo);
            EventDelegate.Remove(btnUnLock.OnClick, ClickedBtnUnLock);
        }

        protected override void OnLocalize()
        {
            btnSmelt.LocalKey = LocalizeKey._16017; // 카드 강화
            btnAutoEquip.LocalKey = LocalizeKey._16016; // 자동 장착
            labelLock.Text = LocalizeKey._16015.ToText()
                .Replace(ReplaceKey.VALUE, BasisType.EQUIPMENT_CARD_SLOT_UNLOCK_LEVEL.GetInt(index + 1)); // [+{VALUE}강]\n달성 시\n카드 슬롯\n 해금
        }

        public void SetData(CardSlotState slotState, ItemInfo info, bool isEditable, string iconName)
        {
            cardProfile.SetActive(false);
            btnSmelt.SetActive(false);
            btnAutoEquip.SetActive(false);
            btnUnEquip.SetActive(false);
            btnEmpty.SetActive(false);
            lockInfo.SetActive(false);
            btnInfo.SetActive(false);
            goShadowLock.SetActive(false);
            btnUnLock.SetActive(false);
            btnUnLock.SetIconName(iconName);

            switch (slotState)
            {
                case CardSlotState.Lock:
                    {
                        lockInfo.SetActive(true);
                    }
                    break;

                case CardSlotState.Empty:
                    {
                        btnAutoEquip.SetActive(true);
                        btnEmpty.SetActive(true);
                    }
                    break;

                case CardSlotState.Use:
                    {
                        cardProfile.SetData(info);
                        cardProfile.SetActive(true);
                        btnSmelt.SetActive(true);
                        btnUnEquip.SetActive(true);
                        btnInfo.SetActive(true);
                    }
                    break;

                case CardSlotState.Shadow:
                    {
                        goShadowLock.SetActive(true);
                    }
                    break;

                case CardSlotState.ShadowLock:
                    {
                        goShadowLock.SetActive(true);
                        btnUnLock.SetActive(true);
                    }
                    break;
            }

            if (!isEditable)
            {
                btnSmelt.SetActive(false);
                btnAutoEquip.SetActive(false);
                btnUnEquip.SetActive(false);
                btnEmpty.SetActive(false);
                lockInfo.SetActive(false);
                btnInfo.SetActive(false);
                goShadowLock.SetActive(false);
            }
        }

        void OnClickedBtnSmelt()
        {
            OnSelect?.Invoke(index, CardSlotEvent.Smelt);
        }

        void OnClickedBtnAutoEquip()
        {
            OnSelect?.Invoke(index, CardSlotEvent.AutoEquip);
        }

        void ClickedBtnUnEquip()
        {
            OnSelect?.Invoke(index, CardSlotEvent.UnEquip);
        }

        void ClickedBtnEmpty()
        {
            OnSelect?.Invoke(index, CardSlotEvent.CardInven);
        }

        void ClickedBtnInfo()
        {
            OnSelect?.Invoke(index, CardSlotEvent.CardInfo);
        }

        void ClickedBtnUnLock()
        {
            OnSelect?.Invoke(index, CardSlotEvent.SlotUnLock);
        }
    }
}