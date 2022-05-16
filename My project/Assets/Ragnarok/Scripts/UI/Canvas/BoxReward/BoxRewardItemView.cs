using UnityEngine;

namespace Ragnarok.View
{
    public class BoxRewardItemView : UIView
    {
        public interface IInput
        {
            string IconName { get; }
            string Name { get; }
            string Description { get; }
        }

        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelDescription;

        protected override void OnLocalize()
        {
        }

        public void Set(IInput info)
        {
            icon.SetItem(info.IconName);
            labelName.Text = info.Name;
            labelDescription.Text = info.Description;
        }
    }
}