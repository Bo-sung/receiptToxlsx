using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class UIFriendInviteRewardSlot : UIView
    {
        [SerializeField] UILabelHelper labelMaxCount;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] GameObject complete;

        private int completeCount;

        protected override void OnLocalize()
        {
            
        }

        public void SetData(RewardGroupData data)
        {
            completeCount = data.condition_value;

            labelMaxCount.Text = LocalizeKey._33075.ToText() // {COUNT}명
                .Replace(ReplaceKey.COUNT, completeCount);

            rewardHelper.SetData(new RewardData(data.goods_type, data.goods_value, data.goods_count));
        }

        public void SetComplete(int cnt)
        {
            complete.SetActive(cnt >= completeCount);
        }
    }
}