using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class BoxRewardElement : UIView
    {
        public interface IInput
        {
            int GiveCount { get; }
            int TotalRate { get; }
            List<BoxRewardInfo> GetRewards();
        }

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelGiveCount;
        [SerializeField] UIButtonHelper btnFold;
        [SerializeField] UIButtonHelper btnUnfold;
        [SerializeField] UIGrid gridRewards;
        [SerializeField] BoxRewardSlot slot;

        private bool isFolded; // 접혀있는지 여부
        private Action onSelectFold;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnFold.OnClick, OnClickedSelectFold);
            EventDelegate.Add(btnUnfold.OnClick, OnClickedSelectFold);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnFold.OnClick, OnClickedSelectFold);
            EventDelegate.Remove(btnUnfold.OnClick, OnClickedSelectFold);
        }

        protected override void OnLocalize()
        {

        }

        public void Set(IInput input, int index, Action onSelectFold)
        {
            this.onSelectFold = onSelectFold;
            SetFold();
            labelTitle.Text = LocalizeKey._8301.ToText().Replace(ReplaceKey.VALUE, index + 1); // 구성품 그룹 {VALUE}
            labelGiveCount.Text = LocalizeKey._8302.ToText().Replace(ReplaceKey.COUNT, input.GiveCount); // 지급 횟수 x {COUNT}

            List<BoxRewardInfo> rewards = input.GetRewards();
            for (int i = 0; i < rewards.Count; i++)
            {
                BoxRewardSlot boxRewardSlot = NGUITools.AddChild(gridRewards.gameObject, slot.gameObject).GetComponent<BoxRewardSlot>();
                boxRewardSlot.Set(rewards[i], input.TotalRate);
            }
            gridRewards.Reposition();
        }

        private void OnClickedSelectFold()
        {
            isFolded = !isFolded;
            SetFold();
            onSelectFold?.Invoke();
        }
       
        private void SetFold()
        {
            gridRewards.gameObject.SetActive(!isFolded);
            btnFold.SetActive(isFolded);
            btnUnfold.SetActive(!isFolded);
        }
    }
}