using UnityEngine;

namespace Ragnarok.View
{
    public class DarkTreeRewardView : UIView
    {
        [SerializeField] UIRewardHelper result;
        [SerializeField] UILabelHelper labelName, labelDescription;
        [SerializeField] UILabelValue needPoint, needTime;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] DarkTreeRewardElement element;
        [SerializeField] UILabelHelper labelNotice;

        private SuperWrapContent<DarkTreeRewardElement, DarkTreeRewardElement.IInput> wrapContent;

        public event System.Action<int> OnSelect;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<DarkTreeRewardElement, DarkTreeRewardElement.IInput>(element);
            foreach (var item in wrapContent)
            {
                item.OnSelect += OnSelectElement;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var item in wrapContent)
            {
                item.OnSelect -= OnSelectElement;
            }
        }

        protected override void OnLocalize()
        {
            labelNotice.LocalKey = LocalizeKey._9033; // 수확할 보상 아이템을 선택하세요.
            needPoint.TitleKey = LocalizeKey._9020; // 필요 포인트
            needTime.TitleKey = LocalizeKey._9019; // 수확 시간
        }

        void OnSelectElement(int id)
        {
            OnSelect?.Invoke(id);
        }

        public void SetData(DarkTreeRewardElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }

        public void Refresh(DarkTreeRewardElement.IInput selected)
        {
            // 현재 선택한 보상으로 세팅
            int selectedRewardId = selected.Id;
            foreach (var item in wrapContent)
            {
                item.SetSelectedRewardId(selectedRewardId);
            }

            // 보상 세팅
            RewardData rewardData = selected.GetReward();
            result.SetData(rewardData);

            labelName.Text = rewardData == null ? string.Empty : rewardData.ItemName;
            labelDescription.Text = rewardData == null ? string.Empty : rewardData.GetDescription();
            int maxPoint = selected.GetMaxPoint();
            needPoint.Value = maxPoint.ToString("N0"); // 필요 포인트 세팅
            needTime.Value = LocalizeKey._9039.ToText() // {MINUTES}분
                .Replace(ReplaceKey.MINUTES, selected.GetTotalMinutes());
        }
    }
}