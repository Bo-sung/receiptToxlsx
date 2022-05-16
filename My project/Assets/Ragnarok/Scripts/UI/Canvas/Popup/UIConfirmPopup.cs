using System;
using UnityEngine;

namespace Ragnarok
{
    public class UIConfirmPopupData : IUIData
    {
        public Action callback;
        public string title;
        public string description;
        public bool isShowIcon;
        public float timeout;
        public string confirmText;

        public UIConfirmPopupData OnClickedEvent(Action callback)
        {
            this.callback = callback;
            return this;
        }
    }

    public class UIConfirmPopup : UICanvas, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;
        public override int layer => Layer.UI_Popup;

        [SerializeField] UILabelHelper labTitle;
        [SerializeField] UILabelHelper labDesc;
        [SerializeField] protected UIButtonHelper btnConfirm;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] OpenTooltipOnClick openTooltipOnClick;

        protected UIConfirmPopupData uiData;

        protected override void OnInit()
        {
            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            if (openTooltipOnClick)
                openTooltipOnClick.OnShowTooltip += OnShowTooltip;
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            if (openTooltipOnClick)
                openTooltipOnClick.OnShowTooltip -= OnShowTooltip;
        }

        protected override void OnShow(IUIData data = null)
        {
            if (data != null)
                uiData = data as UIConfirmPopupData;

            SetTitle(uiData.title);
            SetDesc(uiData.description);
            btnConfirm.Text = uiData.confirmText;
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        protected virtual void OnClickedBtnConfirm()
        {
            CloseUI();
        }

        protected virtual void CloseUI()
        {
            UI.Close<UIConfirmPopup>();
            uiData.callback?.Invoke();
        }

        protected virtual void OnShowTooltip()
        {
            UI.Close<UIConfirmPopup>();
        }

        protected override void OnBack()
        {
            CloseUI();
        }

        protected void SetTitle(string title)
        {
            labTitle.Text = title;
        }

        protected void SetDesc(string desc)
        {
            labDesc.Text = desc;
        }
    }
}