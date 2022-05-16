using UnityEngine;

namespace Ragnarok
{
    public sealed class UIMailShortCutPopup : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButtonHelper btnHideView;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UILabelHelper labelItemName;
        [SerializeField] UIButtonHelper btnShortCut;
        [SerializeField] UILabelHelper labelCoolingOff;

        protected override void OnInit()
        {
            EventDelegate.Add(btnShortCut.OnClick, OnClickedBtnShortCut);
            EventDelegate.Add(btnHideView.OnClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnShortCut.OnClick, OnClickedBtnShortCut);
            EventDelegate.Remove(btnHideView.OnClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._5300; // 감사합니다!
            labelDesc.LocalKey = LocalizeKey._5301; // 상품이 우편으로 발송 되었습니다.
            btnShortCut.LocalKey = LocalizeKey._5302; // 우편함 바로가기
            labelCoolingOff.LocalKey = LocalizeKey._5303; // 우편함에서 해당 상품의 구성품을 수령 할 경우, 청약 철회가 불가능합니다.
        }

        public void Set(string itemName)
        {
            labelItemName.Text = itemName;
        }

        void CloseUI()
        {
            UI.Close<UIMailShortCutPopup>();
        }

        void OnClickedBtnShortCut()
        {
            if (GameServerConfig.IsOnBuff())
            {
                UI.ShortCut<UIMailOnBuff>().Set(tabIndex: 2);
            }
            else
            {
                UI.ShortCut<UIMail>().Set(tabIndex: 2);
            }
        }
    }
}