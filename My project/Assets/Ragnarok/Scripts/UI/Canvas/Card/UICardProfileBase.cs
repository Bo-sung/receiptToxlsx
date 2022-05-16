using UnityEngine;

namespace Ragnarok
{
    public class UICardProfileBase : UIInfo<ItemInfo>
    {
        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabelHelper labelLevel;

        protected override void Refresh()
        {
            if (IsInvalid())
            {
                SetActive(false);
                return;
            }
            SetActive(true);

            icon.Set(info.IconName);
            labelLevel.Text = LocalizeKey._18005.ToText(). // Lv. {LEVEL}
                    Replace("{LEVEL}", info.GetCardLevelView());
        }
    }
}