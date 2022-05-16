using UnityEngine;

namespace Ragnarok.View
{
    public sealed class NabihoRewardSelectView : SelectPopupView
    {
        [SerializeField] UIRewardHelper result;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UILabelValue needTime;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] NabihoRewardElement element;
        [SerializeField] UILabelHelper labelNotice;

        private SuperWrapContent<NabihoRewardElement, NabihoRewardElement.IInput> wrapContent;
        private int currentLevel;
        private int selectedId;

        public event System.Action<int> OnSelect;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<NabihoRewardElement, NabihoRewardElement.IInput>(element);
            foreach (var item in wrapContent)
            {
                item.OnSelect += OnSelectElement;
            }

            MainTitleLocalKey = LocalizeKey._10915; // 재료 선택
            ConfirmLocalKey = LocalizeKey._10919; // 보상 결정
            CancelLocalKey = LocalizeKey._2; // 취소

            OnConfirm += ExecuteConfirm;
            OnConfirm += Hide;
            OnCancel += Hide;
            OnExit += Hide;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var item in wrapContent)
            {
                item.OnSelect -= OnSelectElement;
            }

            OnConfirm -= ExecuteConfirm;
            OnConfirm -= Hide;
            OnCancel -= Hide;
            OnExit -= Hide;
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();

            needTime.TitleKey = LocalizeKey._10916; // 완료까지 시간
            labelNotice.LocalKey = LocalizeKey._10918; // 보상 받을 재료를 선택하세요.
        }

        void OnSelectElement(NabihoRewardElement.IInput input)
        {
            System.TimeSpan timeSpan = System.TimeSpan.FromMinutes(input.NeedMinute);

            result.SetData(input.Reward);
            labelName.Text = input.Reward.ItemName;
            labelDescription.Text = input.Reward.GetDescription();
            needTime.Value = StringUtils.TimeToText(timeSpan);

            SetSelectedRewardId(input.Id);
            SetIsEnabledBtnConfirm(currentLevel >= input.IntimacyCondition);
        }

        void ExecuteConfirm()
        {
            OnSelect?.Invoke(selectedId);
        }

        public void Show(NabihoRewardElement.IInput[] inputs)
        {
            Show();

            wrapContent.SetProgress(0f);
            wrapContent.SetData(inputs);

            int length = inputs == null ? 0 : inputs.Length;
            if (length == 0)
                return;

            // 첫번째 데이터 선택
            OnSelectElement(inputs[0]);
        }

        public void UpdateLevel(int level)
        {
            currentLevel = level;

            foreach (var item in wrapContent)
            {
                item.UpdateLevel(currentLevel);
            }
        }

        private void SetSelectedRewardId(int selectedId)
        {
            this.selectedId = selectedId;

            foreach (var item in wrapContent)
            {
                item.SetSelectedId(selectedId);
            }
        }
    }
}