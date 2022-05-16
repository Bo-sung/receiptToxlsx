namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIRpsEvent"/>
    /// </summary>
    public sealed class RpsEventPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly EventModel eventModel;
        private readonly InventoryModel inventoryModel;

        // <!-- Repositories --!>
        private readonly EventRpsDataManager eventRpsDataRepo;
        private readonly ItemDataManager itemDataRepo;
        private readonly int costItemId;
        public readonly int eventCoinMaxCount;
        private readonly PartsItemInfo costItemInfo; // 필요코인 재료정보

        // <!-- Event --!>
        public event System.Action OnUpdateRpsInfo
        {
            add { eventModel.OnUpdateRpsInfo += value; }
            remove { eventModel.OnUpdateRpsInfo -= value; }
        }

        public RpsEventPresenter()
        {
            eventModel = Entity.player.Event;
            inventoryModel = Entity.player.Inventory;
            eventRpsDataRepo = EventRpsDataManager.Instance;
            itemDataRepo = ItemDataManager.Instance;

            costItemId = BasisItem.EventCoin.GetID();
            eventCoinMaxCount = BasisType.EVENT_COIN_MAX_COUNT.GetInt();

            costItemInfo = new PartsItemInfo();
            ItemData costItemData = itemDataRepo.Get(costItemId);
            costItemInfo.SetData(costItemData);
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public RpsRoundType GetRoundType()
        {
            return eventModel.EventRpsRound;
        }

        public RpsResultType GetResultType()
        {
            return eventModel.EventRpsResult;
        }

        public EventRpsData GetData(RpsRoundType type)
        {
            return eventRpsDataRepo.Get(type);
        }

        public RewardData[] GetRewardDatas()
        {
            return eventRpsDataRepo.GetRewardDatas();
        }

        public string GetCostItemImage()
        {
            return itemDataRepo.Get(costItemId).icon_name;
        }

        public int GetCostItemCount()
        {
            return inventoryModel.GetItemCount(costItemId);
        }

        public void RequestEventRpsStart()
        {
            eventModel.RequestEventRpsStart().WrapNetworkErrors();
        }

        public void RequestEventRpsInit()
        {
            eventModel.RequestEventRpsInit().WrapNetworkErrors();
        }

        public void ShowCostItemData()
        {
            UI.Show<UIPartsInfo>(costItemInfo);
        }
    }
}