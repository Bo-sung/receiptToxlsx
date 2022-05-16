using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIAmountBuy"/>
    /// </summary>
    public class AmountBuyPresenter : ViewPresenter
    {
        public interface IView
        {
            void UpdateCoin();
        }

        /******************** Models ********************/
        private readonly GoodsModel goodsModel;
        private readonly InventoryModel inventoryModel;
        private readonly DungeonModel dungeonModel;
        private readonly SkillModel skillModel;
        private readonly StatusModel statusModel;
        private readonly LeagueModel leagueModel;
        private readonly SharingModel sharingModel;

        /******************** Repositories ********************/
        private readonly ItemDataManager itemDataRepo;

        /******************** Event ********************/

        private IView view;
        private ShopInfo shopInfo;

        public AmountBuyPresenter(IView view)
        {
            this.view = view;

            goodsModel = Entity.player.Goods;
            inventoryModel = Entity.player.Inventory;
            dungeonModel = Entity.player.Dungeon;
            skillModel = Entity.player.Skill;
            statusModel = Entity.player.Status;
            leagueModel = Entity.player.League;
            sharingModel = Entity.player.Sharing;
            itemDataRepo = ItemDataManager.Instance;
        }

        public override void AddEvent()
        {
            Entity.player.Goods.OnUpdateZeny += OnUpdateCoin;
            Entity.player.Goods.OnUpdateCatCoin += OnUpdateCoin;
            Entity.player.Goods.OnUpdateRoPoint += OnUpdateCoin;
            Entity.player.Goods.OnUpdateOnBuffPoint += OnUpdateOnBuffPoint;
        }

       

        public override void RemoveEvent()
        {
            Entity.player.Goods.OnUpdateZeny -= OnUpdateCoin;
            Entity.player.Goods.OnUpdateCatCoin -= OnUpdateCoin;
            Entity.player.Goods.OnUpdateRoPoint -= OnUpdateCoin;
            Entity.player.Goods.OnUpdateOnBuffPoint -= OnUpdateOnBuffPoint;
        }

        void OnUpdateCoin(long coinValue)
        {
            view.UpdateCoin();
        }

        private void OnUpdateOnBuffPoint()
        {
            view.UpdateCoin();
        }

        void OnUpdateCoin(int coinValue)
        {
            view.UpdateCoin();
        }

        public void SetData(ShopInfo shopInfo)
        {
            this.shopInfo = shopInfo;
        }

        /// <summary>
        /// 보유 재화량 반환 (지정된 코인타입)
        /// </summary>
        public long GetHaveCoin() => GetHaveCoin(this.shopInfo.CoinType);

        /// <summary>
        /// 상품 개당 가격 반환
        /// </summary>
        public int GetPrice() => this.shopInfo.Cost;

        public RewardData GetRewardData() => this.shopInfo.Reward;
        public string GetItemName() => this.shopInfo.Name;
        public string GetCoinTypeIconName() => this.shopInfo.CoinType.IconName();

        /// <summary>
        /// 보유 재화량 반환
        /// </summary>
        long GetHaveCoin(CoinType coinType)
        {
            switch (coinType)
            {
                case CoinType.CatCoin: return goodsModel.CatCoin;
                case CoinType.Zeny: return goodsModel.Zeny;
                case CoinType.GuildCoin: return goodsModel.GuildCoin;
                case CoinType.RoPoint: return goodsModel.RoPoint;
                case CoinType.OnBuffPoint: return goodsModel.OnBuffPoint;

                default:
#if UNITY_EDITOR
                    Debug.LogError($"처리되지 않은 CoinType = {coinType}");
#endif
                    break;
            }

            return 0;
        }

        /// <summary>
        /// 한번에 구매 가능한 최대 수량 반환
        /// </summary>
        public int GetMaxBuyCount()
        {
            if (shopInfo.IsBuyLimit)
            {
                return Mathf.Min(shopInfo.BuyLimitCount - shopInfo.BuyCount, BasisType.SHOP_BULK_PURCHASE_MAX_COUNT.GetInt());
            }
            return BasisType.SHOP_BULK_PURCHASE_MAX_COUNT.GetInt();
        }

        public bool IsInvalid() => this.shopInfo == null;

        /// <summary>
        /// 보상 타입에 맞는 보유량 반환
        /// </summary>
        public long GetOwnedValue()
        {
            RewardData data = GetRewardData();

            switch (data.RewardType)
            {
                case RewardType.Zeny:
                    return goodsModel.Zeny;

                case RewardType.CatCoin:
                case RewardType.CatCoinFree:
                    return goodsModel.CatCoin;

                case RewardType.SkillPoint:
                    return skillModel.SkillPoint;

                case RewardType.StatPoint:
                    return statusModel.StatPoint;

                case RewardType.Item:
                    return inventoryModel.GetItemCount(data.RewardValue);

                case RewardType.GuildCoin:
                    return goodsModel.GuildCoin;

                case RewardType.ROPoint:
                    return goodsModel.RoPoint;

                case RewardType.DefDungeonTicket:
                    return dungeonModel.GetFreeEntryCount(DungeonType.Defence);

                case RewardType.WorldBossTicket:
                    return dungeonModel.GetFreeEntryCount(DungeonType.WorldBoss);

                case RewardType.PveTicket:
                    return leagueModel.LeagueFreeTicket;

                case RewardType.CharacterShareChargeItem1:
                    return sharingModel.GetShareTicketCount(ShareTicketType.ChargeItem1);

                case RewardType.CharacterShareChargeItem2:
                    return sharingModel.GetShareTicketCount(ShareTicketType.ChargeItem2);

                case RewardType.CharacterShareChargeItem3:
                    return sharingModel.GetShareTicketCount(ShareTicketType.ChargeItem3);

                case RewardType.MultiMazeTicket:
                    return dungeonModel.GetFreeEntryCount(DungeonType.MultiMaze);

                case RewardType.SummonMvpTicket:
                    return dungeonModel.SummonMvpTicketCount;

                case RewardType.ZenyDungeonTicket:
                    return dungeonModel.GetFreeEntryCount(DungeonType.ZenyDungeon);

                case RewardType.ExpEungeonTicket:
                    return dungeonModel.GetFreeEntryCount(DungeonType.ExpDungeon);

                case RewardType.EventMultiMazeTicket:
                    return dungeonModel.GetFreeEntryCount(DungeonType.EventMultiMaze);
            }
            Debug.LogError($"잘못된 보유량 타입 요청={data.RewardType}");
            return default;
        }
    }
}