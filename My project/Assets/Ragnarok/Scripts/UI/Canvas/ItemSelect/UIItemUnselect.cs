using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIItemUnselect : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] SelectPopupView popupView;
        [SerializeField] ItemUnselectView itemUnselectView;

        private TaskAwaiter<long[]> awaiter;
        private long[] result; // 선택하지 않은 아이템

        protected override void OnInit()
        {
            popupView.OnExit += CloseUI;
            popupView.OnCancel += CloseUI;
            popupView.OnConfirm += OnConfirm;
            itemUnselectView.OnCheckSelect += OnCheckSelect;
        }

        protected override void OnClose()
        {
            popupView.OnExit -= CloseUI;
            popupView.OnCancel -= CloseUI;
            popupView.OnConfirm -= OnConfirm;
            itemUnselectView.OnCheckSelect -= OnCheckSelect;
        }

        protected override void OnShow(IUIData data = null)
        {
            result = null;
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._6022; // 알림
            popupView.ConfirmLocalKey = LocalizeKey._6023; // 분해
            popupView.CancelLocalKey = LocalizeKey._6024; // 취소
        }

        public TaskAwaiter<long[]> Show(ItemInfo[] itemInfos)
        {
            Complete(DuplicateUIException.Default); // UI 중복

            Show();
            awaiter = new TaskAwaiter<long[]>();

            itemUnselectView.SetData(itemInfos);

            return awaiter;
        }

        void OnConfirm()
        {
            result = itemUnselectView.GetUnselectNos();
            CloseUI();
        }

        private void CloseUI()
        {
            Complete(null);
            UI.Close<UIItemUnselect>();
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

        private void OnCheckSelect(bool isCheck)
        {
            popupView.SetIsEnabledBtnConfirm(isCheck);
        }
    }
}