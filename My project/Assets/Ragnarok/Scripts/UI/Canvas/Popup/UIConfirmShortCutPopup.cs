using UnityEngine;

namespace Ragnarok
{
    public sealed class UIConfirmShortCutPopup : UIConfirmPopup
    {
        [SerializeField] UIButtonHelper btnShortCut;
        System.Action shortCutAction;

        protected override void OnInit()
        {
            base.OnInit();

            EventDelegate.Add(btnShortCut.OnClick, OnClickedBtnShortCut);
        }

        protected override void OnClose()
        {
            base.OnClose();

            EventDelegate.Remove(btnShortCut.OnClick, OnClickedBtnShortCut);
        }

        protected override void OnHide()
        {
            base.OnHide();

            shortCutAction = null;
        }

        /// <summary>
        /// 링크 클릭 시의 액션 설정
        /// </summary>
        public void SetAction(string linkDescription, System.Action action)
        {
            btnShortCut.Text = linkDescription;
            shortCutAction = action;
        }

        protected override void OnBack()
        {
            CloseUI();
        }

        protected override void CloseUI()
        {
            uiData.callback?.Invoke();
            OnClickedBtnShortCut();
        }

        void OnClickedBtnShortCut()
        {
            shortCutAction?.Invoke();
            UI.Close<UIConfirmShortCutPopup>();
        }
    }
}