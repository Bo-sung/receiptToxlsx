using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIGuideElement : UIElement<UIGuideElement.IInput>
    {
        public interface IInput
        {
            string IconName { get; }
            string Name { get; }
            string Title { get; }
            string Description { get; }
            int Index { get; }
        }

        [SerializeField] UILabelHelper labelIndex;
        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelValue description;

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            labelIndex.Text = info.Index.ToString();
            icon.SetContentsUnlock(info.IconName);
            labelName.Text = info.Name;
            description.Title = info.Title;
            description.Value = info.Description;
        }
    }
}