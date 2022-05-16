using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIAmountBuyData
    {
        public delegate void UIAmountBuyResponse(PopupType popupType, int buyCount);

        public ShopInfo shopInfo;
        public UIAmountBuyResponse onClickedEvent;

        public UIAmountBuyData OnClickedEvent(UIAmountBuyResponse onClickedEvent)
        {
            this.onClickedEvent = onClickedEvent;
            return this;
        }
    }

    public sealed class UIAmountBuy : UICanvas<AmountBuyPresenter>, AmountBuyPresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] int smallSize = 440;
        [SerializeField] int fullSize = 662;
        [SerializeField] UIWidget popupBase;

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper btnExit;

        [SerializeField] UIRewardHelper reward;
        [SerializeField] UILabelHelper labelName;

        [SerializeField] UILabelHelper labelHaveMoneyLocal;
        [SerializeField] UILabelHelper labelNeedMoneyLocal;
        [SerializeField] UILabelHelper labelNeedCashLocal;

        [SerializeField] UISprite sprHaveCoinType;
        [SerializeField] UILabelHelper labelHaveCoinValue;
        [SerializeField] UISprite sprNeedCoinType;
        [SerializeField] UILabelHelper labelTotalPriceValue;
        [SerializeField] UILabelHelper labelCashValue;

        [SerializeField] UIButtonHelper btnCountSub;
        [SerializeField] UIButtonHelper btnCountAdd;
        [SerializeField] UILabelHelper labelCount;

        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnBuy;

        // 인게임 재화로 구매
        [SerializeField] GameObject goods;
        // 현금으로 구매
        [SerializeField] GameObject cash;

        [SerializeField] UILabelHelper labelOwned;
        [SerializeField] UILabelHelper labelOwnedValue;

        [SerializeField] AddGoodsView addGoodsView;
        [SerializeField] UILabelHelper labelCoolingOff;
        [SerializeField] UILabelHelper labelOnBuffNotice;

        private UIAmountBuyData data;
        private int buyCount; // 현재 구매 개수

        protected override void OnInit()
        {
            presenter = new AmountBuyPresenter(this);
            presenter.AddEvent();

            EventDelegate.Add(btnExit.OnClick, OnClickedBtnExit);
            EventDelegate.Add(btnCountSub.OnClick, OnClickedBtnCountSub);
            EventDelegate.Add(btnCountAdd.OnClick, OnClickedBtnCountAdd);
            EventDelegate.Add(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Add(btnBuy.OnClick, OnClickedBtnBuy);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnExit.OnClick, OnClickedBtnExit);
            EventDelegate.Remove(btnCountSub.OnClick, OnClickedBtnCountSub);
            EventDelegate.Remove(btnCountAdd.OnClick, OnClickedBtnCountAdd);
            EventDelegate.Remove(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Remove(btnBuy.OnClick, OnClickedBtnBuy);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        public void Show(UIAmountBuyData data)
        {
            this.data = data;
            buyCount = 1;
            presenter.SetData(this.data.shopInfo);
            Refresh();
        }

        protected override void OnHide()
        {
            presenter.SetData(null);
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._8100; // 구매
            labelHaveMoneyLocal.LocalKey = LocalizeKey._8101; // 보유 재화
            labelNeedMoneyLocal.LocalKey = LocalizeKey._8102; // 구매 가격
            labelNeedCashLocal.LocalKey = LocalizeKey._8102; // 구매 가격
            btnCancel.LocalKey = LocalizeKey._8103;  // 취소
            btnBuy.LocalKey = LocalizeKey._8104; // 구매
            labelOwned.LocalKey = LocalizeKey._8105; // 보유

            if (GameServerConfig.IsKoreaLanguage())
            {
                const int LOCAL_KEY = LocalizeKey._90302; // 구매하는 상품의 수량 및 금액을 반드시 확인하시기 바랍니다.\n유료재화로 구매된 상품은 구매 후 7일 이내에는 청약철회가 가능합니다.\n상품을 사용하였거나 즉시 적용되는 상품의 경우, 추가 혜택이 사용된 경우 청약 철회 대상에서 제외됩니다.\n(유료 재화부터 우선 소모 됩니다.)
                labelCoolingOff.Text = BasisUrl.KoreanPurchaseTerms.AppendText(LOCAL_KEY.ToText());
            }
            else
            {
                labelCoolingOff.LocalKey = LocalizeKey._90306; // 구매하는 상품의 수량 및 금액을 반드시 확인하시기 바랍니다.
            }

            labelOnBuffNotice.LocalKey = LocalizeKey._90328; // ※ 해당 상품은 OnBuff 패스 전용 상품입니다.
        }

        void Refresh()
        {
            if (presenter.IsInvalid())
                return;

            // 상품 프로필
            reward.SetData(presenter.GetRewardData());
            labelName.Text = presenter.GetItemName();

            // 구매 정보
            CoinType coinType = data.shopInfo.CoinType;
            bool isGoods = coinType.IsGoods();
            goods.SetActive(isGoods);
            cash.SetActive(!isGoods);

            // 재화구매
            if (isGoods)
            {
                // 재화 아이콘
                string coinIconName = presenter.GetCoinTypeIconName();
                sprHaveCoinType.spriteName = coinIconName;
                sprNeedCoinType.spriteName = coinIconName;

                // 값 업데이트
                RefreshValue();
            }
            else
            {
                labelCashValue.Text = data.shopInfo.CostText;
            }

            RefreshOwned();
            UpdateAddGoodsView();
            labelOnBuffNotice.SetActive(coinType == CoinType.OnBuffPoint);
        }

        /// <summary>
        /// 값 업데이트 (구매 개수, 가격 등)
        /// </summary>
        void RefreshValue()
        {
            if (presenter.IsInvalid())
                return;

            if (this.buyCount > presenter.GetMaxBuyCount())
                this.buyCount = presenter.GetMaxBuyCount();

            int priceEA = presenter.GetPrice();
            int totalPrice = this.buyCount * priceEA;
            long haveCoin = presenter.GetHaveCoin();
            if (totalPrice > haveCoin)
            {
                this.buyCount = Mathf.Max(1, this.buyCount - 1);
            }
            labelTotalPriceValue.Text = (this.buyCount * priceEA).ToString("N0"); // 필요 재화
            RefreshHaveCoin(); // 보유 재화 업데이트

            labelCount.Text = this.buyCount.ToString(); // 구매 개수
        }

        /// <summary>
        /// 보유 재화만 업데이트
        /// </summary>
        void RefreshHaveCoin()
        {
            if (presenter.IsInvalid())
                return;

            labelHaveCoinValue.Text = presenter.GetHaveCoin().ToString("N0");
        }

        /// <summary>
        /// 보유 수량 업데이트
        /// </summary>
        void RefreshOwned()
        {
            if (presenter.IsInvalid())
                return;

            labelOwnedValue.Text = presenter.GetOwnedValue().ToString("N0");
        }

        #region 버튼 이벤트

        /// <summary>
        /// 종료 버튼
        /// </summary>
        void OnClickedBtnExit()
        {
            this.data.onClickedEvent?.Invoke(PopupType.Cancel, this.buyCount);
            CloseUI();
        }

        /// <summary>
        /// 개수 감소
        /// </summary>
        void OnClickedBtnCountSub()
        {
            --buyCount;
            if (buyCount < 1)
                buyCount = 1;

            labelCount.Text = buyCount.ToString();
            RefreshValue();
        }

        /// <summary>
        /// 개수 증가
        /// </summary>
        void OnClickedBtnCountAdd()
        {
            ++buyCount;

            labelCount.Text = buyCount.ToString();
            RefreshValue();
        }

        /// <summary>
        /// 취소 버튼
        /// </summary>
        void OnClickedBtnCancel()
        {
            this.data.onClickedEvent?.Invoke(PopupType.Cancel, this.buyCount);
            CloseUI();
        }

        /// <summary>
        /// 구매 버튼
        /// </summary>
        void OnClickedBtnBuy()
        {
            this.data.onClickedEvent?.Invoke(PopupType.Confirm, this.buyCount);
            CloseUI();
        }

        #endregion

        private void CloseUI()
        {
            UI.Close<UIAmountBuy>();
        }

        /// <summary>
        /// 재화 업데이트 이벤트
        /// </summary>
        void AmountBuyPresenter.IView.UpdateCoin()
        {
            RefreshHaveCoin();
        }

        protected override void OnBack()
        {
            OnClickedBtnCancel();
        }

        void UpdateAddGoodsView()
        {
            if (!data.shopInfo.IsAddGoods)
            {
                popupBase.SetDimensions(popupBase.width, smallSize);
                addGoodsView.SetActive(false);
                return;
            }

            if (data.shopInfo.GetAddGoodsReward().RewardType != RewardType.Item)
            {
                popupBase.SetDimensions(popupBase.width, smallSize);
                addGoodsView.SetActive(false);
                return;
            }

            popupBase.SetDimensions(popupBase.width, fullSize);
            addGoodsView.SetActive(true);
            addGoodsView.Set(data.shopInfo);
        }
    }
}