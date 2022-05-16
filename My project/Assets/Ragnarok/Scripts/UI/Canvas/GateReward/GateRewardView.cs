using UnityEngine;

namespace Ragnarok.View
{
    public sealed class GateRewardView : PopupView
    {
        [SerializeField] GateRewardSlot[] slots;
        [SerializeField] UILabelHelper labelNotice;

        protected override void OnLocalize()
        {
            base.OnLocalize();

            labelNotice.LocalKey = LocalizeKey._6950; // 파티 승리 시, 파티원에게 같은 보상이 지급됩니다.\n(죽은 유저의 보상 불이익이 없습니다.)
        }

        public void SetData(RewardData[][] rewards)
        {
            int length = rewards == null ? 0 : rewards.Length;
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].SetData(i < length ? rewards[i] : null);
            }
        }
    }
}