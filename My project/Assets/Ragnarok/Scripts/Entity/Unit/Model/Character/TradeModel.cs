using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class TradeModel : CharacterEntityModel
    {
        public interface IInputValue
        {
            string PrivateStoreComment { get; }
            PrivateStoreSellingState PrivateStoreSellingState { get; }
        }

        private PrivateStoreSellingState sellingState;
        /// <summary> 개인상점 개설 상태 </summary>
        public PrivateStoreSellingState SellingState
        {
            get => sellingState;
            set
            {
                if (sellingState == value)
                    return;

                sellingState = value;
                OnUpdatePrivateStoreState?.Invoke();

                if (sellingState == PrivateStoreSellingState.SELLING)
                {
                    UI.ShowToastPopup(LocalizeKey._45220.ToText()); // 물건 판매를 시작하였습니다.
                }
                else
                {
                    UI.ShowToastPopup(LocalizeKey._45221.ToText()); // 물건 판매를 종료했습니다.
                }
            }
        }

        /// <summary> 노점 이름 </summary>
        public string StallName { get; private set; }    

        /// <summary> 개인상점에 등록한 내 아이템 리스트 </summary>
        public PrivateStoreItemList PrivateStoreItemList { get; private set; }

        /// <summary> 거래소에 등록한 내 아이템 리스트 </summary> 
        public AuctionItemList AuctionItemList { get; private set; }

        /// <summary> 최근의 거래소 아이템 검색결과 리스트 </summary>
        public AuctionItemList AuctionSearchItemList { get; private set; }

        /// <summary> 내 거래소 등록 가능 수 </summary>
        public int tradeShopRegisterLimit;       

        /// <summary>
        /// 노점 상태 변경시 호출
        /// </summary>
        public event System.Action OnUpdatePrivateStoreState;

        /// <summary>
        /// 다른 유저가 내 노점 아이템 구매 했을시 호출 
        /// </summary>
        public event System.Action OnUpdateStallItemSell;

        /// <summary>
        /// 노점 이름 변경시 이벤트 호출
        /// </summary>
        public event System.Action OnUpdateStallName;

        public TradeModel()
        {
            PrivateStoreItemList = new PrivateStoreItemList();
            AuctionItemList = new AuctionItemList();
            AuctionSearchItemList = new AuctionItemList();
        }

        public override void AddEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                // 다른 플레이어가 내 개인 상점 상품을 구매했을 때 수신 이벤트 추가.
                Protocol.RESPONSE_TRADEPRIVATE_SELL.AddEvent(OnReceiveOtherPlayerBuyMine);
            }
        }
        public override void RemoveEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                // 다른 플레이어가 내 개인 상점 상품을 구매했을 때 수신 이벤트 제거.
                Protocol.RESPONSE_TRADEPRIVATE_SELL.RemoveEvent(OnReceiveOtherPlayerBuyMine);
            }
        }

        internal void Initialize(IInputValue inputValue)
        {
            SetStallName(inputValue.PrivateStoreComment);
            sellingState = inputValue.PrivateStoreSellingState;
        }

        internal void UpdateData(PrivateStoreSellingState sellingState, string privateStoreComment)
        {
            SetStallName(privateStoreComment);
            this.sellingState = sellingState;
            OnUpdatePrivateStoreState?.Invoke();
        }

        /// <summary>
        /// 거래소 등록 물품 로드
        /// </summary>
        /// <param name="tradeShopTradeItemPacket"></param>
        internal void Initialize(AuctionItemPacket auctionItemPacket)
        {
            AuctionItemList.LoadRegisteredItemList(auctionItemPacket);
        }

        /// <summary>
        /// 개인상점 최근 물품 로드
        /// </summary>
        internal void Initialize(PrivateStoreItemPacket[] tradeShopPrivateItems)
        {
            PrivateStoreItemList.LoadRegisteredItemList(tradeShopPrivateItems);
        }       

        /// <summary>
        /// 노점 이름 세팅
        /// </summary>
        /// <param name="stallName"></param>
        public void SetStallName(string stallName)
        {
            if (!string.IsNullOrEmpty(StallName) && StallName.Equals(stallName))
                return;

            StallName = stallName;
            OnUpdateStallName?.Invoke();
        }

        /// <summary>
        /// 개인상점 귓말
        /// </summary>
        /// <param name="targetCID">귓속말을 받을 대상의 CID</param>
        public async void RequestChat_PrivateWhisper(string msg, int targetCID)
        {
            var param = Protocol.NewInstance();
            param.PutUtfString("1", msg);
            param.PutInt("2", targetCID);

            var response = await Protocol.REQUEST_TRADEPRIVATE_WHISPER.SendAsync(param);
            if (response.isSuccess)
            {
                // TODO: 채팅 UI에 내가 보낸 귓말 추가.
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 개인상점 아이템 등록
        /// </summary>
        public async Task<bool> RequestPrivateItemRegister(List<PrivateStoreItemData> regItemList, string stallName)
        {
            int regCount = regItemList != null ? regItemList.Count : 0;          

            var sfs = Protocol.NewInstance();
            // 등록할 아이템 개수
            sfs.PutInt("1", regCount);

            // 노점 이름
            if (!string.IsNullOrEmpty(stallName))
                sfs.PutUtfString("2", stallName);

            for (int i = 0; i < regCount; ++i)
            {
                PrivateStoreItemData thisItem = regItemList[i];
                sfs.PutLong((3 + i * 4).ToString(), thisItem.item_No);
                sfs.PutInt((4 + i * 4).ToString(), thisItem.itemData.id);
                sfs.PutInt((5 + i * 4).ToString(), thisItem.item_count);
                sfs.PutInt((6 + i * 4).ToString(), thisItem.item_price);
            }

            var response = await Protocol.REQUEST_TRADEPRIVATE_REGISTER.SendAsync(sfs);

            if(!response.isSuccess)
            {
                response.ShowResultCode();
                return false;
            }

            foreach (var item in regItemList.OrEmptyIfNull())
            {
                item.isVirtualRegister = false;
            }

            // 노점 이름
            if (response.ContainsKey("2"))
            {
                stallName = response.GetUtfString("2");
            }

            // 수신
            UpdateData(PrivateStoreSellingState.SELLING, stallName);

            // 인구수, 노점 수 변경 이벤트
            UI.GetUI<UITown>()?.RefreshChannelView();

            return true;
        }

        /// <summary>
        /// 다른 유저의 개인상점 아이템 구매
        /// </summary>
        /// <param name="cid">해당 유저의 CID</param>
        public async Task<bool> RequestPrivateItemBuy(int cid, long itemNo, int itemId, int buyCount = 1, byte index = 0)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", cid);
            sfs.PutLong("2", itemNo);
            sfs.PutInt("3", itemId);
            sfs.PutInt("4", buyCount);
            sfs.PutByte("5", index);

            var response = await Protocol.REQUEST_TRADEPRIVATE_BUY.SendAsync(sfs);
            if (response.isSuccess)
            {
                NotifyAddAlarmMailTrade();
                long buyedItemNo = response.GetLong("1");
                long buyedItemCount = response.GetInt("2");

                // cud. 캐릭터 업데이트 데이터
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);

                // 토스트 팝업
                ItemData boughtItemData = ItemDataManager.Instance.Get(itemId);
                string desc = LocalizeKey._90076.ToText() // {NAME} x{COUNT} 아이템을 구매했습니다.
                    .Replace(ReplaceKey.NAME, boughtItemData.name_id.ToText())
                    .Replace(ReplaceKey.COUNT, (int)buyedItemCount);
                UI.ShowRewardToast(boughtItemData.icon_name, desc);
            }
            else
            {
                if (response.resultCode == ResultCode.TRADEPRIVATE_BUY_FAIL  // 다른유저가 구매한 상품이거나 등록취소한 아이템
                    || response.resultCode == ResultCode.PTMARKET_NOREGITEM) // 상품 정보가 변경된경우 (등록취소 후 재둥록한 아이템)
                {
                    UI.ConfirmPopup(LocalizeKey._471.ToText()); // 개인거래 구매에 실패하였습니다.
                }               
                else
                {
                    response.ShowResultCode();
                }
            }

            return response.isSuccess;
        }               

        /// <summary>
        /// 노점 판매중 회수.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RequestPrivateItemCancel(PrivateStoreItemData item)
        {
            var param = Protocol.NewInstance();
            param.PutLong("1", item.item_No);
            param.PutInt("2", item.itemData.id);

            var response = await Protocol.REQUEST_TRADEPRIVATE_REGCANCELONE.SendAsync(param);
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }
            else
            {
                response.ShowResultCode();
            }

            return response.isSuccess;
        }

        /// <summary>
        /// 내 개인상점 판매 종료 요청
        /// </summary>
        /// <returns></returns>
        public async Task RequestPrivateStoreClose()
        {
            // 판매를 종료 하시겠습니까?
            if (!await UI.SelectPopup(string.Empty, LocalizeKey._45006.ToText()))
                return;            

            var response = await Protocol.REQUEST_TRADEPRIVATE_END.SendAsync();

            if(!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            // 노점 판매 종료
            UpdateData(PrivateStoreSellingState.NOT_SELLING, string.Empty);            
        }        

        /// <summary>
        /// 다른 유저의 개인상점 품목 조회 요청
        /// (itemList, storeCID, CIDHex, nickName, storeComment)
        /// </summary>
        public async Task<PrivateStoreData> RequestPrivateStoreProductListAsync(int CID)
        {
            var param = Protocol.NewInstance();
            param.PutInt("1", CID);

            var response = await Protocol.REQUEST_TRADEPRIVATE_REGLIST.SendAsync(param);
            if (response.isSuccess)
            {
                var resCID = response.GetInt("1");
                var destPlayer = FindPlayer(resCID);
                if (destPlayer == null)
                {
                    Debug.LogError($"REQUEST_TRADEPRIVATE_REGLIST 프로토콜 수신 에러. {resCID}번 유저가 존재하지 않는데 개인상점 품목이 수신됨.");
                    return default;
                }

                var dataList = response.GetSFSArray("2");
                string nickName = destPlayer.GetName();
                string storeComment = destPlayer.Trade.StallName;
                int storeCID = resCID;
                string CIDHex = MathUtils.CidToHexCode(resCID);
                PrivateStoreItemList itemList = new PrivateStoreItemList();
                for (int i = 0; i < dataList.Count; ++i)
                {
                    var data = dataList.GetSFSObject(i);
                    var resItemNo = data.GetLong("1");
                    var resCID2 = data.GetInt("2");
                    var resItemID = data.GetInt("3");
                    var resItemCount = data.GetInt("4");
                    var resItemTierPer = data.GetInt("5");
                    var resItemLevel = data.GetByte("6");
                    var resItemPos = data.GetByte("7");
                    resItemPos = 0; /// TODO: Pos는 항상 0이어야 한다.
                    var resCardID1 = data.GetLong("8");
                    var resCardID2 = data.GetLong("9");
                    var resCardID3 = data.GetLong("10");
                    var resCardID4 = data.GetLong("11");
                    var resPrice = data.GetLong("12");
                    var resTranscend = data.GetInt("13");
                    var resElementChange = data.ContainsKey("14") ? data.GetInt("14") : 0;
                    byte index = data.GetByte("15"); // 노점 상품 고유 인덱스
                    int resElementLevel = data.GetInt("16");

                    PrivateStoreItemData privateItemData = new PrivateStoreItemData(resItemID);
                    privateItemData.SetInfo(resItemNo, resItemCount, (int)resPrice, resItemTierPer, resItemLevel, resItemPos.ToEnum<ItemEquipmentSlotType>(), resCardID1, resCardID2, resCardID3, resCardID4, resTranscend, resElementChange, resElementLevel);
                    privateItemData.SetIndex(index);
                    itemList.Add(privateItemData);
                }

                return new PrivateStoreData(itemList, storeCID, CIDHex, nickName, storeComment);
            }
            else
            {
                response.ShowResultCode();
                return default;
            }
        }

        List<CharacterEntity> GetAllPlayers()
        {
            List<CharacterEntity> playerList = new List<CharacterEntity>();
            BattleUnitList unitList = BattleManager.Instance.unitList;
            for (int i = 0; i < unitList.size; ++i)
            {
                UnitEntity unit = unitList[i];
                if (unit.type == UnitEntityType.MultiPlayer || unit.type == UnitEntityType.Player)
                {
                    playerList.Add(unit as CharacterEntity);
                }
            }
            return playerList;
        }

        /// <summary>
        /// 특정 플레이어 찾기
        /// </summary>
        CharacterEntity FindPlayer(int CID)
        {
            return GetAllPlayers().Find(e => e.Character.Cid.Equals(CID));
        }

        /// <summary>
        /// 로비 입장
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public async Task<Response> RequestEnterChannelAsync(int channelId)
        {
            var param = Protocol.NewInstance();
            param.PutInt("1", channelId);

            var response = await Protocol.REQUEST_TRADEPRIVATE_ENTERROOM.SendAsync(param);
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                // 셰어캐릭터 이용 가능 남은 시간 동기화
                if (response.ContainsKey("99"))
                {
                    int shareRemainTime = response.GetInt("99");
                    Entity.Sharing.SetRemainTimeForShare(shareRemainTime);
                }
            }
            else
            {
                response.ShowResultCode();
            }

            return response;
        }        

        #region Response Event

        /// <summary>      
        /// 다른 유저가 내 노점 아이템을 구매했을때 호출된다.
        /// </summary>
        void OnReceiveOtherPlayerBuyMine(Response response)
        {
            long itemNo = response.GetLong("1");
            int sellCount = response.GetInt("2");

            if (SellingState == PrivateStoreSellingState.NOT_SELLING)
            {
                Debug.LogError($"RESPONSE_TRADEPRIVATE_SELL 프로토콜 수신 에러. 개인상점 판매중이 아닌데 {itemNo}가 팔렸다.");
                return;
            }

            PrivateStoreItemData sellItem = PrivateStoreItemList.Find((PrivateStoreItemData e) => e.item_No == itemNo);
            if (sellItem == null)
            {
                Debug.LogError($"RESPONSE_TRADEPRIVATE_SELL 프로토콜 수신 에러. {itemNo}가 팔렸는데 내가 올린 품목에는 {itemNo}가 없다.");
                return;
            }

            NotifyAddAlarmMailTrade();

            if (sellItem.item_count > sellCount)
                sellItem.item_count -= (short)sellCount;
            else
                PrivateStoreItemList.Remove(sellItem);                

            // cud. 캐릭터 업데이트 데이터 -> 메일로 보상이 오기 때문에 cud가 오지 않는 듯 하다.
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }

            // 토스트 팝업
            string desc = LocalizeKey._90075.ToText() // {NAME} x{COUNT} 아이템이 판매되었습니다.
                .Replace(ReplaceKey.NAME, sellItem.itemData.name_id.ToText())
                .Replace(ReplaceKey.COUNT, sellCount);
            UI.ShowRewardToast(sellItem.itemData.icon_name, desc);

            OnUpdateStallItemSell?.Invoke();
        }

        #endregion

        /// <summary>
        /// 우편함 거래탭 빨간점 표시
        /// </summary>
        private void NotifyAddAlarmMailTrade()
        {
            NotifyAddAlarm(AlarmType.MailTrade); // 우편함 거래탭 빨간점 표시
        }
    }
}