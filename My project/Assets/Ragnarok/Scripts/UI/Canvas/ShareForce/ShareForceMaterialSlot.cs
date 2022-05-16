using UnityEngine;

namespace Ragnarok.View
{
    public class ShareForceMaterialSlot : UIView
    {
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UILabelHelper labelCount;

        protected override void OnLocalize()
        {
        }

        public void Set(RewardData data, int ownedCount)
        {
            if(data == null || data.RewardType == RewardType.None)
            {
                SetActive(false);
                return;
            }

            SetActive(true);

            rewardHelper.SetData(data);

            int needCount = data.Count;
            int curCount = ownedCount;

            bool hasMaterial = curCount >= needCount;
            if (hasMaterial)
            {
                labelCount.Text = $"[4C4A4D]{curCount}/{needCount}[-]";
            }
            else
            {
                labelCount.Text = $"[D76251]{curCount}[-][4C4A4D]/{needCount}[-]";
            }
        }
    }
}