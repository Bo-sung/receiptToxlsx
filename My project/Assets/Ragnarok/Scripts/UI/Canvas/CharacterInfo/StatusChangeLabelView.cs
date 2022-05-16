using UnityEngine;

namespace Ragnarok
{
    public class StatusChangeLabelView : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelValue;
        [SerializeField] UILabelHelper labelSign; // ▲ ▼

        public UILabelHelper Name => labelName;
        public UILabelHelper Value => labelValue;
        public UILabelHelper Sign => labelSign;

        GameObject cachedGameObject;

        /*
        Color32 increaseColor = new Color32(0xA8, 0xED, 0xFF, 255); // 플러스
        Color32 decreaseColor = new Color32(0xEC, 0xEC, 0xEC, 255); // 마이너스
        */

        public void Initialize()
        {
            cachedGameObject = cachedGameObject ?? gameObject;
        }

        public void SetActive(bool isActive)
        {
            cachedGameObject.SetActive(isActive);
        }

        public void SetSignType(bool isIncrease)
        {
            if (!cachedGameObject.activeSelf)
                return;

            if (isIncrease)
            {
                labelSign.Text = "[FFB7C2]▲";
            }
            else
            {
                labelSign.Text = "[ECECEC]▼";
            }
        }

        public void SetData(string name, int value, bool isPercent = false)
        {
            if (!cachedGameObject.activeSelf)
                return;

            labelName.Text = name;
            if (isPercent)
            {
                labelValue.Text = $"{MathUtils.ToPercentValue(value).ToString("F1")}";
            }
            else
            {
                labelValue.Text = value.ToString();
            }
        }
    }
}