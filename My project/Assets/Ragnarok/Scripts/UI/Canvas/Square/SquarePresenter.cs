namespace Ragnarok
{
    /// <summary>
    /// <see cref="UISquare"/>
    /// </summary>
    public class SquarePresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly GoodsModel goodsModel;
        private readonly InventoryModel inventoryModel;
        private readonly QuestModel questModel;

        // <!-- Repositories --!>
        private readonly ConnectionManager connectionManager;
        private readonly BattleManager battleManager;

        // <!-- Event --!>
        public event System.Action<long> OnUpdateZeny
        {
            add { goodsModel.OnUpdateZeny += value; }
            remove { goodsModel.OnUpdateZeny -= value; }
        }

        public event System.Action<long> OnUpdateCatCoin
        {
            add { goodsModel.OnUpdateCatCoin += value; }
            remove { goodsModel.OnUpdateCatCoin -= value; }
        }

        public event System.Action OnUpdateKafra
        {
            add { questModel.OnUpdateKafra += value; }
            remove { questModel.OnUpdateKafra -= value; }
        }

        public event System.Action OnUpateNabiho
        {
            add { inventoryModel.OnUpdateNabiho += value; }
            remove { inventoryModel.OnUpdateNabiho -= value; }
        }

        public SquarePresenter()
        {
            goodsModel = Entity.player.Goods;
            inventoryModel = Entity.player.Inventory;
            questModel = Entity.player.Quest;
            connectionManager = ConnectionManager.Instance;
            battleManager = BattleManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void RemoveNewOpenContent()
        {
            questModel.RemoveNewOpenContent(ContentType.TradeTown); // 신규 컨텐츠 플래그 제거 (거래소)
        }

        public void OnClickedBtnLobby()
        {
            battleManager.StartBattle(BattleMode.Lobby);
        }

        public void OnClickedBtnKafraDelivery()
        {
            battleManager.StartBattle(BattleMode.Lobby, LobbyEntry.PostAction.MoveToNpcSorin);
        }

        public void OnClickedBtnExchangeShop()
        {
            battleManager.StartBattle(BattleMode.Lobby, LobbyEntry.PostAction.MoveToNpcTailing);
        }

        public void OnClickedBtnNabiho()
        {
            UI.Show<UINabiho>();
        }

        public bool IsOpenNabiho()
        {
            return BasisType.NABIHO_OPEN_BY_SERVER.GetInt(connectionManager.GetSelectServerGroupId()) == 0;
        }

        public bool IsNoticeNabiho()
        {
            if (!IsOpenNabiho())
                return false;

            return inventoryModel.IsNoticeNobiho();
        }

        public bool IsNoticeKafraDelivery()
        {
            return questModel.KafraCompleteType != KafraCompleteType.None;
        }
    }
}