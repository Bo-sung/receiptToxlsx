using UnityEngine;

namespace Ragnarok
{
    public class UICardProfile : UIInfo<InvenPresenter, ItemInfo>
    {
        [SerializeField] UISprite itemProfile;
        [SerializeField] UITextureHelper icon;
        [SerializeField] GameObject equipRibbon;
        [SerializeField] UILabelHelper labelEquip;
        [SerializeField] UILabelHelper labelSmelt;
        [SerializeField] GameObject iconNotice;
        [SerializeField] UISprite classType;
        [SerializeField] UIGridHelper gridRate;
        [SerializeField] bool tireType2;


        protected override void OnLocalize()
        {
            base.OnLocalize();

            labelEquip.LocalKey = LocalizeKey._6011; // E
        }

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            itemProfile.spriteName = info.GetBackSpriteName(tireType2, false);
            icon.Set(info.IconName);
            equipRibbon.SetActive(info.IsEquipped);

            if (classType)
                classType.spriteName = info.ClassType.GetIconName(info.ItemDetailType);

            labelSmelt.Text = LocalizeKey._18005.ToText(). // Lv. {LEVEL}
                    Replace(ReplaceKey.LEVEL, info.GetCardLevelView());

            bool isActiveNotice = info.IsNew;
            if (iconNotice != null)
                iconNotice.SetActive(isActiveNotice);
            gridRate.SetValue(info.Rating);
        }
    }
}