using System;
using UnityEngine;

namespace Ragnarok
{
    public class UISelectPopupData : UIConfirmPopupData
    {
        public string cancelText;
        public ConfirmButtonType confirmButtonType;
        public Action<bool?> onClickedEvent;

        public UISelectPopupData OnClickedEvent(Action<bool?> onClickedEvent)
        {
            this.onClickedEvent = onClickedEvent;
            return this;
        }
    }

    public class UISelectPopup : UICanvas, SelectPopupPresenter.IView, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labTitle;
        [SerializeField] UILabelHelper labDesc;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UIButtonHelper btnConfirmAd;
        [SerializeField] UIButtonHelper btnConfirmRed;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UISprite sprIcon;
        [SerializeField] OpenTooltipOnClick openTooltipOnClick;

        SelectPopupPresenter presenter;
        protected UISelectPopupData uiData;

        protected override void OnInit()
        {
            presenter = new SelectPopupPresenter(this);
            presenter.AddEvent();

            if (btnConfirmAd)
                EventDelegate.Add(btnConfirmAd.OnClick, OnClickedBtnConfirm);
            if (btnConfirmRed)
                EventDelegate.Add(btnConfirmRed.OnClick, OnClickedBtnConfirm);

            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);
            EventDelegate.Add(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Add(btnExit.OnClick, OnClickedBtnClose);
            if (openTooltipOnClick)
                openTooltipOnClick.OnShowTooltip += OnShowTooltip;
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            if (btnConfirmAd)
                EventDelegate.Remove(btnConfirmAd.OnClick, OnClickedBtnConfirm);
            if (btnConfirmRed)
                EventDelegate.Remove(btnConfirmRed.OnClick, OnClickedBtnConfirm);

            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);
            EventDelegate.Remove(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Remove(btnExit.OnClick, OnClickedBtnClose);
            if (openTooltipOnClick)
                openTooltipOnClick.OnShowTooltip -= OnShowTooltip;
        }

        protected override void OnShow(IUIData data = null)
        {
            if (data != null)
                uiData = data as UISelectPopupData;

            labTitle.Text = uiData.title;
            labDesc.Text = uiData.description;
            if (sprIcon)
                sprIcon.enabled = uiData.isShowIcon;

            btnCancel.Text = uiData.cancelText;

            btnConfirm.SetActive(uiData.confirmButtonType == ConfirmButtonType.None);
            if (btnConfirmAd)
                btnConfirmAd.SetActive(uiData.confirmButtonType == ConfirmButtonType.Ad);
            if (btnConfirmRed)
                btnConfirmRed.SetActive(uiData.confirmButtonType == ConfirmButtonType.Red);

            switch (uiData.confirmButtonType)
            {
                case ConfirmButtonType.None:
                    btnConfirm.Text = uiData.confirmText;
                    break;
                case ConfirmButtonType.Ad:
                    if (btnConfirmAd)
                        btnConfirmAd.Text = uiData.confirmText;
                    break;
                case ConfirmButtonType.Red:
                    if (btnConfirmRed)
                        btnConfirmRed.Text = uiData.confirmText;
                    break;
            }
        }

        protected override void OnHide() { }

        protected override void OnLocalize()
        {
        }

        public void SetBtnConfirmText(string text)
        {
            btnConfirm.Text = text;

            if (btnConfirmAd)
            {
                btnConfirmAd.Text = text;
            }
        }

        public void SetBtnCancelText(string text)
        {
            btnCancel.Text = text;
        }

        #region 버튼 이벤트

        /// <summary>확인 버튼 클릭 이벤트</summary>
        protected virtual void OnClickedBtnConfirm()
        {
            uiData?.onClickedEvent?.Invoke(true);
            UI.Close<UISelectPopup>();
        }

        /// <summary>취소 버튼 클릭 이벤트</summary>
        protected virtual void OnClickedBtnCancel()
        {
            uiData?.onClickedEvent?.Invoke(false);
            UI.Close<UISelectPopup>();
        }

        /// <summary>닫기 버튼 클릭 이벤트</summary>
        protected virtual void OnClickedBtnClose()
        {
            uiData?.onClickedEvent?.Invoke(null);
            UI.Close<UISelectPopup>();
        }

        #endregion

        void OnShowTooltip()
        {
            UI.Close<UISelectPopup>();
        }

        protected override void OnBack()
        {
            OnClickedBtnClose();
        }
    }
}