using UnityEngine;

namespace Ragnarok.View
{
    public class BoxRewardSlot : UIView
    {
        public interface IInput
        {
            string ItemName { get; }
            string ItemCount { get; }
            bool IsFixed { get; }
            int Rate { get; }
        }

        [SerializeField] UILabelHelper labelItemName;
        [SerializeField] UILabelHelper labelCount;
        [SerializeField] UILabelHelper labelRate;

        protected override void OnLocalize()
        {
        }

        public void Set(IInput input, int totalRate)
        {
            labelItemName.Text = input.ItemName;
            labelCount.Text = input.ItemCount;

            if (input.IsFixed)
            {
                labelRate.LocalKey = LocalizeKey._8303; // 확정
            }
            else
            {
                labelRate.Text = (input.Rate / (float)totalRate).ToString("0.####%");
            }
        }
    }
}