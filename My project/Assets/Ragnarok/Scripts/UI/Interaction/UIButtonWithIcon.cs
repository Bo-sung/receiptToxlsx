using UnityEngine;

namespace Ragnarok
{
    public class UIButtonWithIcon : UIButtonHelper
    {
        [SerializeField] UISprite icon;

        public void SetActiveIcon(bool isActive)
        {
            NGUITools.SetActive(icon.cachedGameObject, isActive);
        }

        public void SetIconName(string iconName)
        {
            icon.spriteName = iconName;
        }

        public UISprite GetIcon()
        {
            return icon;
        }
    }
}