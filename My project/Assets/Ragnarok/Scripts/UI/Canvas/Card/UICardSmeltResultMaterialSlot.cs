using UnityEngine;

namespace Ragnarok.View
{
    public class UICardSmeltResultMaterialSlot : UIView, IAutoInspectorFinder
    {
        public class Info
        {
            public readonly string iconName;
            public readonly int count;

            public Info(string iconName, int count)
            {
                this.iconName = iconName;
                this.count = count;
            }
        }

        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabelHelper labelCount;

        protected override void OnLocalize()
        {
        }

        public void Set(Info info)
        {
            icon.Set(info.iconName);
            labelCount.Text = info.count.ToString();
        }
    }
}