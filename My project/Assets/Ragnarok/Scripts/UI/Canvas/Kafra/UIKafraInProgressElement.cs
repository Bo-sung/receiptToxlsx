using UnityEngine;

namespace Ragnarok.View
{
    public class UIKafraInProgressElement : UIElement<UIKafraInProgressElement.IInput>
    {
        public interface IInput
        {
            RewardData Result { get; }
            RewardData Material { get; }
            int Count { get; }
            int GetRewardCount();
        }

        [SerializeField] UIMaterialRewardHelper material;
        [SerializeField] UILabelHelper labelMaterialName;
        [SerializeField] UILabelHelper labelAmount;
        [SerializeField] UIRewardHelper result;

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            material.SetData(info.Material);
            labelMaterialName.Text = info.Material.ItemName;
            result.SetData(info.Result);
            material.SetAmount(info.Count, min: 0);
            labelAmount.Text = info.Count.ToString("N0");
        }
    }
}