using UnityEngine;

namespace Ragnarok.View
{
    public sealed class EventStageEntryPopupView : UIView
    {
        public enum SelectResult
        {
            Cancel = 1,
            Confirm,
        }

        [SerializeField] EnvelopContent header;
        [SerializeField] UILabelHelper labelPopupTitle;
        [SerializeField] UIButton btnExit;
        [SerializeField] UITable table;
        [SerializeField] UILabelHelper labelMessage, labelLevel, labelPoint;
        [SerializeField] UILabelHelper labelDescription;

        [Header("EventRewardInfo")]
        [SerializeField] GameObject goEventRewardInfo;
        [SerializeField] UILabelHelper labelClearReward;
        [SerializeField] UIRewardHelper eventReward;

        [Header("RemainCountInfo")]
        [SerializeField] GameObject goRemainCountInfo;
        [SerializeField] UILabelValue remainCount;

        [Header("Bottom")]
        [SerializeField] UITable tableButton;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UIItemCostButtonHelper btnCostConfirm;

        private TaskAwaiter<SelectResult> awaiter;
        private SelectResult result;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnExit.onClick, Hide);
            EventDelegate.Add(btnCancel.OnClick, Hide);
            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);
            EventDelegate.Add(btnCostConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnExit.onClick, Hide);
            EventDelegate.Remove(btnCancel.OnClick, Hide);
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);
            EventDelegate.Remove(btnCostConfirm.OnClick, OnClickedBtnConfirm);

            Complete(DestroyUIException.Default); // UI 강제 제거
        }

        void OnClickedBtnConfirm()
        {
            result = SelectResult.Confirm;
            Hide();
        }

        protected override void OnLocalize()
        {
            labelClearReward.LocalKey = LocalizeKey._48267; // 보스 클리어 보상
            remainCount.TitleKey = LocalizeKey._48233; // 오늘 입장 가능 횟수
            btnCostConfirm.LocalKey = LocalizeKey._48235; // 입장
            btnCancel.LocalKey = LocalizeKey._48234; // 취소
        }

        public override void Show()
        {
            base.Show();

            result = SelectResult.Cancel;
        }

        public override void Hide()
        {
            base.Hide();

            Complete(null);
        }

        public TaskAwaiter<SelectResult> Show(string title, string message, int level, int point, string desc, RewardData rewardData, string confirmText)
        {
            bool hasCost = false;
            bool hasReward = rewardData != null;

            btnConfirm.SetActive(!hasCost);
            btnCostConfirm.SetActive(hasCost);
            goEventRewardInfo.SetActive(hasReward);
            goRemainCountInfo.SetActive(hasCost);

            eventReward.SetData(rewardData);
            btnConfirm.Text = confirmText;
            return Show(title, message, level, point, desc);
        }

        public TaskAwaiter<SelectResult> Show(string title, string message, int level, int point, string desc, RewardData rewardData, int remainEntryCount, string itemIcon, int itemCount)
        {
            bool hasCost = true;
            bool hasReward = rewardData != null;

            btnConfirm.SetActive(!hasCost);
            btnCostConfirm.SetActive(hasCost);
            goEventRewardInfo.SetActive(hasReward);
            goRemainCountInfo.SetActive(hasCost);

            eventReward.SetData(rewardData);
            remainCount.Value = remainEntryCount.ToString();
            btnCostConfirm.IsEnabled = remainEntryCount > 0;
            btnCostConfirm.SetItemIcon(itemIcon);
            btnCostConfirm.SetItemCount(itemCount);
            return Show(title, message, level, point, desc);
        }

        private TaskAwaiter<SelectResult> Show(string title, string message, int level, int point, string desc)
        {
            Complete(DuplicateUIException.Default); // UI 중복

            Show();

            tableButton.Reposition();
            table.Reposition();
            header.Execute();

            labelPopupTitle.Text = title;
            labelMessage.Text = message;
            labelLevel.Text = LocalizeKey._48229.ToText() // 도전 레벨 Lv. {LEVEL}
                .Replace(ReplaceKey.LEVEL, level);
            labelPoint.Text = LocalizeKey._48230.ToText() // (획득 점수 {POINT}점)
                .Replace(ReplaceKey.POINT, point);
            labelDescription.Text = desc;

            awaiter = new TaskAwaiter<SelectResult>();
            return awaiter;
        }

        private void Complete(UIException exception)
        {
            // Awaiter 음슴
            if (awaiter == null)
                return;

            if (!awaiter.IsCompleted)
                awaiter.Complete(result, exception);

            awaiter = null;
        }
    }
}