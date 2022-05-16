using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 재화 포함된 버튼
    /// </summary>
    public class UICostButtonHelper : UIButtonHelper
    {
        [SerializeField] UILabelHelper labTitle;
        [SerializeField] UISprite sprCost;
        [SerializeField] UILabelHelper labCost;
        [SerializeField] UISprite sprCostBg;
        [SerializeField] string noramlSprite = "Ui_Common_BG_Cost_Point";
        [SerializeField] string disabledSprite = "Ui_Common_BG_Cost_None";
        [SerializeField] Color normalCostLabelColor = new Color32(128, 192, 255, 255);
        [SerializeField] Color disabledCostLabelColor = new Color32(180, 180, 180, 255);

        public override bool IsEnabled
        {
            get => base.IsEnabled;
            set
            {
                base.IsEnabled = value;

                if (labTitle)
                    labTitle.Outline = value ? normal : disabled;

                if (labCost)
                    labCost.Outline = value ? normal : disabled;

                if (sprCostBg)
                    sprCostBg.spriteName = value ? noramlSprite : disabledSprite;
            }
        }

        public string TitleText
        {
            set
            {
                if (labTitle)
                    labTitle.Text = value;
            }
        }

        public int TitleLocalKey
        {
            set
            {
                TitleText = value.ToText();
            }
        }

        public string CostIcon
        {
            set
            {
                if (sprCost)
                {
                    sprCost.enabled = true;
                    sprCost.spriteName = value;
                }
            }
        }

        public string CostText
        {
            set
            {
                if (labCost)
                {
                    labCost.SetActive(true);
                    labCost.Text = value;
                }
            }
        }

        public void SetCostCount(string value)
        {
            if (labCost)
                labCost.Text = value;
        }

        public void SetCostCount(int value)
        {
            if (labCost)
                labCost.Text = $"{value:N0}";
        }

        public void CostView(bool isShowCost)
        {
            if (sprCost)
                sprCost.enabled = isShowCost;

            if (labCost)
                labCost.SetActive(isShowCost);
        }

        public void SetCostColor(bool isEnable)
        {
            SetCostColor(isEnable ? normalCostLabelColor : disabledCostLabelColor);
        }

        public void SetCostColor(Color color)
        {
            if (labCost)
                labCost.Color = color;
        }

        public override bool Find()
        {
            base.Find();

            if (labCost)
                normal = labCost.Color;
            return true;
        }
    }
}