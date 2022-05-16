using UnityEngine;

namespace Ragnarok
{
    public sealed class UIConsumableProfile : UIInfo<ItemInfo>
    {
        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabelHelper labelCount;
        [SerializeField] GameObject iconNotice;

        [Rename(displayName = "1개일 때도 개수 표시")]
        [SerializeField] bool showSingleCount;

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            icon.Set(info.IconName);

            if (showSingleCount || info.ItemCount > 1)
                labelCount.Text = info.ItemCount.ToString();
            else
                labelCount.Text = string.Empty;

            if (iconNotice)
                iconNotice.SetActive(info.IsNew);
        }
    }
}