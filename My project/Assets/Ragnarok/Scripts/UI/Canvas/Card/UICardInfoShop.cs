using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 거래소 정보 표시용
    /// </summary>
    public sealed class UICardInfoShop : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButtonHelper background;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UICardStatusInfo cardStatusInfo;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UIButtonHelper btnRestoreInfo;

        [SerializeField] UIButtonHelper btnGetSource;
        [SerializeField] UIButtonHelper btnUseSource;

        CardInfoShopPresenter presenter;

        protected override void OnInit()
        {
            presenter = new CardInfoShopPresenter();

            presenter.AddEvent();

            EventDelegate.Add(background.OnClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnConfirm.OnClick, CloseUI);
            EventDelegate.Add(btnGetSource.OnClick, OnClickBtnGetSource);
            EventDelegate.Add(btnUseSource.OnClick, OnClickBtnUseSource);
            EventDelegate.Add(btnRestoreInfo.OnClick, OnClickBtnRestoreInfo);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(background.OnClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnConfirm.OnClick, CloseUI);
            EventDelegate.Remove(btnGetSource.OnClick, OnClickBtnGetSource);
            EventDelegate.Remove(btnUseSource.OnClick, OnClickBtnUseSource);
            EventDelegate.Remove(btnRestoreInfo.OnClick, OnClickBtnRestoreInfo);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._18100; // 아이템 정보            
            btnConfirm.LocalKey = LocalizeKey._18101; // 확인
            btnGetSource.LocalKey = LocalizeKey._18102; // 획득처 보기
            btnUseSource.LocalKey = LocalizeKey._18103; // 사용처 보기
            btnRestoreInfo.LocalKey = LocalizeKey._22201;
        }

        public void Show(ItemInfo info)
        {
            if (info == null)
            {
                CloseUI();
                return;
            }
            presenter.Set(info);
            cardStatusInfo.SetData(info);
        }

        private void CloseUI()
        {
            UI.Close<UICardInfoShop>();
        }

        /// <summary>
        /// 획득처 보기
        /// </summary>
        void OnClickBtnGetSource()
        {
            UI.Show<UIItemSource>(new UIItemSource.Input(UIItemSource.Mode.GetSource, presenter.Item));
        }

        /// <summary>
        /// 사용처 보기
        /// </summary>
        void OnClickBtnUseSource()
        {
            UI.Show<UIItemSource>(new UIItemSource.Input(UIItemSource.Mode.Use, presenter.Item));
        }

        void OnClickBtnRestoreInfo()
        {
            UI.Show<UICardRestorePointInfo>(new UICardRestorePointInfo.Input() { cardItem = presenter.Item as CardItemInfo, mode = UICardRestorePointInfo.Mode.RestorePointInfo });
        }
    }
}