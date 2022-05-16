using System;

using UnityEngine;

namespace Ragnarok
{
    public class BingoLineRewardSlot : MonoBehaviour
    {
        [SerializeField] UIRewardHelper slot;
        [SerializeField] UIButtonHelper button;
        [SerializeField] GameObject rewardedMark;
        [SerializeField] GameObject rewardableMark;
        [SerializeField] GameObject inRewardableLineMark;

        private BingoStateDecoratedData data;
        private Action<BingoStateDecoratedData> onClickSlot;

        private void Start()
        {
            EventDelegate.Add(button.OnClick, OnClickButton);
        }

        public void SetData(BingoStateDecoratedData data, Action<BingoStateDecoratedData> onClickSlot)
        {
            this.data = data;
            this.onClickSlot = onClickSlot;

            slot.SetData(data.bingoData.GetReward());
            slot.UseDefaultButtonEvent = data.state != BingoStateDecoratedData.State.Rewardable;

            rewardedMark.SetActive(data.state == BingoStateDecoratedData.State.Rewarded);
            rewardableMark.SetActive(data.state == BingoStateDecoratedData.State.Rewardable);
            inRewardableLineMark.SetActive(data.isInRewardableLine);
        }

        public void Refresh()
        {
            slot.UseDefaultButtonEvent = data.state != BingoStateDecoratedData.State.Rewardable;
            rewardedMark.SetActive(data.state == BingoStateDecoratedData.State.Rewarded);
            rewardableMark.SetActive(data.state == BingoStateDecoratedData.State.Rewardable);
            inRewardableLineMark.SetActive(data.isInRewardableLine);
        }

        private void OnClickButton()
        {
            if (data.state == BingoStateDecoratedData.State.Rewardable)
            {
                onClickSlot?.Invoke(data);
            }
        }
    }
}