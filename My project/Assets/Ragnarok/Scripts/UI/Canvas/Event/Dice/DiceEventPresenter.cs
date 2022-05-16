using Ragnarok.View;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIDiceEvent"/>
    /// </summary>
    public sealed class DiceEventPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly EventModel eventModel;
        private readonly InventoryModel inventoryModel;

        // <!-- Repositories --!>
        private readonly GachaDataManager gachaDataRepo;
        private readonly DiceRewardDataManager diceRewardDataRepo;
        private readonly DiceDataManager diceDataRepo;
        public readonly int needCostCount; // 필요코인 수
        private readonly int costItemId; // 필요코인 아이템 id
        public readonly PartsItemInfo costItemInfo; // 필요코인 재료정보
        public readonly int eventCoinMaxCount;

        // <!-- Event --!>
        public event System.Action OnUpdateCostItemCount; // 재화 아이템 개수 변경

        /// <summary>
        /// 주사위 굴리기 이벤트
        /// </summary>
        public event EventModel.DiceRollEvent OnUpdateDiceRollEvent
        {
            add { eventModel.OnUpdateDiceRollEvent += value; }
            remove { eventModel.OnUpdateDiceRollEvent -= value; }
        }

        /// <summary>
        /// 완주 보상 업데이트
        /// </summary>
        public event System.Action OnUpdateDiceCompleteRewardEvent
        {
            add { eventModel.OnUpdateDiceCompleteRewardEvent += value; }
            remove { eventModel.OnUpdateDiceCompleteRewardEvent -= value; }
        }

        public DiceEventPresenter()
        {
            eventModel = Entity.player.Event;
            inventoryModel = Entity.player.Inventory;
            gachaDataRepo = GachaDataManager.Instance;
            diceRewardDataRepo = DiceRewardDataManager.Instance;
            diceDataRepo = DiceDataManager.Instance;

            needCostCount = BasisType.DICE_EVENT_NEED_POINT.GetInt();
            costItemInfo = new PartsItemInfo();
            costItemId = BasisItem.EventCoin.GetID();
            eventCoinMaxCount = BasisType.EVENT_COIN_MAX_COUNT.GetInt();

            ItemData costItemData = ItemDataManager.Instance.Get(costItemId);
            costItemInfo.SetData(costItemData);
        }

        public override void AddEvent()
        {
            inventoryModel.AddItemEvent(costItemId, OnUpdateCostItem);
            eventModel.OnDiceCompleteRewardEvent += OnDiceCompleteRewardEvent;
        }

        public override void RemoveEvent()
        {
            inventoryModel.RemoveItemEvent(costItemId, OnUpdateCostItem);
            eventModel.OnDiceCompleteRewardEvent -= OnDiceCompleteRewardEvent;
        }

        void OnUpdateCostItem()
        {
            OnUpdateCostItemCount?.Invoke();
        }

        void OnDiceCompleteRewardEvent(RewardData[] rewards)
        {
            if (rewards == null || rewards.Length == 0)
                return;

            RewardData reward = rewards[0];
            var input = new UISingleReward.Input(UISingleReward.Mode.DICE_COMPLETE_REWARD, reward, reward.IconName);
            UI.Show<UISingleReward>(input);
        }

        /// <summary>
        /// 완주 보상 목록 정보
        /// </summary>
        public UIDiceCompleteElement.IInput[] GetCompleteRewards()
        {
            return diceRewardDataRepo.GetRewards();
        }

        /// <summary>
        /// 이벤트 이름 목록 정보
        /// </summary>
        public string[] GetEventImageNames()
        {
            return diceDataRepo.GetImageNames();
        }

        /// <summary>
        /// 주화 코인 수
        /// </summary>
        public int GetCostItemCount()
        {
            return inventoryModel.GetItemCount(costItemId);
        }

        /// <summary>
        /// 주사위 보드 데이터
        /// </summary>
        public UIMonopolyTile.IInput[] GetData()
        {
            if (eventModel.EventDiceGroupId == 0)
                return null;

            return gachaDataRepo.Gets(GroupType.DiceEvent, eventModel.EventDiceGroupId);
        }

        /// <summary>
        /// 이벤트 데이터 반환
        /// </summary>
        public UIDiceEventResult.IInput GetDiceEventData(int eventId)
        {
            if (eventId == 0)
                return null;

            return diceDataRepo.Get(eventId);
        }

        /// <summary>
        /// 주사위 위치
        /// </summary>
        public int GetDiceStep()
        {
            return eventModel.EventDiceStep;
        }

        /// <summary>
        /// 더블 상태
        /// </summary>
        public bool IsDiceDoubleState()
        {
            return eventModel.IsEventDiceDouble;
        }

        /// <summary>
        /// 완주 횟수
        /// </summary>
        public int GetCompleteCount()
        {
            return eventModel.EventDiceCompleteCount;
        }

        /// <summary>
        /// 주사위게임 보상수령한 회차
        /// </summary>
        public int GetCompleteRewardStep()
        {
            return eventModel.EventDiceCompleteRewardStep;
        }

        /// <summary>
        /// 주사위 굴리기
        /// </summary>
        public void RequestRoll()
        {
            // 더블 상태가 아니고, 코인이 부족할 경우
            if (!IsDiceDoubleState() && GetCostItemCount() < needCostCount)
            {
                UI.ShowToastPopup(LocalizeKey._11307.ToText()); // 코인이 부족합니다.
                return;
            }

            eventModel.RequestDiceRoll().WrapNetworkErrors();
        }

        /// <summary>
        /// 주사위 완주 보상
        /// </summary>
        public void RequestDiceReward()
        {
            eventModel.RequestDiceReward().WrapNetworkErrors();
        }
    }
}