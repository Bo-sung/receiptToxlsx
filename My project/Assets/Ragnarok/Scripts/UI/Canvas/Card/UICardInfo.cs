using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UICardInfo : UICanvas, CardInfoPresenter.IView
    {
        public sealed class Input : IUIData
        {
            public readonly ItemInfo itemInfo;
            public readonly ItemInfo equipmentInfo;
            public readonly byte index;
            public bool customBtnView;
            public bool showBtnRestoreInfo;
            public bool showBtnRestore;
            public bool showBtnConfirm;
            public bool showBtnLevelUp;

            public Input(ItemInfo itemInfo, ItemInfo equipmentInfo = null, byte index = 0)
            {
                this.itemInfo = itemInfo;
                this.equipmentInfo = equipmentInfo;
                this.index = index;
            }

            public Input ShowBtnRestoreInfo()
            {
                customBtnView = true;
                showBtnRestoreInfo = true;
                return this;
            }

            public Input ShowBtnRestore()
            {
                customBtnView = true;
                showBtnRestore = true;
                return this;
            }

            public Input ShowBtnConfirm()
            {
                customBtnView = true;
                showBtnConfirm = true;
                return this;
            }

            public Input ShowBtnLevelUp()
            {
                customBtnView = true;
                showBtnLevelUp = true;
                return this;
            }
        }

        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButtonHelper background;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnLevelup;
        [SerializeField] UIButtonHelper btnRestore;
        [SerializeField] UIButtonHelper btnRestoreInfo;
        [SerializeField] UICardStatusInfo cardStatusInfo;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UIGrid gridBtn;
        [SerializeField] UILabelHelper cardQualityPanelLabel;
        [SerializeField] UILabelHelper cardQualityLabel;
        [SerializeField] UIButtonHelper btnGetSource;
        [SerializeField] UIButtonHelper btnUseSource;
        [SerializeField] GameObject goGetSourceLine;
        [SerializeField] GameObject goUseSourceLine;
        [SerializeField] UIToolTipHelper qualityPanel;
        [SerializeField] UILabelHelper labelNotice;

        CardInfoPresenter presenter;

        protected override void OnInit()
        {
            presenter = new CardInfoPresenter(this);

            presenter.AddEvent();
            EventDelegate.Add(background.OnClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);
            EventDelegate.Add(btnLevelup.OnClick, OnClickedBtnLevelup);
            EventDelegate.Add(btnRestore.OnClick, OnClickedBtnRestore);
            EventDelegate.Add(btnGetSource.OnClick, OnClickBtnGetSource);
            EventDelegate.Add(btnUseSource.OnClick, OnClickBtnUseSource);

            // 현재 카드 수치를 종합한 퍼센트입니다.\n강화, 복원, 재구성을 통해 변경할 수 있습니다.
            qualityPanel.SetToolTipLocalizeKey(LocalizeKey._18012);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            EventDelegate.Remove(background.OnClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);
            EventDelegate.Remove(btnLevelup.OnClick, OnClickedBtnLevelup);
            EventDelegate.Remove(btnRestore.OnClick, OnClickedBtnRestore);
            EventDelegate.Remove(btnGetSource.OnClick, OnClickBtnGetSource);
            EventDelegate.Remove(btnUseSource.OnClick, OnClickBtnUseSource);
        }

        protected override void OnShow(IUIData data)
        {
            if (data is Input)
            {
                presenter.SelectInfo(data as Input);
                return;
            }

            Debug.LogError("data가 존재하지 않습니다");
            CloseUI();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._18000; // 아이템 정보
            btnConfirm.LocalKey = LocalizeKey._18003; // 확인
            btnLevelup.LocalKey = LocalizeKey._18504; // 레벨업
            btnRestore.LocalKey = LocalizeKey._28035;
            cardQualityPanelLabel.LocalKey = LocalizeKey._28049;
            labelNotice.LocalKey = LocalizeKey._18013; // 5랭크 쉐도우 카드는 쉐도우 장비에 하나만 장착할 수 있습니다.
        }

        public void Refresh()
        {
            if (presenter.input == null)
                return;

            ItemInfo info = presenter.input.itemInfo;

            cardStatusInfo.SetData(info);

            btnRestoreInfo.SetActive(false);
            btnRestore.SetActive(false);
            btnConfirm.SetActive(false);
            btnLevelup.SetActive(false);

            if (presenter.input.customBtnView)
            {
                btnRestoreInfo.SetActive(presenter.input.showBtnRestoreInfo);
                btnRestore.SetActive(presenter.input.showBtnRestore);
                btnConfirm.SetActive(presenter.input.showBtnConfirm);
                btnLevelup.SetActive(presenter.input.showBtnLevelUp);
            }
            else
            {
                // 가방 카드탭에서 선택해서 들어온 경우
                if (presenter.input.equipmentInfo == null || info.IsEquipped)
                {
                    btnRestore.SetActive(true);
                    btnLevelup.SetActive(true);
                    btnConfirm.SetActive(false);
                }
                else
                {
                    btnLevelup.SetActive(false);
                    btnConfirm.SetActive(true);
                    // 가방 장비탭에서 인첸트를 통해 들어온경우
                    if (info.IsEquipped)
                    {
                        btnConfirm.LocalKey = LocalizeKey._18002; // 해제
                    }
                    else
                    {
                        btnConfirm.LocalKey = LocalizeKey._18001; // 인챈트
                    }
                }
            }

            gridBtn.repositionNow = true;

            btnLevelup.IsEnabled = info.CanLevelUp;

            bool hasGetSource = (info.Get_ClassBitType != ItemSourceCategoryType.None);
            btnGetSource.Text = StringBuilderPool.Get()
                .Append(UIItemSource.GetActivationColorString(isActive: hasGetSource))
                .Append(LocalizeKey._18007.ToText()).Release(); // 획득처 보기
            goGetSourceLine.SetActive(hasGetSource);

            bool hasUseSource = (info.Use_ClassBitType != ItemSourceCategoryType.None);
            btnUseSource.Text = StringBuilderPool.Get()
                .Append(UIItemSource.GetActivationColorString(isActive: hasUseSource))
                .Append(LocalizeKey._18006.ToText()).Release(); // 사용처 보기
            goUseSourceLine.SetActive(hasUseSource);

            cardQualityLabel.Text = $"{MathUtils.ToInt(info.OptionRate * 100)}%";

            labelNotice.SetActive(info.IsShadow);
        }

        private void CloseUI()
        {
            UI.Close<UICardInfo>();
        }

        private async void OnClickedBtnConfirm()
        {
            ItemInfo info = presenter.input.itemInfo;

            if (presenter.input.equipmentInfo == null)
            {
                CloseUI();
            }
            else
            {
                // 가방 장비탭에서 인첸트를 통해 들어온경우
                if (info.IsEquipped)
                {
                    await presenter.RequestUnEnchantEquipment();
                    CloseUI();
                }
                else
                {
                    await presenter.RequestEnchantEquipment();
                    CloseUI();
                    UI.Close<UICardInven>();
                }
            }
        }

        void OnClickedBtnLevelup()
        {
            UI.Show<UICardSmelt>(presenter.input.itemInfo);
        }

        void OnClickedBtnRestore()
        {
            UI.ShortCut<UIMake>(new UIMake.Input(UIMake.InputType.GoToRestoreCard, presenter.input.itemInfo.ItemNo));
        }

        /// <summary>
        /// 획득처 보기
        /// </summary>
        void OnClickBtnGetSource()
        {
            UI.Show<UIItemSource>(new UIItemSource.Input(UIItemSource.Mode.GetSource, presenter.input.itemInfo));
        }

        /// <summary>
        /// 사용처 보기
        /// </summary>
        void OnClickBtnUseSource()
        {
            UI.Show<UIItemSource>(new UIItemSource.Input(UIItemSource.Mode.Use, presenter.input.itemInfo));
        }
    }
}
