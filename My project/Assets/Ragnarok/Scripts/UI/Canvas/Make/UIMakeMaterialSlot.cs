using UnityEngine;

namespace Ragnarok
{
    public class UIMakeMaterialSlot : UIInfo<MakePresenter, MaterialInfo>
    {
        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabelHelper labelMaterialCount;
        [SerializeField] GameObject selectMode;
        [SerializeField] UIButtonHelper button;
        [SerializeField] GameObject notice;
        [SerializeField] UISprite iconElement;
        [SerializeField] UILabelHelper labelElementLevel;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(button.OnClick, OnClickedButton);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(button.OnClick, OnClickedButton);
        }

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            icon.Set(info.IconName);
            int needCount = info.Count * presenter.MakeCount;
            int curCount = GetItemCount();

            bool hasMaterial = curCount >= needCount;
            if (hasMaterial)
            {
                labelMaterialCount.Text = $"[4C4A4D]{curCount}/{needCount}[-]";
            }
            else
            {
                labelMaterialCount.Text = $"[D76251]{curCount}[-][4C4A4D]/{needCount}[-]";
            }

            if (info.IsStackable)
            {
                selectMode.SetActive(false);
            }
            else
            {
                // 장비류
                selectMode.SetActive(true);
                int itemCount = presenter.GetItemCount(info.ItemId);
                bool hasItem = itemCount >= needCount;
                notice.SetActive(hasItem);
            }

            if (iconElement)
            {
                int elementStoneLevel = info.GetElementStoneLevel();
                bool isElementStone = elementStoneLevel >= 0;
                iconElement.cachedGameObject.SetActive(isElementStone);
                if (isElementStone)
                {
                    labelElementLevel.Text = (elementStoneLevel + 1).ToString(); // 1을 추가하여 보여줌
                }
            }
        }

        private int GetItemCount()
        {
            if (info.IsStackable)
                return presenter.GetItemCount(info.ItemId);

            return presenter.GetSelectItemCount(info.SlotIndex);
        }

        void OnClickedButton()
        {
            if (info.ItemGroupType == ItemGroupType.Equipment)
            {
                // 장비 선택
                presenter.SetSelectMaterialInfo(info);
                UI.Show<UIMakeSelectPart>();
            }
            else
            {
                ItemInfo itemInfo = presenter.CreateItemInfo(info.ItemId);
                if (itemInfo != null)
                    UI.ShowItemInfo(itemInfo);
            }
        }
    }
}