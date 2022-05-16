using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGuestNoticePopup : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        public enum SelectResult
        {
            Cancel = 1,
            Confirm,
        }

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButton btnExit;
        [SerializeField] UILabelHelper labelMessage;
        [SerializeField] UIButtonHelper btnCancel, btnConfirm;

        private TaskAwaiter<SelectResult> awaiter;
        private SelectResult result;

        protected override void OnInit()
        {
            EventDelegate.Add(btnExit.onClick, CloseUI);
            EventDelegate.Add(btnCancel.OnClick, CloseUI);
            EventDelegate.Add(btnConfirm.OnClick, OnClickedConfirm);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnExit.onClick, CloseUI);
            EventDelegate.Remove(btnCancel.OnClick, CloseUI);
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedConfirm);

            Complete(CloseUIException.Default); // UI 강제 닫기
        }

        protected override void OnShow(IUIData data = null)
        {
            result = SelectResult.Cancel;
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnCancel.LocalKey = LocalizeKey._2; // 취소
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Complete(DestroyUIException.Default); // UI 강제 제거
        }

        public TaskAwaiter<SelectResult> Show(string title, string message, string cancelText, string confirmText)
        {
            Complete(DuplicateUIException.Default); // UI 중복

            Show();
            awaiter = new TaskAwaiter<SelectResult>();

            labelTitle.Text = title;
            labelMessage.Text = message;
            btnCancel.Text = cancelText;
            btnConfirm.Text = confirmText;
            return awaiter;
        }

        void OnClickedConfirm()
        {
            result = SelectResult.Confirm;
            CloseUI();
        }

        private void CloseUI()
        {
            Complete(null);
            UI.Close<UIGuestNoticePopup>(); 
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

        protected override void OnBack()
        {
            CloseUI();
        }
    }
}