using UnityEngine;

namespace Ragnarok
{
    public class UILabelPreviewValue : UILabelValue, IAutoInspectorFinder
    {
        private enum ViewMode
        {
            Single = 1,
            Compare,
        }

        [SerializeField] UILabelHelper labelBeforeValue;
        [SerializeField] UILabelHelper labelAfterValue;
        [SerializeField] GameObject arrow;

        private ViewMode savedViewMode;

        public void Show(float value, bool showPercentagePostfix = false)
        {
            Show(GetValueText(value, showPercentagePostfix));
        }

        public void Show(string value)
        {
            SetViewMode(ViewMode.Single);
            Value = value;
        }

        public void Show(float beforeValue, float afterValue, bool showPercentagePostfix = false)
        {
            if (beforeValue == afterValue)
            {
                Show(afterValue, showPercentagePostfix);
                return;
            }

            Show(GetValueText(beforeValue, showPercentagePostfix), GetValueText(afterValue, showPercentagePostfix));
        }

        public void Show(string beforeValue, string afterValue)
        {
            if (beforeValue.Equals(afterValue))
            {
                Show(afterValue);
                return;
            }

            SetViewMode(ViewMode.Compare);

            labelAfterValue.Text = afterValue; // Anchor 가 뒤에부터 잡혀있을 수 있기 때문에 After 부터 처리한다
            labelBeforeValue.Text = beforeValue;
        }

        private void SetViewMode(ViewMode viewMode)
        {
            if (savedViewMode == viewMode)
                return;

            savedViewMode = viewMode;

            SetActiveValue(savedViewMode == ViewMode.Single);

            if (labelBeforeValue != null)
                labelBeforeValue.SetActive(savedViewMode == ViewMode.Compare);

            if (labelAfterValue != null)
                labelAfterValue.SetActive(savedViewMode == ViewMode.Compare);

            if (arrow != null)
                arrow.SetActive(savedViewMode == ViewMode.Compare);
        }

        private string GetValueText(float value, bool showPercentagePostfix)
        {
            if (value == 0f)
                return "-";

            if (showPercentagePostfix)
            {
                return StringBuilderPool.Get().Append(value.ToString("N0")).Append("%").Release();
            }

            return value.ToString("F1");
        }
    }
}