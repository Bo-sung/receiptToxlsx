using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 아이템 재화 포함된 버튼
    /// </summary>
    public class UIItemCostButtonHelper : UIButtonHelper
    {
        [SerializeField] UITextureHelper itemIcon;
        [SerializeField] UILabelHelper labelItemCount;
        [SerializeField] UISprite itemCountBg;
        [SerializeField] string noramlSprite = "Ui_Common_BG_Cost_Point";
        [SerializeField] string disabledSprite = "Ui_Common_BG_Cost_None";
        [SerializeField] Color normalCostLabelColor = new Color32(86, 86, 86, 255);
        [SerializeField] Color disabledCostLabelColor = new Color32(128, 0, 0, 255);

        public override bool IsEnabled
        {
            get => base.IsEnabled;
            set
            {
                base.IsEnabled = value;

                if (labelItemCount)
                    labelItemCount.Outline = value ? normal : disabled;

                if (itemCountBg)
                    itemCountBg.spriteName = value ? noramlSprite : disabledSprite;
            }
        }

        public void SetItemIcon(string icon)
        {
            if (itemIcon)
            {
                itemIcon.SetItem(icon, isAsync: false);
            }
        }

        public void SetItemCount(int value)
        {
            SetItemCount(value.ToString("N0"));
            SetCountColor(value > 0);
        }

        public void SetItemCount(string value)
        {
            if (labelItemCount)
                labelItemCount.Text = value;
        }

        public void SetCountColor(bool isEnable)
        {
            SetCountColor(isEnable ? normalCostLabelColor : disabledCostLabelColor);
        }

        public void SetCountColor(Color color)
        {
            if (labelItemCount)
                labelItemCount.Color = color;
        }

        public override bool Find()
        {
            base.Find();

            if (labelItemCount)
                normal = labelItemCount.Color;

            return true;
        }
    }
}