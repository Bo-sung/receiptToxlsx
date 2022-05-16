namespace Ragnarok
{
    public class UIConfirmPopupLong : UIConfirmPopup
    {
        private System.Action OnSelectConfrim;

        protected override void CloseUI()
        {
            UI.Close<UIConfirmPopupLong>();
            uiData.callback?.Invoke();
        }

        protected override void OnClickedBtnConfirm()
        {
            UI.Close<UIConfirmPopupLong>();
            OnSelectConfrim?.Invoke();
        }

        public void SetAction(System.Action action)
        {
            OnSelectConfrim = action;
        }
    }
}