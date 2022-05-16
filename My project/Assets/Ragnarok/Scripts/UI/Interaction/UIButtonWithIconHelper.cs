using UnityEngine;

namespace Ragnarok
{
    public class UIButtonWithIconHelper : UIButtonHelper
    {
        [SerializeField] UITextureHelper icon;

        public void SetActiveIcon(bool isActive)
        {
            icon.SetActive(isActive);
        }

        public void SetIconName(string iconName)
        {
            icon.Set(iconName);
        }

        public void SetMode(UIGraySprite.SpriteMode mode)
        {
            icon.Mode = mode;
        }

        public UITextureHelper GetIcon()
        {
            return icon;
        }
    }
}