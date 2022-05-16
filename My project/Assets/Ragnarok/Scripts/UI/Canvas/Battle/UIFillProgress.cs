using System.Globalization;
using UnityEngine;

namespace Ragnarok.View
{
    public class UIFillProgress : MonoBehaviour
    {
        static NumberFormatInfo percentageFormat;

        [SerializeField] UISprite sprite;
        [SerializeField] UILabel labelText;

        private void Awake()
        {
            if (percentageFormat is null)
                percentageFormat = new NumberFormatInfo { PercentPositivePattern = 1, PercentNegativePattern = 1 };
        }

        public void Set(int cur, int max)
        {
            float progress = MathUtils.GetProgress(cur, max);

            sprite.fillAmount = progress;

            if (labelText)
            {
                labelText.text = progress.ToString("P0", percentageFormat);
            }
        }

    }
}