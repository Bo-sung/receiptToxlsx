using UnityEngine;

namespace Ragnarok.View
{
    public sealed class NabihoRewardElement : UIElement<NabihoRewardElement.IInput>
    {
        public interface IInput
        {
            int Id { get; }
            RewardData Reward { get; }
            int NeedMinute { get; }
            int IntimacyCondition { get; }
        }

        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UIButtonHelper btnSelect;
        [SerializeField] GameObject goSelect;
        [SerializeField] GameObject goLock;
        [SerializeField] UILabelHelper labelLock;

        public event System.Action<IInput> OnSelect;

        private int selectedId;
        private int currentLevel;

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
            rewardHelper.SetData(info.Reward);
            UpdateLock();
            UpdateSelect();
        }

        void OnClickedBtnSelect()
        {
            OnSelect?.Invoke(info);
        }

        public void UpdateLevel(int level)
        {
            currentLevel = level;
            UpdateLock();
        }

        public void SetSelectedId(int id)
        {
            selectedId = id;
            UpdateSelect();
        }

        private void UpdateLock()
        {
            if (info == null)
                return;

            bool isLock = currentLevel < info.IntimacyCondition;
            NGUITools.SetActive(goLock, isLock);

            if (isLock)
            {
                labelLock.Text = LocalizeKey._10917.ToText() // 친밀도\n{VALUE}단계
                    .Replace(ReplaceKey.VALUE, info.IntimacyCondition);
            }
        }

        private void UpdateSelect()
        {
            if (info == null)
                return;

            NGUITools.SetActive(goSelect, info.Id == selectedId);
        }
    }
}