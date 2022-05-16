namespace Ragnarok
{
    public class GoodsInfoPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly GoodsModel goodsModel;
        private readonly SharingModel sharingModel;
        private readonly DungeonModel dungeonModel;
        private readonly SkillModel skillModel;

        // <!-- Repositories --!>

        // <!-- Event --!>

        public GoodsInfoPresenter()
        {
            goodsModel = Entity.player.Goods;
            sharingModel = Entity.player.Sharing;
            dungeonModel = Entity.player.Dungeon;
            skillModel = Entity.player.Skill;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 보유 수량 표시 가능한 타입 여부
        /// </summary>
        /// <param name="rewardType"></param>
        /// <returns></returns>
        public bool IsShowOwned(RewardType rewardType)
        {
            switch (rewardType)
            {
                case RewardType.Zeny:
                case RewardType.CatCoin:
                case RewardType.CatCoinFree:
                case RewardType.ROPoint:
                case RewardType.CharacterShareChargeItem1:
                case RewardType.CharacterShareChargeItem2:
                case RewardType.CharacterShareChargeItem3:
                case RewardType.SummonMvpTicket:
                case RewardType.GuildCoin:
                case RewardType.SkillPoint:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 보유 수량
        /// </summary>
        /// <param name="rewardType"></param>
        /// <returns></returns>
        public long GetOwnedValue(RewardType rewardType)
        {
            switch (rewardType)
            {
                case RewardType.Zeny:
                    return goodsModel.Zeny;

                case RewardType.CatCoin:
                case RewardType.CatCoinFree:
                    return goodsModel.CatCoin;

                case RewardType.ROPoint:
                    return goodsModel.RoPoint;

                case RewardType.CharacterShareChargeItem1:
                    return sharingModel.GetShareTicketCount(ShareTicketType.ChargeItem1);

                case RewardType.CharacterShareChargeItem2:
                    return sharingModel.GetShareTicketCount(ShareTicketType.ChargeItem2);

                case RewardType.CharacterShareChargeItem3:
                    return sharingModel.GetShareTicketCount(ShareTicketType.ChargeItem3);

                case RewardType.SummonMvpTicket:
                    return dungeonModel.SummonMvpTicketCount;

                case RewardType.GuildCoin:
                    return goodsModel.GuildCoin;

                case RewardType.SkillPoint:
                    return skillModel.SkillPoint;
            }
            return 0;
        }
    }
}