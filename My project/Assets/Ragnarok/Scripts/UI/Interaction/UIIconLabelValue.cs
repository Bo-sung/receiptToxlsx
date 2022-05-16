using UnityEngine;

namespace Ragnarok
{
    public class UIIconLabelValue : UILabelValue
    {
        [SerializeField] UITextureHelper icon;

        public void SetActiveIcon(bool isActive)
        {
            icon.SetActive(isActive);
        }

        public void SetIconName(string iconName)
        {
            icon.Set(iconName, isAsync: false);
        }
    }
}