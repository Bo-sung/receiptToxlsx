using UnityEngine;

namespace Ragnarok.View
{
    public sealed class PassReward : UIView
    {
        [SerializeField] UIGrid rewardGrid;
        [SerializeField] PassSimpleReward[] rewards;
        [SerializeField] GameObject goArrow;
        [SerializeField] UIButtonHelper btnReceive;

        public event System.Action OnSelectReceive;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnReceive.OnClick, OnClickedBtnReceive);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnReceive.OnClick, OnClickedBtnReceive);
        }

        protected override void OnLocalize()
        {

        }

        void OnClickedBtnReceive()
        {
            OnSelectReceive?.Invoke();
        }

        public void SetRewardData(RewardData[] data)
        {
            int dataLength = data == null ? 0 : data.Length;
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(i < dataLength ? data[i] : null);
            }

            rewardGrid.Reposition();
        }

        public void SetBtnReceice(bool isActive)
        {
            btnReceive.SetActive(isActive);
        }

        public void SetComplete(bool isActive)
        {
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetComplete(isActive);
            }
        }

        public void SetNotice(bool isActive)
        {
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetNotice(isActive);
            }

            NGUITools.SetActive(goArrow, isActive);
        }

        public void SetLock(bool isActive)
        {
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetLock(isActive);
            }
        }
    }
}