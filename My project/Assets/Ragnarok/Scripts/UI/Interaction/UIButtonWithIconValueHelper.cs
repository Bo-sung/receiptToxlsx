using UnityEngine;

namespace Ragnarok
{
    public class UIButtonWithIconValueHelper : UIButtonHelper
    {
        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabelHelper labelValue;

        public void SetActiveIcon(bool isActive)
        {
            icon.SetActive(isActive);
        }

        public void SetIconName(string iconName)
        {
            icon.Set(iconName);
        }

        public void SetValue(string valueText)
        {
            labelValue.Text = valueText;
        }

        public void SetActiveValue(bool isActive)
        {
            labelValue.SetActive(isActive);
        }
    }
}