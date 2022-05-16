using UnityEngine;

namespace Ragnarok.View
{
    public class EveryDayRewardSlot : UIView
    {
        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabelHelper labelCount;

        protected override void OnLocalize()
        {            
        }

        public void Set(string iconName, int count)
        {
            icon.Set(iconName);
            labelCount.Text = count.ToString("N0");
        }
    }
}