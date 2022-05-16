using UnityEngine;

namespace Ragnarok
{
    public class UIPrivateStoreNameEdit : UICanvas, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UIInput inputText;

        PrivateStoreNameEditPresenter presenter;

        protected override void OnInit()
        {
            presenter = new PrivateStoreNameEditPresenter();
            presenter.AddEvent();

            EventDelegate.Add(btnExit.OnClick, OnClickBtnExit);
            EventDelegate.Add(btnClose.OnClick, OnClickBtnClose);
            EventDelegate.Add(btnConfirm.OnClick, OnClickBtnConfirm);

            inputText.characterLimit = Constants.Trade.PRIVATE_STORE_NAME_MAX_COUNT;
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnExit.OnClick, OnClickBtnExit);
            EventDelegate.Remove(btnClose.OnClick, OnClickBtnClose);
            EventDelegate.Remove(btnConfirm.OnClick, OnClickBtnConfirm);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitle.Text = LocalizeKey._45100.ToText(); // 점포 이름
            labelNotice.Text = LocalizeKey._45101.ToText(); // 노점 이름은 최대 16자 입니다.
            btnClose.LocalKey = LocalizeKey._45102; // 취소
            btnConfirm.LocalKey = LocalizeKey._45103; // 확인
        }

        protected override void OnShow(IUIData data = null)
        {
            inputText.value = presenter.GetCurrentMyStoreComment();
        }

        /// <summary>
        /// 노점 이름 변경 버튼
        /// </summary>
        void OnClickBtnConfirm()
        {
            // 글자수 체크
            if (inputText.value.Length > Constants.Trade.PRIVATE_STORE_NAME_MAX_COUNT)
            {
                // 글자수 관련 팝업.
                UI.ConfirmPopup(LocalizeKey._90077.ToText()); //노점 이름은 최대 16자 입니다.
                return;
            }

            string title = FilterUtils.ReplaceChat(inputText.value);

            presenter.RequestTitleChange(title);
            CloseUI();           
        }

        void OnClickBtnExit()
        {
            CloseUI();
        }

        void OnClickBtnClose()
        {
            CloseUI();
        }

        void CloseUI()
        {
            UI.Close<UIPrivateStoreNameEdit>();
        }
    }
}