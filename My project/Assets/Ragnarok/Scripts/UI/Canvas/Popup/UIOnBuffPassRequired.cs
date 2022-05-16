using UnityEngine;

namespace Ragnarok
{
    public sealed class UIOnBuffPassRequired : UIConfirmPopup
    {
        public enum SelectResult
        {
            Exit = 1,
            Confirm,
        }

        [SerializeField] UIButtonHelper btnBuyPass;

        private TaskAwaiter<SelectResult> awaiter;
        private SelectResult result;

        protected override void OnInit()
        {
            base.OnInit();

            EventDelegate.Add(btnBuyPass.OnClick, OnClickedBtnBuyPass);
        }

        protected override void OnClose()
        {
            base.OnClose();

            EventDelegate.Remove(btnBuyPass.OnClick, OnClickedBtnBuyPass);

            Complete(CloseUIException.Default); // UI 강제 닫기
        }

        protected override void OnLocalize()
        {
            SetTitle(LocalizeKey._5.ToText()); // 알림
            SetDesc(LocalizeKey._90329.ToText()); // 해당 상품은 OnBuff 패스 전용 상품입니다.\nOnBuff 패스를 구입하시겠습니까?

            btnBuyPass.LocalKey = LocalizeKey._39819; // OnBuff 패스 구매
            btnConfirm.LocalKey = LocalizeKey._2; //취소
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Complete(DestroyUIException.Default); // UI 강제 제거
        }

        protected override void OnShow(IUIData data = null)
        {
            result = SelectResult.Exit;
        }

        public TaskAwaiter<SelectResult> ShowAsync()
        {
            Complete(DuplicateUIException.Default); // UI 중복

            Show();
            awaiter = new TaskAwaiter<SelectResult>();

            return awaiter;
        }

        void OnClickedBtnBuyPass()
        {
            result = SelectResult.Confirm;
            CloseUI();
        }

        protected override void CloseUI()
        {
            Complete(null);
            UI.Close<UIOnBuffPassRequired>();
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
