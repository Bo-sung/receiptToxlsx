using System;
using UnityEngine;

namespace Ragnarok
{
    public class BingoCheckSlot : MonoBehaviour
    {
        [SerializeField] UITextureHelper collectSprite;
        [SerializeField] UIRewardHelper rewardSlot;
        [SerializeField] UIButtonHelper button;
        [SerializeField] GameObject unCheckedMark;
        [SerializeField] GameObject checkableMark;
        [SerializeField] GameObject checkedMark;
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

            collectSprite.Set(data.bingoData.GetCollectItemData().IconName);
            rewardSlot.SetData(data.bingoData.GetReward());
            rewardSlot.UseDefaultButtonEvent = data.state != BingoStateDecoratedData.State.Checkable;

            unCheckedMark.SetActive(data.state == BingoStateDecoratedData.State.Normal || data.state == BingoStateDecoratedData.State.Checkable);
            checkableMark.SetActive(data.state == BingoStateDecoratedData.State.Checkable);
            checkedMark.SetActive(data.state == BingoStateDecoratedData.State.Checked);
            inRewardableLineMark.SetActive(data.isInRewardableLine);
        }

        public void Refresh()
        {
            rewardSlot.UseDefaultButtonEvent = data.state != BingoStateDecoratedData.State.Checkable;
            unCheckedMark.SetActive(data.state == BingoStateDecoratedData.State.Normal || data.state == BingoStateDecoratedData.State.Checkable);
            checkableMark.SetActive(data.state == BingoStateDecoratedData.State.Checkable);
            checkedMark.SetActive(data.state == BingoStateDecoratedData.State.Checked);
            inRewardableLineMark.SetActive(data.isInRewardableLine);
        }

        private void OnClickButton()
        {
            if (data.state == BingoStateDecoratedData.State.Checkable)
            {
                onClickSlot?.Invoke(data);
            }            
        }
    }
}