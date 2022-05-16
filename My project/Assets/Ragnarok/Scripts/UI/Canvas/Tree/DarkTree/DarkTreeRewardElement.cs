using UnityEngine;

namespace Ragnarok.View
{
    public class DarkTreeRewardElement : UIElement<DarkTreeRewardElement.IInput>
    {
        public interface IInput
        {
            int Id { get; }
            RewardData GetReward();

            int GetMaxPoint();
            int GetTotalMinutes();
        }

        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UIButtonHelper btnSelect;
        [SerializeField] GameObject goSelect;

        public event System.Action<int> OnSelect;

        private int selectedRewardId;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnSelect.OnClick, OnClickedBtnSelect);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnSelect.OnClick, OnClickedBtnSelect);
        }

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            if (info == null)
                return;

            NGUITools.SetActive(goSelect, info.Id == selectedRewardId);
            rewardHelper.SetData(info.GetReward());
        }

        void OnClickedBtnSelect()
        {
            if (info == null)
                return;

            OnSelect?.Invoke(info.Id);
        }

        public void SetSelectedRewardId(int selectedRewardId)
        {
            this.selectedRewardId = selectedRewardId;
            Refresh();
        }
    }
}