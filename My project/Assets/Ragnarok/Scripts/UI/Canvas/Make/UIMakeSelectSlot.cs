using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIMakeSelectSlot : UIInfo<MakeSelectPartPresenter, ItemInfo>
    {
        [SerializeField] UIButtonHelper button;
        [SerializeField] UIEquipmentProfile itemBase;
        [SerializeField] UIButtonHelper btnInfoMode;
        [SerializeField] GameObject select;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(button.OnClick, OnClickedButton);
            EventDelegate.Add(btnInfoMode.OnClick, OnClickedBtnInfoMode);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(button.OnClick, OnClickedButton);
            EventDelegate.Remove(btnInfoMode.OnClick, OnClickedBtnInfoMode);
        }

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            itemBase.SetData(info);
            btnInfoMode.SetActive(presenter.IsInfoMode);
            if (presenter.IsSelect(info))
            {
                if (info.IsLock || info.IsEquipped)
                {
                    presenter.SelectItemInfo(info);
                }
                else
                {
                    bool isCard = false;
                    for (int i = 0; i < Constants.Size.MAX_EQUIPPED_CARD_COUNT; ++i)
                    {
                        ItemInfo card = info.GetCardItem(i);
                        if (card != null)
                        {
                            isCard = true;
                            break;                            
                        }
                    }

                    // 카드가 장착되어 있는 아이템은 선택을 제거
                    if (isCard)
                    {
                        presenter.SelectItemInfo(info);
                    }
                    else
                    {
                        select.SetActive(!presenter.IsInfoMode);
                    }                   
                }
            }
            else
            {
                select.SetActive(false);
            }
        }

        async void OnClickedButton()
        {
            for (int i = 0; i < Constants.Size.MAX_EQUIPPED_CARD_COUNT; ++i)
            {
                ItemInfo card = info.GetCardItem(i);
                if (card != null)
                {
                    UI.ShowToastPopup(LocalizeKey._90196.ToText()); // 카드가 장착된 장비는 등록할 수 없습니다.
                    return;
                }             
            }
           
            if(info.IsEquipped || info.IsLock)
            {
                string description = LocalizeKey._90088.ToText(); // 장착, 잠금 해제 후 재료로 사용하시겠습니까?
                if (!await UI.SelectPopup(description))
                    return;

                await presenter.UnEquip(info);
            }
            presenter.SelectItemInfo(info);  
        }

        void OnClickedBtnInfoMode()
        {
            UI.Show<UIEquipmentInfo>().Set(info.ItemNo);
        }
    }
}
