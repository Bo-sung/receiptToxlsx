using UnityEngine;

namespace Ragnarok
{
    public sealed class UISelectShortCutPopup : UISelectPopup
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

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

            shortCutAction = null;
        }

        protected override void OnShow(IUIData data = null)
        {
            base.OnShow(data);
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();
        }

        /// <summary>
        /// 링크 클릭 시의 액션 설정
        /// </summary>
        public void SetAction(string linkDescription, System.Action action)
        {
            btnShortCut.Text = linkDescription;
            shortCutAction = action;
        }

        /// <summary>확인 버튼 클릭 이벤트</summary>
        protected override void OnClickedBtnConfirm()
        {
            uiData?.onClickedEvent?.Invoke(true);
            UI.Close<UISelectShortCutPopup>();
        }

        /// <summary>취소 버튼 클릭 이벤트</summary>
        protected override void OnClickedBtnCancel()
        {
            uiData?.onClickedEvent?.Invoke(false);
            UI.Close<UISelectShortCutPopup>();
        }

        void OnClickedBtnShortCut()
        {
            shortCutAction?.Invoke();
            UI.Close<UISelectShortCutPopup>();
        }
    }
}