using UnityEngine;

namespace Ragnarok
{
    public class UIButtonWithIconValue : UIButtonHelper
    {
        [SerializeField] UISprite icon;
        [SerializeField] UILabelHelper labelValue;

        public void SetActiveIcon(bool isActive)
        {
            icon.cachedGameObject.SetActive(isActive);
        }

        public void SetIconName(string iconName)
        {
            icon.spriteName = iconName;
        }

        public void SetLabelValue(string cost)
        {
            labelValue.Text = cost;
        }
    }
}