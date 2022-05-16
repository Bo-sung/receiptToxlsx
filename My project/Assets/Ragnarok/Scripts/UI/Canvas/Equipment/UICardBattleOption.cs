using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UICardBattleOption : UIInfo<ItemInfo>
    {
        [SerializeField] byte index;
        [SerializeField] UISprite icon;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UIButtonHelper btnCardInfo;
        [SerializeField] UIButtonHelper btnEnchant;
        [SerializeField] UILabelValue[] battleOptions;
        [SerializeField] TweenColor labName_TweenColor;
        [SerializeField] UIButtonHelper btnUnEquip;
        [SerializeField] GameObject cardLockInfo, cardUnLockInfo;
        [SerializeField] UILabelHelper labelLock;

        public event Action<byte> OnSelectEquip;
        public event Action<byte> OnSelectUnEquip;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnCardInfo.OnClick, OnClickedBtnCardInfo);
            EventDelegate.Add(btnEnchant.OnClick, OnClickedBtnEnchant);
            EventDelegate.Add(btnUnEquip.OnClick, ClickedBtnUnEquip);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnCardInfo.OnClick, OnClickedBtnCardInfo);
            EventDelegate.Remove(btnEnchant.OnClick, OnClickedBtnEnchant);
            EventDelegate.Remove(btnUnEquip.OnClick, ClickedBtnUnEquip);
        }

        protected override void OnLocalize()
        {
            btnCardInfo.LocalKey = LocalizeKey._16003; // 정보
            btnEnchant.LocalKey = LocalizeKey._16004; // 인챈트
            labelLock.Text = LocalizeKey._16014.ToText()
                .Replace(ReplaceKey.VALUE, BasisType.EQUIPMENT_CARD_SLOT_UNLOCK_LEVEL.GetInt(index + 1)); // [+VALUE강] 달성 시, 카드 슬롯 해금
        }

        protected override void Refresh()
        {
            OnLocalize();
            var card = info.GetCardItem(index);
            if (card == null)
            {
                labelName.LocalKey = LocalizeKey._16005; // 빈 슬롯               
                btnCardInfo.SetActive(false);
                btnEnchant.SetActive(true);
                btnUnEquip.SetActive(false);
                icon.spriteName = "Ui_Common_Icon_Card_None";
                labName_TweenColor.PlayForward();
                SetOptionActive(false);
                return;
            }

            icon.spriteName = card.GetSlotIconName();
            labName_TweenColor.PlayReverse();
            labelName.Text = card.Name;
            btnCardInfo.SetActive(true);
            btnEnchant.SetActive(false);
            btnUnEquip.SetActive(true);
            SetOptionData(card);
        }

        public override void SetActive(bool isActive)
        {
            if (!isActive)
            {
                SetOptionActive(isActive);
            }
            base.SetActive(isActive);
        }

        public void SetOpenSlot(bool isOpen)
        {
            if (!isOpen)
            {
                SetOptionActive(isOpen);
            }
            cardLockInfo.SetActive(!isOpen);
            cardUnLockInfo.SetActive(isOpen);
        }

        void SetOptionActive(bool isActive)
        {
            for (int i = 0; i < battleOptions.Length; i++)
            {
                battleOptions[i].SetActive(isActive);
            }
        }

        void SetOptionData(IEnumerable<BattleOption> collection)
        {
            IEnumerator<BattleOption> enumerator = collection.GetEnumerator();

            foreach (var label in battleOptions)
            {
                bool moveNext = enumerator.MoveNext();
                label.SetActive(moveNext);

                if (moveNext)
                {
                    BattleOption option = enumerator.Current;

                    label.Title = option.GetTitleText();
                    label.Value = option.GetValueText();                    
                }
            }
        }

        void OnClickedBtnCardInfo()
        {
            UI.Show<UICardInfo>(new UICardInfo.Input(info.GetCardItem(index), info, index));
        }

        void OnClickedBtnEnchant()
        {
            UI.Show<UICardInven>(new UICardInvenData(info, index));
            //OnSelectEquip?.Invoke(index);
        }

        void ClickedBtnUnEquip()
        {
            OnSelectUnEquip?.Invoke(index);
        }
    }
}
