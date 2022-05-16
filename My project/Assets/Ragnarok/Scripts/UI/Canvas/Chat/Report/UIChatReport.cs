using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIChatReport : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;
        public override int layer => Layer.UI_Popup;

        [SerializeField] SelectPopupView popupView;
        [SerializeField] ChatReportView chatReportView;

        ChatReportPresenter presenter;

        protected override void OnInit()
        {
            presenter = new ChatReportPresenter();

            popupView.OnExit += OnBack;
            popupView.OnCancel += OnBack;
            popupView.OnConfirm += OnConfirm;
            chatReportView.OnSelect += OnSelect;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            popupView.OnExit -= OnBack;
            popupView.OnCancel -= OnBack;
            popupView.OnConfirm -= OnConfirm;
            chatReportView.OnSelect -= OnSelect;
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._6200; // 유저 신고
            popupView.ConfirmLocalKey = LocalizeKey._6206; // 신고
            popupView.CancelLocalKey = LocalizeKey._6205; // 취소
        }

        public void Set(int cid, string nickName)
        {
            string name = $"{nickName}({MathUtils.CidToHexCode(cid)})";
            chatReportView.SetName(name);
            chatReportView.ResetTab();
            presenter.SetCid(cid);
            presenter.SetTabIndex(-1);
        }

        void OnConfirm()
        {
            presenter.RequestChatReport();
        }

        private void OnSelect(int tabIndex)
        {
            presenter.SetTabIndex(tabIndex);
        }
    }
}