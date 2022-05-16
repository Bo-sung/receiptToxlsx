using UnityEngine;

namespace Ragnarok
{
    public class UICostPopupData : UISelectPopupData
    {
        public CoinType coinType;
        public int needCoin;
    }

    public sealed class UICostPopup : UICanvas, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labTitle;
        [SerializeField] UILabelHelper labDesc;
        [SerializeField] UIButtonWithIconValue btnConfirm;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnExit;

        UICostPopupData uiData;

        protected override void OnInit()
        {
            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);
            EventDelegate.Add(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Add(btnExit.OnClick, OnClickedBtnCancel);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);
            EventDelegate.Remove(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Remove(btnExit.OnClick, OnClickedBtnCancel);
        }

        protected override void OnShow(IUIData data = null)
        {
            if (data != null)
                uiData = data as UICostPopupData;

            labTitle.Text = uiData.title;
            labDesc.Text = uiData.description;
            btnConfirm.SetIconName(uiData.coinType.IconName());
            btnConfirm.SetLabelValue(uiData.needCoin.ToString("N0"));

            btnConfirm.Text = uiData.confirmText;
            btnCancel.Text = uiData.cancelText;
        }

        protected override void OnHide() { }

        protected override void OnLocalize()
        {
        }

        #region 버튼 이벤트

        /// <summary>확인 버튼 클릭 이벤트</summary>
        void OnClickedBtnConfirm()
        {
            uiData?.onClickedEvent?.Invoke(true);
            UI.Close<UICostPopup>();
        }

        /// <summary>취소 버튼 클릭 이벤트</summary>
        void OnClickedBtnCancel()
        {
            uiData?.onClickedEvent?.Invoke(false);
            UI.Close<UICostPopup>();
        }

        #endregion
    }
}
