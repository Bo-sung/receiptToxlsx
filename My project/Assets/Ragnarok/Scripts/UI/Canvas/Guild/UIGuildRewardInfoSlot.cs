using UnityEngine;

namespace Ragnarok
{
    public class UIGuildRewardInfoSlot : UIInfo<GuildAttendRewardInfo>
    {
        [SerializeField] UIRewardHelper reward;
        [SerializeField] UILabelHelper labelCount;
        [SerializeField] UILabelHelper labelName;

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            reward.SetData(info.reward);
            labelCount.Text = LocalizeKey._33076.ToText()
                .Replace("{COUNT}",info.count.ToString()); // {COUNT}명 이상
            labelName.Text = info.reward.ItemName;
        }
    }
}
