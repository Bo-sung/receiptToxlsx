using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIRewardItem : UIView
    {
        public interface IInput
        {
            string GetItemIcon();
            int GetItemCount();
        }

        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabel labelCount;

        string itemIcon;
        int itemCount;

        public void SetData(IInput input)
        {
            SetData(input.GetItemIcon(), input.GetItemCount());
        }

        public void SetData(string itemIcon, int itemCount)
        {
            this.itemIcon = itemIcon;
            this.itemCount = itemCount;

            icon.Set(itemIcon);
            labelCount.text = itemCount.ToString();
        }

        public void Launch(UIRewardLauncher.GoodsDestination destination)
        {
            UI.LaunchReward(transform.position, itemIcon, itemCount, destination);
        }

        protected override void OnLocalize()
        {
        }
    }
}