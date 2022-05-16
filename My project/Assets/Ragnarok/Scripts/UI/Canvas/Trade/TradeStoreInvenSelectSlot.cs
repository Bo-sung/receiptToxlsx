using UnityEngine;

namespace Ragnarok
{
    public class TradeStoreInvenSelectSlot : UIInfo<TradeStoreInvenSelectPresenter, ItemInfo>
    {
        [SerializeField] UICardProfile cardProfile;
        [SerializeField] UIEquipmentProfile equipmentProfile;
        [SerializeField] UIPartsProfile partsProfile;
        [SerializeField] UIButtonHelper btnIcon;
        [SerializeField] GameObject lockIcon;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnIcon.OnClick, OnClickBtnIcon);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnIcon.OnClick, OnClickBtnIcon);
        }

        protected override void Refresh()
        {
            if (info == null)
                return;

            SetActive(cardProfile.gameObject, info.ItemGroupType == ItemGroupType.Card);
            SetActive(equipmentProfile.gameObject, info.ItemGroupType == ItemGroupType.Equipment);
            SetActive(partsProfile.gameObject, info.ItemGroupType == ItemGroupType.ProductParts);

            switch (info.ItemGroupType)
            {
                case ItemGroupType.Card:
                    if (info.IsShadow)
                    {
                        if (!info.CanTrade)
                        {
                            lockIcon.SetActive(true); 
                        }
                        else
                        {
                            lockIcon.SetActive(info.CardLevel > 1);
                        }
                    }
                    else
                    {
                        lockIcon.SetActive(!info.CanTrade);
                    }
                    cardProfile.SetData(info as CardItemInfo);
                    break;

                case ItemGroupType.Equipment:
                    lockIcon.SetActive(!info.CanTrade);
                    equipmentProfile.SetData(info as EquipmentItemInfo);
                    break;

                case ItemGroupType.ProductParts:
                    lockIcon.SetActive(!info.CanTrade);
                    partsProfile.SetData(info as PartsItemInfo);
                    break;
            }
        }

        void OnClickBtnIcon()
        {
            presenter.SelectItem(info);
        }

        void SetActive(GameObject go, bool isActive)
        {
            if (go.activeSelf != isActive)
                go.SetActive(isActive);
        }
    }
}