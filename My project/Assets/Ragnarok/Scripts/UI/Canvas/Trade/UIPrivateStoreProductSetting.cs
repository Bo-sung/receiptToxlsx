using UnityEngine;


namespace Ragnarok
{
    /// <summary>
    /// 개인상점 아이템 등록, 수정, 구매 팝업
    /// </summary>
    public sealed class UIPrivateStoreProductSetting : UICanvas<PrivateStoreProductSettingPresenter>, PrivateStoreProductSettingPresenter.IView, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;
        private const int MAX_TOTAL_PRICE = 100_000_000;
        private static readonly Color MyStallColor = new Color32(77, 75, 78, 255);

        [SerializeField] UIPrivateStoreSlot item;

        [SerializeField] UILabelHelper labelTitle;

        [SerializeField] UILabelHelper labelItemName;
        [SerializeField] UILabelHelper labelDesc1; // 구매자:현재판매수량 / 판매자:아이템하한가
        [SerializeField] UILabelHelper labelOwned;
        [SerializeField] UILabelHelper labelOwnedValue;

        [SerializeField] UILabelHelper labelCountLocal;
        [SerializeField] UILabelHelper labelPriceLocal;
        [SerializeField] UILabelHelper labelTotalPriceLocal;
        [SerializeField] UILabelHelper labelRPointLocal;
        [SerializeField] UILabelHelper labelCount;
        [SerializeField] UILabelHelper labelPrice;
        [SerializeField] UILabelHelper labelTotalPrice;
        [SerializeField] UILabelHelper labelRPoint;

        [SerializeField] GameObject goCountBase;
        [SerializeField] UIButtonHelper btnCountAdd;
        [SerializeField] UIButtonHelper btnCountMax;
        [SerializeField] UIButtonHelper btnCountSub;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnRegister;

        [SerializeField] UIInput inputCount;
        [SerializeField] UIInput inputPrice;
        [SerializeField] UISprite sprPriceBackground;

        [SerializeField] UIGrid grid;
        [SerializeField] UIButtonWithIcon btnElement;

        // Data
        ItemInfo itemInfo;
        int price;
        bool isMyStore;
        int lowestPriceLimit;
        int maxPriceLimit;
        int spendRoPoint;
        int CID;
        byte index;

        PrivateStoreItemData iconProperty;
        long item_price;
        int item_count;

        private ElementType elementType;

        protected override void OnInit()
        {
            presenter = new PrivateStoreProductSettingPresenter(this);
            presenter.AddEvent();

            EventDelegate.Add(btnCountAdd.OnClick, OnClickBtnCountAdd);
            EventDelegate.Add(btnCountSub.OnClick, OnClickBtnCountSub);
            EventDelegate.Add(btnExit.OnClick, OnClickBtnExit);
            EventDelegate.Add(btnCancel.OnClick, OnClickBtnCancel);
            EventDelegate.Add(btnRegister.OnClick, OnClickBtnRegister);
            EventDelegate.Add(btnCountMax.OnClick, OnClickBtnCountMax);

            inputCount.submitOnUnselect = true;
            EventDelegate.Add(inputCount.onSubmit, OnSubmitInputCount);
            inputPrice.submitOnUnselect = true;
            EventDelegate.Add(inputPrice.onSubmit, OnSubmitInputPrice);

            if (btnElement)
            {
                EventDelegate.Add(btnElement.OnClick, OnClickedBtnElement);
            }
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnCountAdd.OnClick, OnClickBtnCountAdd);
            EventDelegate.Remove(btnCountSub.OnClick, OnClickBtnCountSub);
            EventDelegate.Remove(btnExit.OnClick, OnClickBtnExit);
            EventDelegate.Remove(btnCancel.OnClick, OnClickBtnCancel);
            EventDelegate.Remove(btnRegister.OnClick, OnClickBtnRegister);
            EventDelegate.Remove(btnCountMax.OnClick, OnClickBtnCountMax);

            EventDelegate.Remove(inputCount.onSubmit, OnSubmitInputCount);
            EventDelegate.Remove(inputPrice.onSubmit, OnSubmitInputPrice);

            if (btnElement)
            {
                EventDelegate.Remove(btnElement.OnClick, OnClickedBtnElement);
            }
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._45213; // 상품 정보
            btnCancel.LocalKey = LocalizeKey._45206; // 취소
            btnCountMax.LocalKey = LocalizeKey._45222; // MAX
            labelOwned.LocalKey = LocalizeKey._45225; // 보유
        }

        protected override void OnShow(IUIData data = null)
        {
            SelectInputPrice();
        }

        public void Show(ItemInfo itemInfo, int price, bool isMyStore, int lowestPriceLimit, int maxPriceLimit, int spendRoPoint, int CID, byte index = 0)
        {
            this.itemInfo = itemInfo;
            this.price = price;
            this.isMyStore = isMyStore;
            this.lowestPriceLimit = lowestPriceLimit;
            this.maxPriceLimit = maxPriceLimit;
            this.spendRoPoint = spendRoPoint;
            this.CID = CID;
            this.index = index;

            SetItemCount(1);
            item_price = this.price;

            if (this.isMyStore)
                inputPrice.value = this.lowestPriceLimit.ToString();
            else
                inputPrice.value = this.price.ToString();

            iconProperty = new PrivateStoreItemData(this.itemInfo);

            Refresh();
            ValueRefresh();
        }

        void Refresh()
        {
            bool isMyStore = this.isMyStore;

            int haveCount = presenter.GetItemHaveCount(itemInfo.ItemId);
            bool isCountable = itemInfo.IsStackable;
            goCountBase.SetActive(isCountable);

            long parseCount = ToNumber(inputCount.value);
            long parsePrice = ToNumber(inputPrice.value);
            labelCount.Text = parseCount.ToString();
            labelPrice.Text = parsePrice.ToString();

            labelTotalPrice.Text = (parseCount * parsePrice).ToString();
            labelRPoint.Text = spendRoPoint.ToString("N0");

            if (inputPrice.enabled != this.isMyStore)
                inputPrice.enabled = this.isMyStore;

            sprPriceBackground.spriteName = GetPriceBGSpriteName(isEditable: isMyStore);
            inputPrice.activeTextColor = isMyStore ? MyStallColor : Color.white;
            inputPrice.label.color = isMyStore ? MyStallColor : Color.white;
            inputPrice.label.effectStyle = isMyStore ? UILabel.Effect.None : UILabel.Effect.Outline8;

            item.SetData(iconProperty);

            // 로컬라이징
            labelItemName.Text = itemInfo.Name;
            string descText;
            if (isMyStore)
            {
                if (maxPriceLimit > 0)
                {
                    descText = StringBuilderPool.Get()
                        .Append(LocalizeKey._45208.ToText().Replace(ReplaceKey.VALUE, lowestPriceLimit.ToString("N0"))) // [c][FF4040]아이템 하한가 : {VALUE}[-][/c]
                        .Append("[c][FF4040] (MAX: ").Append(maxPriceLimit.ToString("N0")).Append(")[-][/c]")
                        .Release();
                }
                else
                {
                    descText = LocalizeKey._45208.ToText().Replace(ReplaceKey.VALUE, lowestPriceLimit.ToString("N0")); // [c][FF4040]아이템 하한가 : {VALUE}[-][/c]
                }
            }
            else
            {
                descText = LocalizeKey._45200.ToText().Replace(ReplaceKey.COUNT, itemInfo.ItemCount.ToString("N0")); // 현재 판매 수량 : {COUNT} 
            }
            labelDesc1.Text = descText;

            labelOwnedValue.Text = haveCount.ToString("N0");

            labelCountLocal.Text = isMyStore ?
                LocalizeKey._45209.ToText() : // 판매 수량
                LocalizeKey._45202.ToText(); // 구매 수량
            labelPriceLocal.Text = isMyStore ?
                LocalizeKey._45210.ToText() : // 판매 단가
                LocalizeKey._45203.ToText(); // 구매 단가
            labelTotalPriceLocal.Text = isMyStore ?
                LocalizeKey._45211.ToText() : // 판매 총 가격
                LocalizeKey._45204.ToText(); // 구매 총 가격
            labelRPointLocal.Text = isMyStore ?
                "" :
                LocalizeKey._45205.ToText(); // 소모 RO Point

            btnRegister.LocalKey = isMyStore ?
                LocalizeKey._45212 : // 등록
                LocalizeKey._45207; // 구매

            elementType = itemInfo.ElementType;

            ItemEquipmentSlotType slotType = itemInfo.SlotType;
            bool isShowElementIcon = slotType == ItemEquipmentSlotType.Weapon || slotType == ItemEquipmentSlotType.Armor || itemInfo.IsElementStone;

            if (btnElement)
            {
                btnElement.SetActive(isShowElementIcon);
                if (isShowElementIcon)
                {
                    elementType = itemInfo.ElementType;
                    btnElement.SetIconName(elementType.GetIconName());
                    btnElement.Text = itemInfo.GetElementLevelText();
                }
                else
                {
                    elementType = ElementType.None;
                }
            }

            grid.Reposition();
        }

        /// <summary>
        /// 가격 등의 라벨 업데이트
        /// </summary>
        void ValueRefresh()
        {
            if (item_count <= 0)
            {
                SetItemCount(1);
            }
            else if (item_count > itemInfo.ItemCount)
            {
                SetItemCount(itemInfo.ItemCount);
            }

            inputCount.Set(item_count.ToString());
            inputPrice.Set(item_price.ToString());
            labelTotalPrice.Text = (item_count * item_price).ToString("N0");
            labelRPoint.Text = spendRoPoint.ToString("N0");
        }

        void OnClickBtnCountAdd()
        {
            int tempMaxCount = isMyStore ? itemInfo.ItemCount : presenter.GetMaxCount(price, spendRoPoint, itemInfo.ItemCount);
            int maxCount = Mathf.Min(presenter.maxRegisterCount, tempMaxCount);

            int tempItemCount = item_count + 1;
            if (tempItemCount > maxCount)
            {
                SetItemCount(1);
            }
            else
            {
                SetItemCount(tempItemCount);
            }

            ValueRefresh();
        }

        void OnClickBtnCountMax()
        {
            int tempMaxCount = isMyStore ? itemInfo.ItemCount : presenter.GetMaxCount(price, spendRoPoint, itemInfo.ItemCount);
            int maxCount = Mathf.Min(presenter.maxRegisterCount, tempMaxCount);

            SetItemCount(maxCount);
            ValueRefresh();
        }

        void OnClickBtnCountSub()
        {
            int tempMaxCount = isMyStore ? itemInfo.ItemCount : presenter.GetMaxCount(price, spendRoPoint, itemInfo.ItemCount);
            int maxCount = Mathf.Min(presenter.maxRegisterCount, tempMaxCount);

            int tempItemCount = item_count - 1;
            if (tempItemCount < 1)
            {
                SetItemCount(maxCount);
            }
            else
            {
                SetItemCount(tempItemCount);
            }

            ValueRefresh();
        }

        void OnClickBtnExit()
        {
            CloseUI();
        }

        void OnClickBtnCancel()
        {
            CloseUI();
        }

        async void OnClickBtnRegister()
        {
            SetItemCount((int)ToNumber(inputCount.value));
            item_price = ToNumber(inputPrice.value);

            int itemCountValue = item_count;
            int itemPriceValue = (int)item_price;

            // 판매자의 경우 -> 아이템 등록
            if (isMyStore)
            {
                // 하한가 체크
                if (item_price < lowestPriceLimit)
                {
                    UI.ConfirmPopup(LocalizeKey._45214.ToText(), SelectInputPrice); // 하한가보다 낮게 단가를 설정 할 수 없습니다.
                    return;
                }

                // 상한가가 존재할 경우
                if (maxPriceLimit > 0)
                {
                    if (item_price > maxPriceLimit)
                    {
                        UI.ConfirmPopup(LocalizeKey._45226.ToText(), SelectInputPrice); // 상한가보다 높게 단가를 설정 할 수 없습니다.
                        return;
                    }
                }

                if (item_count * item_price > MAX_TOTAL_PRICE)
                {
                    string message = LocalizeKey._45224.ToText() // 판매 총 가격은 {MAX}을 넘길 수 없습니다. 
                        .Replace(ReplaceKey.MAX, MAX_TOTAL_PRICE.ToString("N0"));
                    UI.ConfirmPopup(message, SelectInputPrice);
                    return;
                }

                // 판매중인 경우에는 프로토콜 전송
                if (presenter.IsPrivateStoreSelling)
                {
                    PrivateStoreItemData uploadItemData = new PrivateStoreItemData(itemInfo);
                    uploadItemData.item_count = itemCountValue;
                    uploadItemData.item_price = itemPriceValue;
                    bool res = await presenter.RequestPrivateItemRegister(uploadItemData);
                    if (!res)
                        return;

                    presenter.RemoveFromInven(itemInfo.ItemNo, itemInfo.ItemId, itemCountValue); // 인벤에서 아이템 삭제
                    presenter.PrivateInvenRegister(itemInfo, itemCountValue, itemPriceValue, false); // 개인상점 인벤에 추가
                }
                else
                {
                    presenter.RemoveFromInven(itemInfo.ItemNo, itemInfo.ItemId, itemCountValue); // 인벤에서 아이템 삭제
                    presenter.PrivateInvenRegister(itemInfo, itemCountValue, itemPriceValue, true); // 개인상점 인벤에 추가
                }

                // 개인상점 Refresh
                presenter.RefreshPersonalStore();

                CloseUI();
                return;
            }

            // 구매자의 경우 -> 아이템 구매

            // RoPoint 체크
            if (!CoinType.RoPoint.Check(spendRoPoint))
                return;

            // 제니 체크
            if (!CoinType.Zeny.Check(itemPriceValue * itemCountValue))
                return;

            // 구매 경고 팝업
            string purchaseWarningMessage = itemInfo.GetPurchaseWarningMessage();
            if (!string.IsNullOrEmpty(purchaseWarningMessage))
            {
                if (!await UI.SelectPopup(purchaseWarningMessage))
                    return;
            }

            // 구매하기 프로토콜 전송
            var isSuccess = await presenter.RequestPrivateItemBuy(CID, itemInfo.ItemNo, itemInfo.ItemId, itemCountValue, index);

            // 다시 아이템 목록을 업데이트 받는다. 
            PrivateStoreData param = await Entity.player.Trade.RequestPrivateStoreProductListAsync(CID);
            UI.Show<UIPrivateStore>().Show(param);
            CloseUI();

            if (isSuccess)
            {
                UI.ConfirmPopup(LocalizeKey._5.ToText(), LocalizeKey._45219.ToText()); // 아이템을 구매 하였습니다.\n우편함에서 획득이 가능합니다.
            }
        }

        void OnSubmitInputCount()
        {
            SetItemCount((int)ToNumber(inputCount.value));
            ValueRefresh();
        }

        void OnSubmitInputPrice()
        {
            item_price = ToNumber(inputPrice.value);
            ValueRefresh();
        }

        private string GetPriceBGSpriteName(bool isEditable)
        {
            if (isEditable)
                return "Ui_Common_BG_DescLine_01";

            return "Ui_Common_BG_DescLine_03";
        }

        private long ToNumber(string text)
        {
            if (long.TryParse(text, out long result))
                return result;

            return 0L;
        }

        private void SetItemCount(int itemCount)
        {
            item_count = Mathf.Clamp(itemCount, 1, presenter.maxRegisterCount);
        }

        private void SelectInputPrice()
        {
            inputPrice.RemoveFocus();
            inputPrice.isSelected = true;
        }

        void CloseUI()
        {
            UI.Close<UIPrivateStoreProductSetting>();
        }

        void OnClickedBtnElement()
        {
            if (elementType == ElementType.None)
                return;

            UI.Show<UISelectPropertyPopup>().ShowElementView(elementType);
        }
    }
}