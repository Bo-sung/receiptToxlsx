using UnityEngine;

namespace Ragnarok
{
    public class UIPrivateStore : UICanvas<PrivateStorePresenter>, PrivateStorePresenter.IView, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIWidget widgetPopupBase;
        [SerializeField] UIWidget widgetContent;
        [SerializeField] UIWidget widgetStoreList;
        [SerializeField] UIPanel widgetScrollView;
        [SerializeField] Transform widgetWrapper;

        [SerializeField] GameObject goChatBase;
        [SerializeField] UIWidget widgetBackground;

        [SerializeField] UILabelHelper labelStoreTitle; // 홍길동의 상점
        [SerializeField] UIButtonHelper btnStoreTitleEdit; // 상점이름 변경 버튼 
        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] UILabelHelper labelStoreState; // 판매중 (11/12) -> [4C4A4D]판매중[-] [7CAAE0](11/12)[-]
        [SerializeField] UILabelHelper labelSellerInfo; // 판매자 정보 라벨 (구매자에게만 보임)
        [SerializeField] GameObject goSellerInfo; // 판매자 정보 베이스
        [SerializeField] UILabelHelper labelSellerInfo_Chat; // 판매자 정보 라벨(챗모드) (구매자에게만 보임)
        [SerializeField] UIButtonHelper btnManage; // 판매시작/판매종료 버튼

        [SerializeField] SuperScrollListWrapper wrapper_chat;
        [SerializeField] GameObject prefab_chat;
        [SerializeField] UIButtonHelper btnChatModeChange;
        [SerializeField] UIInput inputChat;
        [SerializeField] UIButtonHelper btnChatSend;
        [SerializeField] UIButtonHelper btnSizeChange;
        [SerializeField] UISprite iconSizeChange;

        [SerializeField] UIButtonHelper btnZeny;
        [SerializeField] UIButtonHelper btnRoPoint;

        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;

        float[] constPopupBase_Y;
        int[] constPopupBase_H;
        float[] constContent_Y;
        float[] constStoreList_Y;
        int[] constStoreList_H;
        float[] constScrollView_Y;
        int[] constScrollView_H;
        float[] constWrapper_Y;
        float[] constBackground_Y;
        int[] constBackground_H;

        PrivateStoreData data;

        public bool IsMyStoreSelling => presenter.IsMyStoreSelling;

        protected override void OnInit()
        {
            presenter = new PrivateStorePresenter(this);

            presenter.AddEvent();

            constPopupBase_Y = new float[3] { -12f, -12f, -217f };
            constPopupBase_H = new int[3] { 1012, 1012, 602 };
            constContent_Y = new float[3] { -6f, -6f, 199f };
            constStoreList_Y = new float[3] { 7f, -9f + 221f, -200f };
            constStoreList_H = new int[3] { 748, 338, 342 };
            constScrollView_Y = new float[3] { 2f, -221f, -1f };
            constScrollView_H = new int[3] { 752, 342, 342 };
            constWrapper_Y = new float[3] { 264f, 61f + 221f, 62f };
            constBackground_Y = new float[3] { 390f, 390f, -20f };
            constBackground_H = new int[3] { 770, 360, 360 };

            EventDelegate.Add(btnChatModeChange.OnClick, OnClickBtnChat);
            EventDelegate.Add(btnChatSend.OnClick, OnClickBtnChatSend);
            EventDelegate.Add(btnSizeChange.OnClick, OnClickBtnSizeChange);
            EventDelegate.Add(btnManage.OnClick, OnClickBtnManage);
            EventDelegate.Add(btnClose.OnClick, OnClickBtnClose);
            EventDelegate.Add(btnStoreTitleEdit.OnClick, OnClickBtnTitleEdit);
            EventDelegate.Add(btnZeny.OnClick, OnClickedBtnZeny);
            EventDelegate.Add(btnRoPoint.OnClick, OnClickBtnRoPoint);

            wrapper.SpawnNewList(prefab, 0, 0);
            wrapper.SetRefreshCallback(OnElementRefresh);
            wrapper.Resize((Constants.Trade.PRIVATE_STORE_MAX_UPLOAD_COUNT - 1) / Constants.Trade.PRIVATE_STORE_PRODUCT_COUNT_PER_LINE + 1);

            wrapper_chat.SpawnNewList(prefab_chat, 0, 0);
            wrapper_chat.SetRefreshCallback(OnElementRefresh_Chat);

            presenter.OnUpdateHasNewSendPrivateStoreChat += UpdateNewStoreChat;
            presenter.OnUpdatePrivateStoreState += Refresh;
            presenter.OnUpdateStallName += Refresh;
        }

        protected override void OnClose()
        {
            presenter.OnUpdateHasNewSendPrivateStoreChat -= UpdateNewStoreChat;
            presenter.OnUpdatePrivateStoreState -= Refresh;
            presenter.OnUpdateStallName -= Refresh;

            presenter.RemoveEvent();

            EventDelegate.Remove(btnChatModeChange.OnClick, OnClickBtnChat);
            EventDelegate.Remove(btnChatSend.OnClick, OnClickBtnChatSend);
            EventDelegate.Remove(btnSizeChange.OnClick, OnClickBtnSizeChange);
            EventDelegate.Remove(btnManage.OnClick, OnClickBtnManage);
            EventDelegate.Remove(btnClose.OnClick, OnClickBtnClose);
            EventDelegate.Remove(btnStoreTitleEdit.OnClick, OnClickBtnTitleEdit);
            EventDelegate.Remove(btnZeny.OnClick, OnClickedBtnZeny);
            EventDelegate.Remove(btnRoPoint.OnClick, OnClickBtnRoPoint);

            presenter.RestoreVirtualRegisteredItem();
            if (!presenter.IsMyStoreSelling)
            {
                presenter.ExitPrivateStoreChat();
                presenter.ClearPrivateStoreChat();
            }
        }

        protected override void OnHide()
        {
            if (data is null)
                return;

            presenter.RestoreVirtualRegisteredItem();
            if (!presenter.IsMyStoreSelling)
            {
                presenter.ExitPrivateStoreChat();
                presenter.ClearPrivateStoreChat();
            }

            data = null;
        }

        protected override void OnLocalize()
        {
            btnChatSend.LocalKey = LocalizeKey._45009; // 전송
        }

        protected override void OnShow(IUIData data = null)
        {
            UpdateNewStoreChat();
        }

        public void ShowMine()
        {
            Show(new PrivateStoreData(presenter.RegisteredPrivateItemList,
                presenter.MyCID,
                presenter.MyCIDHex,
                presenter.MyName,
                presenter.MyPrivateStoreComment));
        }

        public void Show(PrivateStoreData storeData)
        {
            data = new PrivateStoreData();
            data.itemList = storeData.itemList;
            data.CID = storeData.CID;
            data.CIDHex = storeData.CIDHex;
            data.nickName = storeData.nickName;
            data.storeComment = storeData.storeComment;

            if (presenter.IsMyStoreSelling && !presenter.IsMyStore)
            {
                CloseUI();
                return;
            }

            SetPopupSize(UIPersonalStoreSizeType.Full);
            RefreshZeny();
            RefreshRoPoint();

            if (!presenter.IsMyStore)
            {
                presenter.ClearPrivateStoreChat();
            }
            wrapper_chat.Resize(presenter.GetPrivateStoreChatSize());
            wrapper_chat.SetProgress(1f);
        }


        void OnElementRefresh(GameObject go, int index)
        {
            if (data is null)
                return;

            var slot = go.GetComponent<UIPrivateStoreSlotHelper>();
            for (int i = 0; i < Constants.Trade.PRIVATE_STORE_PRODUCT_COUNT_PER_LINE; ++i)
            {
                int thisIndex = index * Constants.Trade.PRIVATE_STORE_PRODUCT_COUNT_PER_LINE + i;
                slot.SetData(i, presenter, thisIndex < data.itemList.Count ? data.itemList[thisIndex] : null);
            }
        }

        void OnElementRefresh_Chat(GameObject go, int index)
        {
            var slot = go.GetComponent<PrivateStoreChatSlot>();
            slot.SetData(presenter, presenter.GetPrivateStoreChat(index));
        }

        /// <summary>
        /// 팝업의 사이즈를 변경한다
        /// </summary>
        /// <param name="sizeType"></param>
        void SetPopupSize(UIPersonalStoreSizeType sizeType)
        {
            presenter.SetPopupSize(sizeType);
            ApplyPopupSize();
        }

        /// <summary>
        /// 팝업 사이즈 업데이트
        /// </summary>
        void ApplyPopupSize()
        {
            int popupSizeTypeIndex = presenter.popupSizeType.ToIntValue();
            int lastPopupSizeTypeIndex = presenter.lastPopupSizeType.ToIntValue();


            //  Popup Size 변경
            SetLocalPositionY(widgetPopupBase.cachedTransform, constPopupBase_Y[popupSizeTypeIndex]);
            widgetPopupBase.height = constPopupBase_H[popupSizeTypeIndex];

            SetLocalPositionY(widgetContent.cachedTransform, constContent_Y[popupSizeTypeIndex]);

            SetLocalPositionY(widgetStoreList.cachedTransform, constStoreList_Y[popupSizeTypeIndex]);
            widgetStoreList.height = constStoreList_H[popupSizeTypeIndex];

            Vector4 region = widgetScrollView.baseClipRegion;
            region.w = constScrollView_H[popupSizeTypeIndex];
            widgetScrollView.baseClipRegion = region;

            SetLocalPositionY(widgetWrapper, constWrapper_Y[popupSizeTypeIndex]);

            if (presenter.lastPopupSizeType != UIPersonalStoreSizeType.None)
                AddScroll(widgetScrollView, constScrollView_Y[popupSizeTypeIndex] - constScrollView_Y[lastPopupSizeTypeIndex]);


            SetLocalPositionY(widgetBackground.cachedTransform, constBackground_Y[popupSizeTypeIndex]);
            widgetBackground.height = constBackground_H[popupSizeTypeIndex];

            wrapper.SetProgress(0f);

            Refresh();
        }

        public void Refresh()
        {
            if (!gameObject.activeSelf)
                return;

            wrapper.RefreshAllItems();

            bool isChat = presenter.popupSizeType == UIPersonalStoreSizeType.FullChat;
            // 임시 정보 변수
            bool isMyStore = presenter.IsMyStore;
            bool isSelling = isMyStore ? presenter.IsMyStoreSelling : true;
            string storeTitle = isMyStore ? presenter.MyPrivateStoreComment : data.storeComment;
            string sellerNickname = data.nickName;
            string sellerCIDHex = data.CIDHex;

            if (goChatBase.activeSelf != isChat)
                goChatBase.SetActive(isChat);

            labelStoreTitle.Text = storeTitle;

            // 상점 타이틀 변경 버튼 액티브
            bool canEditTitle = isMyStore;
            if (btnStoreTitleEdit.gameObject.activeSelf != canEditTitle)
                btnStoreTitleEdit.SetActive(canEditTitle);

            labelStoreState.Text = (isSelling ?
                LocalizeKey._45000.ToText() : // [4C4A4D]판매 중[-] [7CAAE0]({COUNT}/{MAX})[-]
                LocalizeKey._45001.ToText()) //  [4C4A4D]판매 준비 중[-] [7CAAE0]({COUNT}/{MAX})[-]
                .Replace(ReplaceKey.COUNT, data.itemList.Count)
                .Replace(ReplaceKey.MAX, Constants.Trade.PRIVATE_STORE_MAX_UPLOAD_COUNT);

            labelSellerInfo_Chat.Text =
            labelSellerInfo.Text = LocalizeKey._45002.ToText() // [4C4A4D]판매자 : [-][7CAAE0]{NAME}({VALUE})[-]
                .Replace(ReplaceKey.NAME, sellerNickname)
                .Replace(ReplaceKey.VALUE, sellerCIDHex);

            btnManage.LocalKey = isSelling ?
                LocalizeKey._45003 : // 판매 종료
                LocalizeKey._45004; // 판매 시작

            if (goSellerInfo.gameObject.activeSelf != (!isMyStore && !isChat))
                goSellerInfo.SetActive(!isMyStore && !isChat);

            if (btnManage.gameObject.activeSelf != (isMyStore && !isChat))
                btnManage.SetActive((isMyStore && !isChat));

            bool toExpand = presenter.popupSizeType == UIPersonalStoreSizeType.Small;
            iconSizeChange.spriteName = GetSizeChangeButtonIconName(toExpand);
            btnSizeChange.SpriteName = GetSizeChangeButtonBackgroundName(!isChat);
            UpdateNewStoreChat();

            btnChatModeChange.SpriteName = GetChatButtonBackgroundName(isChat);
            btnChatModeChange.gameObject.SetActive(!presenter.IsMyStore || IsMyStoreSelling);
        }

        /// <summary>
        /// 스크롤뷰를 일정 값만큼 즉시 스크롤해주는 함수.
        /// </summary>
        /// <param name="scrollValue">value만큼 아래로 스크롤</param>
        void AddScroll(UIPanel scrollView, float scrollValue)
        {
            Vector2 p = scrollView.clipOffset;
            p.y -= scrollValue;
            scrollView.clipOffset = p;
            SetLocalPositionY(scrollView.cachedTransform, scrollView.cachedTransform.localPosition.y + scrollValue);
        }

        void SetLocalPositionY(Transform tf, float y)
        {
            Vector3 pos = tf.localPosition;
            pos.y = y;
            tf.localPosition = pos;
        }

        void OnClickBtnChat()
        {
            if (data is null)
                return;

            if (presenter.popupSizeType != UIPersonalStoreSizeType.FullChat)
            {
                SetPopupSize(UIPersonalStoreSizeType.FullChat);
                wrapper_chat.Resize(presenter.GetPrivateStoreChatSize());
                wrapper_chat.SetProgress(1f);
            }
            else
                SetPopupSize(UIPersonalStoreSizeType.Full);
        }

        void OnClickBtnSizeChange()
        {
            if (presenter.popupSizeType == UIPersonalStoreSizeType.Full)
                SetPopupSize(UIPersonalStoreSizeType.Small);
            else if (presenter.popupSizeType == UIPersonalStoreSizeType.Small)
                SetPopupSize(UIPersonalStoreSizeType.Full);
        }

        void OnClickBtnChatSend()
        {
            presenter.RequestSendChat(inputChat.value);
            inputChat.value = string.Empty;
        }

        /// <summary>
        /// 상점 개설/폐쇄 버튼
        /// </summary>
        void OnClickBtnManage()
        {
            // 개설/폐쇄 프로토콜 전송.
            bool isSelling = presenter.IsMyStoreSelling;

            if (isSelling)
            {
                // 판매 종료 프로토콜
                presenter.RequestPrivateStoreClose();
            }
            else
            {
                // 상점 개설
                presenter.RequestPrivateItemRegister();
            }
        }

        /// <summary>
        /// 개인상점 제목 변경 버튼
        /// </summary>
        void OnClickBtnTitleEdit()
        {
            UI.Show<UIPrivateStoreNameEdit>();
        }

        void OnClickBtnClose()
        {
            CloseUI();
        }

        void OnClickedBtnZeny()
        {
            UI.ShowZenyShop();
        }

        void OnClickBtnRoPoint()
        {
            UI.ShowRoPointShop();
        }

        string GetSizeChangeButtonIconName(bool toExpand)
        {
            if (toExpand)
                return "Ui_Common_Icon_Expand";
            return "Ui_Common_Icon_Contract";
        }

        string GetSizeChangeButtonBackgroundName(bool active)
        {
            if (active)
                return "Ui_Common_Btn_05";
            return "Ui_Common_Btn_06";
        }

        string GetChatButtonBackgroundName(bool isBright)
        {
            if (isBright)
                return "Ui_Common_Btn_04";
            return "Ui_Common_Btn_05";
        }

        void CloseUI()
        {
            Hide();
        }

        private void RefreshZeny()
        {
            btnZeny.Text = presenter.Zeny.ToString("N0");
        }

        private void RefreshRoPoint()
        {
            btnRoPoint.Text = presenter.RoPoint.ToString("N0");
        }    

        void PrivateStorePresenter.IView.RefreshMoney(long dummy)
        {
            RefreshZeny();
            RefreshRoPoint();
        }

        void PrivateStorePresenter.IView.Refresh()
        {
            Refresh();
        }

        public int GetCurrentCID()
        {
            return data != null ? data.CID : default;
        }

        void PrivateStorePresenter.IView.RefreshChat()
        {
            wrapper_chat.Resize(presenter.GetPrivateStoreChatSize());
            wrapper_chat.SetProgress(1f);
        }

        void UpdateNewStoreChat()
        {
            bool isCollapsed = presenter.popupSizeType != UIPersonalStoreSizeType.FullChat;
            btnChatModeChange.SetNotice(isCollapsed && presenter.HasNewSendPrivateStoreChat);
        }
    }


    /// <summary>
    /// 개인상점 메인 팝업의 사이즈 타입
    /// </summary>
    public enum UIPersonalStoreSizeType
    {
        Full = 0,       // 큰 팝업
        FullChat = 1,   // 큰 팝업 + 채팅창
        Small = 2,      // 작은 팝업
        None,
    }
}