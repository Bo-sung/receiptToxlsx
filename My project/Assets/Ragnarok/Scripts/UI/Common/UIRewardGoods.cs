using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIRewardGoods : UIView
    {
        [SerializeField] UISprite icon;
        [SerializeField] UILabel labelValue;

        private RewardType rewardType;
        private int value;

        public void SetValue(RewardType rewardType, int value)
        {
            this.rewardType = rewardType;
            this.value = value;

            icon.spriteName = rewardType.IconName();
            labelValue.text = value.ToString("N0");
        }

        protected override void OnLocalize()
        {
        }

        public void Launch()
        {
            UI.LaunchReward(transform.position, rewardType, 100, UIRewardLauncher.GoodsDestination.Basic);
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.J))
                Launch();
        }
    }
}