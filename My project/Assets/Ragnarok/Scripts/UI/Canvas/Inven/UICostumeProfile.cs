using UnityEngine;

namespace Ragnarok.View
{
    public class UICostumeProfile : UIView
    {
        [SerializeField] UITextureHelper icon;
        [SerializeField] GameObject equipRibbon;
        [SerializeField] UILabelHelper labelEquip;
        [SerializeField] GameObject iconNotice;
        [SerializeField] UISprite classType;

        protected override void OnLocalize()
        {
            labelEquip.LocalKey = LocalizeKey._6011; // E
        }

        public void Set(ItemInfo info)
        {
            icon.Set(info.IconName);
            equipRibbon.SetActive(info.IsEquipped);
            if (classType)
                classType.spriteName = info.CostumeType.GetIconName();
            if (iconNotice)
                iconNotice.SetActive(info.IsNew);
        }
    }
}