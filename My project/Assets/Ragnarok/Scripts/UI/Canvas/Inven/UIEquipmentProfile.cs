using UnityEngine;

namespace Ragnarok
{
    public class UIEquipmentProfile : UIInfo<ItemInfo>
    {
        [SerializeField] UISprite itemProfile;
        [SerializeField] UITextureHelper icon;
        [SerializeField] GameObject equipRibbon;
        [SerializeField] UILabelHelper labelEquip;
        [SerializeField] UIGridHelper rate;
        [SerializeField] UILabelHelper labelSmelt;
        [SerializeField] UISprite[] cards;
        [SerializeField] bool tireType2;
        [SerializeField] GameObject lockIcon;
        [SerializeField] GameObject lockIconOff;
        [SerializeField] GameObject iconPowerUp; // 전투력 비교 화살표
        [SerializeField] UISprite sprLock;
        [SerializeField] GameObject cardView;
        [SerializeField] UILabelHelper tierupRate;
        [SerializeField] GameObject goWarning;
        [SerializeField] UILabelHelper labelWarning;

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

            itemProfile.spriteName = info.GetBackSpriteName(tireType2, tierupRate != null);
            icon.Set(info.IconName);

            if (equipRibbon)
                equipRibbon.SetActive(info.IsEquipped);

            if (tierupRate)
                tierupRate.gameObject.SetActive(false);

            if (tierupRate != null && info.ItemTranscend > 0)
            {
                rate.SetValue(0);
                tierupRate.gameObject.SetActive(true);
                tierupRate.Text = info.ItemTranscend.ToString();
            }
            else
            {
                rate.SetValue(info.Rating);
            }

            labelSmelt.Text = info.Smelt > 0 ? $"+{info.Smelt}" : string.Empty;
            if (cards.Length != 0)
            {
                int maxSlot = info.GetMaxCardSlot();
                for (int i = 0; i < Constants.Size.MAX_EQUIPPED_CARD_COUNT; i++)
                {
                    if (i < maxSlot)
                    {
                        cards[i].enabled = true;
                        var cardInfo = info.GetCardItem(i);
                        if (cardInfo is null || !info.IsOpenCardSlot(i))
                        {
                            cards[i].spriteName = Constants.CommonAtlas.UI_COMMON_ICON_CARD_NONE;
                        }
                        else
                        {
                            cards[i].spriteName = cardInfo.GetSlotIconName();
                        }
                    }
                    else
                    {
                        cards[i].enabled = false;
                    }
                }
            }

            if (lockIcon)
                lockIcon.SetActive(info.IsLock);
            if (lockIconOff)
                lockIconOff.SetActive(!info.IsLock);

            if (sprLock)
                sprLock.color = info.GetLockBackColor(tierupRate != null);

            // 제한 관련
            string equipWarningMessage = info.GetEquiqWarningMessage(isPopupMessage: false);
            NGUITools.SetActive(goWarning, !string.IsNullOrEmpty(equipWarningMessage));

            // 제한 메시지
            if (labelWarning)
                labelWarning.Text = equipWarningMessage;
        }

        public void SetActiveCardView(bool value)
        {
            if (cardView != null)
                cardView.SetActive(value);
        }

        public void SetPowerUpIcon(bool isActive)
        {
            NGUITools.SetActive(iconPowerUp, isActive);
        }
    }
}