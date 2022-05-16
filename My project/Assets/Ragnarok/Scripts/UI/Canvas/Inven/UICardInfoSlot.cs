using System;
using UnityEngine;

namespace Ragnarok
{
    public class UICardInfoSlot : UIInfo<InvenPresenter, ItemInfo>
    {
        [SerializeField] UICardProfile cardProfile;
        [SerializeField] UIButtonHelper btnShowInfo;
        [SerializeField] GameObject goWarning;
        [SerializeField] UILabelHelper labelWarning;

        private ItemInfo equipmentItemInfo;
        private byte index;
        private bool isEquipLimit;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnShowInfo.OnClick, OnClickedBtnShowInfo);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnShowInfo.OnClick, OnClickedBtnShowInfo);
        }

        protected override void OnLocalize()
        {
            if (labelWarning != null)
                labelWarning.LocalKey = LocalizeKey._17001; // 장착 제한
        }

        public override void SetData(ItemInfo info)
        {
            base.SetData(info);            
        }

        public void SetSubData(ItemInfo equipmentItemInfo, byte index)
        {
            this.equipmentItemInfo = equipmentItemInfo;
            this.index = index;
        }

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            cardProfile.SetData(presenter, info);

            // 쉐도우중 5성는 1개만 장착 가능하다
            isEquipLimit = false;
            if (equipmentItemInfo != null && info.IsShadow && info.Rating == 5)
            {
                for (int i = 0; i < equipmentItemInfo.GetMaxCardSlot(); i++)
                {
                    ItemInfo card = equipmentItemInfo.GetCardItem(i);
                    if (card != null)
                    {
                        if (card.Rating == 5)
                        {
                            isEquipLimit = true;
                            break;
                        }
                    }
                }
            }

            if (goWarning != null)
                goWarning.SetActive(isEquipLimit);
        }

        /// <summary>
        /// 아이템 정보 보기 버튼 클릭
        /// </summary>
        void OnClickedBtnShowInfo()
        {
            if (presenter != null && presenter.IsDisassembleMode)
            {
                presenter.SelectDisassemble(info);
                Refresh();
            }
            else
            {
                if (isEquipLimit)
                {
                    UI.ShowToastPopup(LocalizeKey._90288.ToText()); // 쉐도우 장비 하나에 랭크5 쉐도우 카드 하나만 장착할 수 있습니다.
                    return;
                }

                UI.Show<UICardInfo>(new UICardInfo.Input(info, equipmentItemInfo, index));
                if (info.IsNew && presenter != null)
                {
                    presenter.HideNew(info);
                    cardProfile.SetData(info);
                }
            }
        }
    }
}
