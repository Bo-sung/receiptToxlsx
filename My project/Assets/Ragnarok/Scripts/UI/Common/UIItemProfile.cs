using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// [아이템] 공통으로 사용하는 아이템 정보 표시
    /// </summary>
    public class UIItemProfile : UIInfo<ItemInfo>
    {
        [SerializeField] UISprite background;
        [SerializeField] UITextureHelper icon;
        [SerializeField] GameObject equipRibbon;
        [SerializeField] UILabelHelper labelEquip;
        [SerializeField] UIGridHelper rate;
        [SerializeField] UILabelHelper labelSmelt;
        [SerializeField] UISprite[] cards;
        [SerializeField] GameObject lockIcon;

        protected override void OnLocalize()
        {
            base.OnLocalize();

            if (labelEquip)
                labelEquip.LocalKey = LocalizeKey._6011; // E
        }


        protected override void Refresh()
        {
            if (IsInvalid())
            {
                SetActive(false);
                return;
            }

            SetActive(true);

            switch (info.ItemGroupType)
            {
                case ItemGroupType.Equipment:
                    SetEquipment();
                    break;
                case ItemGroupType.ConsumableItem:
                case ItemGroupType.Card:
                case ItemGroupType.ProductParts:
                case ItemGroupType.MonsterPiece:
                    SetDefault();
                    break;
            }
        }

        private void SetEquipment()
        {
            icon.Set(info.IconName);
            background.spriteName = info.GetBackSpriteName(false);

            if (equipRibbon)
                equipRibbon.SetActive(info.IsEquipped);

            if (rate)
                rate.SetValue(info.Rating);

            if (labelSmelt)
                labelSmelt.Text = info.Smelt > 0 ? $"+{info.Smelt}" : string.Empty;

            if (cards.Length > 0)
            {
                for (int i = 0; i < Constants.Size.MAX_EQUIPPED_CARD_COUNT; i++)
                {
                    var cardInfo = info.GetCardItem(i);
                    if (!info.IsOpenCardSlot(i))
                    {
                        cards[i].enabled = false;
                        continue;
                    }

                    cards[i].enabled = true;
                    if (cardInfo == null)
                    {
                        cards[i].spriteName = "Ui_Common_Icon_Card_None";
                    }
                    else
                    {
                        cards[i].spriteName = cardInfo.GetSlotIconName();
                    }
                }
            }

            if (lockIcon)
                lockIcon.SetActive(info.IsLock);
        }

        private void SetDefault()
        {
            icon.Set(info.IconName);
            background.spriteName = "Ui_Common_BG_Item_Default";

            if (equipRibbon)
                equipRibbon.SetActive(false);

            if (rate)
                rate.SetActive(false);

            if (labelSmelt)
                labelSmelt.Text = string.Empty;

            if (cards.Length > 0)
            {
                for (int i = 0; i < Constants.Size.MAX_EQUIPPED_CARD_COUNT; i++)
                {
                    cards[i].enabled = false;
                }
            }

            if (lockIcon)
                lockIcon.SetActive(false);
        }
    }
}