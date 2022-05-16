using UnityEngine;

namespace Ragnarok
{
    public class UIPartsProfile : UIInfo<InvenPresenter, ItemInfo>
    {
        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabelHelper labelCount;
        [SerializeField] GameObject iconNotice;
        [SerializeField] UISprite iconElement;
        [SerializeField] UILabelHelper labelElementLevel;

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

            if (iconElement)
            {
                iconElement.cachedGameObject.SetActive(info.IsElementStone);
                if (info.IsElementStone)
                {
                    labelElementLevel.Text = info.GetElementLevelText();
                }
            }
        }
    }
}