using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <see cref="UIPrivateStore"/>
    public class PrivateStorePresenter : ViewPresenter
    {
        public interface IView
        {
            int GetCurrentCID();
            void Refresh();
            void RefreshMoney(long dummy);
            void RefreshChat();
        }

        public int CID => view.GetCurrentCID();
        public UIPersonalStoreSizeType popupSizeType;
        public UIPersonalStoreSizeType lastPopupSizeType;

        public PrivateStoreItemList RegisteredPrivateItemList => Entity.player.Trade.PrivateStoreItemList;
        public string MyPrivateStoreComment
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Entity.player.Trade.StallName))
                {
                    return LocalizeKey._45008.ToText() // {NAME}의 상점
                        .Replace(ReplaceKey.NAME, Entity.player.GetName());
                }
                return Entity.player.Trade.StallName;
            }
        }
        public bool IsMyStoreSelling => (Entity.player.Trade.SellingState == PrivateStoreSellingState.SELLING);
        public int MyCID => characterModel.Cid;
        public string MyCIDHex => characterModel.CidHex;
        public string MyName => characterModel.Name;

        private readonly IView view;
        private readonly InventoryModel inventoryModel;
        private readonly TradeModel tradeModel;
        private readonly CharacterModel characterModel;
        private readonly GoodsModel goodsModel;
        private readonly ChatModel chatModel;

        /// <summary>
        /// 보유 제니
        /// </summary>
        public long Zeny => goodsModel.Zeny;

        /// <summary>
        /// 보유 RoPoint
        /// </summary>
        public long RoPoint => goodsModel.RoPoint;

        /// <summary>
        /// 현재 열린 개인상점 창이 내 소유인가
        /// </summary>
        public bool IsMyStore => (CID == characterModel.Cid);

        /// <summary>
        /// 새로운 개인상점 채팅이 있을 경우 호출
        /// </summary>
        public event System.Action OnUpdateHasNewSendPrivateStoreChat
        {
            add { chatModel.OnUpdateHasNewSendPrivateStoreChat += value; }
            remove { chatModel.OnUpdateHasNewSendPrivateStoreChat -= value; }
        }

        /// <summary>
        /// 개인상점 상태 변경
        /// </summary>
        public event System.Action OnUpdatePrivateStoreState
        {
            add { tradeModel.OnUpdatePrivateStoreState += value; }
            remove { tradeModel.OnUpdatePrivateStoreState -= value; }
        }

        /// <summary>
        /// 개인상점 이름 변경
        /// </summary>
        public event System.Action OnUpdateStallName
        {
            add { tradeModel.OnUpdateStallName += value; }
            remove { tradeModel.OnUpdateStallName -= value; }
        }

        public PrivateStorePresenter(IView view)
        {
            this.view = view;
            inventoryModel = Entity.player.Inventory;
            tradeModel = Entity.player.Trade;
            characterModel = Entity.player.Character;
            goodsModel = Entity.player.Goods;
            chatModel = Entity.player.ChatModel;
        }

        public override void AddEvent()
        {
            popupSizeType = UIPersonalStoreSizeType.Full;
            lastPopupSizeType = UIPersonalStoreSizeType.None;

            goodsModel.OnUpdateZeny += view.RefreshMoney;
            goodsModel.OnUpdateRoPoint += view.RefreshMoney;
            chatModel.OnAddChatEvent += view.RefreshChat;
            tradeModel.OnUpdateStallItemSell += view.Refresh;
        }

        public override void RemoveEvent()
        {
            goodsModel.OnUpdateZeny -= view.RefreshMoney;
            goodsModel.OnUpdateRoPoint -= view.RefreshMoney;
            chatModel.OnAddChatEvent -= view.RefreshChat;
            tradeModel.OnUpdateStallItemSell -= view.Refresh;
        }

        /// <summary>
        /// 팝업의 사이즈를 변경한다
        /// </summary>
        /// <param name="sizeType"></param>
        public void SetPopupSize(UIPersonalStoreSizeType sizeType)
        {
            if (popupSizeType == UIPersonalStoreSizeType.FullChat || sizeType == UIPersonalStoreSizeType.FullChat)
            {
                chatModel.SetHasNewSendPrivateStoreChat(false); // 새로운 개인상점 채팅 해제
            }

            if (popupSizeType != sizeType)
            {
                lastPopupSizeType = popupSizeType;
                popupSizeType = sizeType;
            }
        }

        public void Refresh()
        {
            view.Refresh();
        }

        /// <summary>
        /// 개인상점 폐쇄 신호 송신
        /// </summary>
        public void RequestPrivateStoreClose()
        {
            tradeModel.RequestPrivateStoreClose().WrapNetworkErrors();
        }

        /// <summary>
        /// 개인 상점 개설
        /// </summary>
        /// <param name="registerList"></param>
        /// <returns></returns>
        public void RequestPrivateItemRegister()
        {
            tradeModel.RequestPrivateItemRegister(RegisteredPrivateItemList.FindAll(e => e.isVirtualRegister), MyPrivateStoreComment).WrapNetworkErrors();
        }

        /// <summary>
        /// 가상등록했던 아이템들 전부 인벤토리로 회수
        /// </summary>
        public void RestoreVirtualRegisteredItem()
        {
            // 가상등록 아이템 리스트 받아오기
            List<PrivateStoreItemData> virtualItems = tradeModel.PrivateStoreItemList.FindAll(e => e.isVirtualRegister);

            // 인벤토리로 회수
            for (int i = 0; i < virtualItems.Count; ++i)
                RestoreItemToInven(virtualItems[i]);
        }

        /// <summary>
        /// 회수 (개인상점인벤 -> 인벤)
        /// </summary>
        /// <param name="data">회수할 아이템</param>
        public void RestoreItemToInven(PrivateStoreItemData data)
        {
            inventoryModel.AddToItemList(data.ItemInfo);
            Entity.player.Trade.PrivateStoreItemList.Remove(data);
        }

        /// <summary>
        /// 판매중 회수 (개인상점인벤 -> 인벤)
        /// cud를 받아오므로 개인상점인벤에서만 제거하면 된다.
        /// </summary>
        /// <param name="data"></param>
        public async Task<bool> RestoreItemToInvenWhenSelling(PrivateStoreItemData item)
        {
            return await tradeModel.RequestPrivateItemCancel(item);
        }

        /// <summary>
        /// 개인상점 인벤에서 아이템 삭제
        /// </summary>
        /// <param name="item"></param>
        public void RemoveFromRegisteredList(PrivateStoreItemData item)
        {
            Entity.player.Trade.PrivateStoreItemList.Remove(item);
        }

        /// <summary>
        /// 개인상점 채팅 나가기 처리
        /// </summary>
        public void ExitPrivateStoreChat()
        {
            if (IsMyStore)
                return;

            chatModel.RequestPrivateStoreChatExit();
            chatModel.ClearChatData(ChatMode.PrivateStoreChat);
        }

        /// <summary>
        /// 개인상점 채팅 메시지 전송
        /// </summary>
        public void RequestSendChat(string message)
        {
            chatModel.RequestPrivateStoreChat(message);
        }

        /// <summary>
        /// 채팅 데이터 반환
        /// </summary>
        public ChatModel.ISimpleChatInput GetPrivateStoreChat(int index)
        {
            return chatModel.Get(ChatMode.PrivateStoreChat, index);
        }

        /// <summary>
        /// 채팅 기록 삭제
        /// </summary>
        public void ClearPrivateStoreChat()
        {
            chatModel.ClearChatData(ChatMode.PrivateStoreChat);
        }

        /// <summary>
        /// 채팅 개수 반환
        /// </summary>
        public int GetPrivateStoreChatSize() => chatModel.GetDataSize(ChatMode.PrivateStoreChat);

        /// <summary>
        /// 새로운 개인상점 채팅 유무
        /// </summary>
        public bool HasNewSendPrivateStoreChat => chatModel.HasNewSendPrivateStoreChat;
    }
}