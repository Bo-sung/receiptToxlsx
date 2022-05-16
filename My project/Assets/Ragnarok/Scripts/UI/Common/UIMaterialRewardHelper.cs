using UnityEngine;

namespace Ragnarok
{
    public class UIMaterialRewardHelper : UIRewardHelper, IAutoInspectorFinder
    {
        private GoodsModel goodsModel;
        private SkillModel skillModel;
        private StatusModel statusModel;
        private InventoryModel invenModel;
        private CharacterModel characterModel;
        private DungeonModel dungeonModel;
        private LeagueModel leagueModel;
        private SharingModel sharingModel;

        private bool isLackAmount;
        /// <summary>
        /// 재료 부족 여부
        /// </summary>
        public bool IsLackAmount
        {
            get { return isLackAmount; }
            protected set
            {
                if (isLackAmount == value)
                    return;

                isLackAmount = value;
                OnUpdateLackAmount?.Invoke();
            }
        }

        protected int amount = 1; // 재료 수

        public event System.Action OnUpdateLackAmount;

        protected override void Awake()
        {
            base.Awake();

            goodsModel = Entity.player.Goods;
            skillModel = Entity.player.Skill;
            statusModel = Entity.player.Status;
            invenModel = Entity.player.Inventory;
            characterModel = Entity.player.Character;
            dungeonModel = Entity.player.Dungeon;
            leagueModel = Entity.player.League;
            sharingModel = Entity.player.Sharing;
        }

        protected override void AddEvent()
        {
            base.AddEvent();

            if (IsInvalid())
                return;

            switch (rewardData.RewardType)
            {
                case RewardType.Zeny:
                    goodsModel.OnUpdateZeny += UpdateView;
                    break;

                case RewardType.CatCoin:
                    goodsModel.OnUpdateCatCoin += UpdateView;
                    break;

                case RewardType.CatCoinFree:
                    goodsModel.OnUpdateCatCoin += UpdateView;
                    break;

                case RewardType.SkillPoint:
                    skillModel.OnUpdateSkillPoint += UpdateView;
                    break;

                case RewardType.StatPoint:
                    statusModel.OnUpdateStatPoint += UpdateView;
                    break;

                case RewardType.Item:
                    invenModel.AddItemEvent(rewardData.ItemId, UpdateView);
                    break;

                case RewardType.JobExp:
                    characterModel.OnUpdateJobExp += UpdateView;
                    break;

                case RewardType.LevelExp:
                    characterModel.OnUpdateLevelExp += UpdateView;
                    break;

                case RewardType.GuildCoin:
                    goodsModel.OnUpdateGuildCoin += UpdateView;
                    break;

                case RewardType.ROPoint:
                    goodsModel.OnUpdateRoPoint += UpdateView;
                    break;

                case RewardType.DefDungeonTicket:
                    dungeonModel.OnUpdateTicket += UpdateView;
                    break;

                case RewardType.WorldBossTicket:
                    dungeonModel.OnUpdateTicket += UpdateView;
                    break;

                case RewardType.PveTicket:
                    leagueModel.OnUpdateTicket += UpdateView;
                    break;

                case RewardType.StageBossTicket:
                    // 사용하지 않음
                    break;

                case RewardType.QuestCoin:
                    // 사용하지 않음
                    break;

                case RewardType.Agent:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.CharacterShareChargeItem1:
                    sharingModel.OnUpdateShareTicketCount += UpdateView;
                    break;

                case RewardType.CharacterShareChargeItem2:
                    sharingModel.OnUpdateShareTicketCount += UpdateView;
                    break;

                case RewardType.CharacterShareChargeItem3:
                    sharingModel.OnUpdateShareTicketCount += UpdateView;
                    break;

                case RewardType.MultiMazeTicket:
                    dungeonModel.OnUpdateMultiMazeTicket += UpdateView;
                    break;

                case RewardType.SummonMvpTicket:
                    dungeonModel.OnUpdateSummonMvpTicket += UpdateView;
                    break;

                case RewardType.ZenyDungeonTicket:
                    dungeonModel.OnUpdateZenyDungeonTicket += UpdateView;
                    break;

                case RewardType.ExpEungeonTicket:
                    dungeonModel.OnUpdateExpDungeonTicket += UpdateView;
                    break;

                case RewardType.EventMultiMazeTicket:
                    dungeonModel.OnUpdateEventMultiMazeTicket += UpdateView;
                    break;

                case RewardType.RefRewardGroup:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.RefGacha:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.DuelAlphabet:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.ShareReward:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.InvenWeight:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.TreeReward:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.ShareForce1:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.ShareForce2:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.ShareForce3:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.ShareForce4:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.ShareForce5:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.ShareForce6:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.PassExp:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.BattlePass:
                    // 재료로 사용되지 않음
                    break;
            }
        }

        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            if (IsInvalid())
                return;

            switch (rewardData.RewardType)
            {
                case RewardType.Zeny:
                    goodsModel.OnUpdateZeny -= UpdateView;
                    break;

                case RewardType.CatCoin:
                    goodsModel.OnUpdateCatCoin -= UpdateView;
                    break;

                case RewardType.CatCoinFree:
                    goodsModel.OnUpdateCatCoin -= UpdateView;
                    break;

                case RewardType.SkillPoint:
                    skillModel.OnUpdateSkillPoint -= UpdateView;
                    break;

                case RewardType.StatPoint:
                    statusModel.OnUpdateStatPoint -= UpdateView;
                    break;

                case RewardType.Item:
                    invenModel.RemoveItemEvent(rewardData.ItemId, UpdateView);
                    break;

                case RewardType.JobExp:
                    characterModel.OnUpdateJobExp -= UpdateView;
                    break;

                case RewardType.LevelExp:
                    characterModel.OnUpdateLevelExp -= UpdateView;
                    break;

                case RewardType.GuildCoin:
                    goodsModel.OnUpdateGuildCoin -= UpdateView;
                    break;

                case RewardType.ROPoint:
                    goodsModel.OnUpdateRoPoint -= UpdateView;
                    break;

                case RewardType.DefDungeonTicket:
                    dungeonModel.OnUpdateTicket -= UpdateView;
                    break;

                case RewardType.WorldBossTicket:
                    dungeonModel.OnUpdateTicket -= UpdateView;
                    break;

                case RewardType.PveTicket:
                    leagueModel.OnUpdateTicket -= UpdateView;
                    break;

                case RewardType.StageBossTicket:
                    // 사용하지 않음
                    break;

                case RewardType.QuestCoin:
                    // 사용하지 않음
                    break;

                case RewardType.Agent:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.CharacterShareChargeItem1:
                    sharingModel.OnUpdateShareTicketCount -= UpdateView;
                    break;

                case RewardType.CharacterShareChargeItem2:
                    sharingModel.OnUpdateShareTicketCount -= UpdateView;
                    break;

                case RewardType.CharacterShareChargeItem3:
                    sharingModel.OnUpdateShareTicketCount -= UpdateView;
                    break;

                case RewardType.MultiMazeTicket:
                    dungeonModel.OnUpdateMultiMazeTicket -= UpdateView;
                    break;

                case RewardType.SummonMvpTicket:
                    dungeonModel.OnUpdateSummonMvpTicket -= UpdateView;
                    break;

                case RewardType.ZenyDungeonTicket:
                    dungeonModel.OnUpdateZenyDungeonTicket -= UpdateView;
                    break;

                case RewardType.ExpEungeonTicket:
                    dungeonModel.OnUpdateExpDungeonTicket -= UpdateView;
                    break;

                case RewardType.EventMultiMazeTicket:
                    dungeonModel.OnUpdateEventMultiMazeTicket -= UpdateView;
                    break;

                case RewardType.RefRewardGroup:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.RefGacha:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.DuelAlphabet:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.ShareReward:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.InvenWeight:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.TreeReward:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.ShareForce1:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.ShareForce2:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.ShareForce3:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.ShareForce4:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.ShareForce5:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.ShareForce6:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.PassExp:
                    // 재료로 사용되지 않음
                    break;

                case RewardType.BattlePass:
                    // 재료로 사용되지 않음
                    break;
            }
        }

        protected override void UpdateView()
        {
            base.UpdateView();
            RefreshCount();
        }

        private void UpdateView(long input)
        {
            RefreshCount();
        }

        private void UpdateView(int input)
        {
            RefreshCount();
        }

        protected virtual void RefreshCount()
        {
            long ownValue = GetOwnValue();
            int value = GetValue() * amount;
            IsLackAmount = ownValue < value;

            if (IsLackAmount)
            {
                button.Text = StringBuilderPool.Get()
                    .Append("[c][D76251]").Append(ownValue).Append("[-][/c]")
                    .Append("/")
                    .Append(value).Release();
            }
            else
            {
                button.Text = StringBuilderPool.Get()
                    .Append(ownValue)
                    .Append("/")
                    .Append(value).Release();
            }
        }

        protected long GetOwnValue()
        {
            if (IsInvalid())
                return 0;

            switch (rewardData.RewardType)
            {
                case RewardType.Zeny:
                    return goodsModel.Zeny;

                case RewardType.CatCoin:
                    return goodsModel.CatCoin;

                case RewardType.CatCoinFree:
                    return goodsModel.CatCoin;

                case RewardType.SkillPoint:
                    return skillModel.SkillPoint;

                case RewardType.StatPoint:
                    return statusModel.StatPoint;

                case RewardType.Item:
                    return invenModel.GetItemCount(rewardData.ItemId);

                case RewardType.JobExp:
                    return (int)characterModel.JobLevelExp;

                case RewardType.LevelExp:
                    return characterModel.LevelExp;

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

                case RewardType.StageBossTicket:
                    return 0; // 사용하지 않음

                case RewardType.QuestCoin:
                    return 0; // 사용하지 않음

                case RewardType.Agent:
                    return 0; // 재료로 사용되지 않음

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

                case RewardType.RefRewardGroup:
                    return 0; // 재료로 사용되지 않음

                case RewardType.RefGacha:
                    return 0; // 재료로 사용되지 않음

                case RewardType.DuelAlphabet:
                    return 0; // 재료로 사용되지 않음

                case RewardType.ShareReward:
                    return 0; // 재료로 사용되지 않음

                case RewardType.InvenWeight:
                    return 0; // 재료로 사용되지 않음

                case RewardType.TreeReward:
                    return 0; // 재료로 사용되지 않음

                case RewardType.ShareForce1:
                    return 0; // 재료로 사용되지 않음

                case RewardType.ShareForce2:
                    return 0; // 재료로 사용되지 않음

                case RewardType.ShareForce3:
                    return 0; // 재료로 사용되지 않음

                case RewardType.ShareForce4:
                    return 0; // 재료로 사용되지 않음

                case RewardType.ShareForce5:
                    return 0; // 재료로 사용되지 않음

                case RewardType.ShareForce6:
                    return 0; // 재료로 사용되지 않음

                case RewardType.PassExp:
                    return 0; // 재료로 사용되지 않음

                case RewardType.BattlePass:
                    return 0; // 재료로 사용되지 않음
            }

            Debug.LogError($"작업하지 않은 RewardType = {rewardData.RewardType}");
            return 0;
        }

        protected int GetValue()
        {
            if (IsInvalid())
                return 0;

            return rewardData.Count;
        }

        public void SetAmount(int value, int min = 1)
        {
            amount = Mathf.Max(min, value);
            RefreshCount();
        }

        public int GetMaxAmount(int min = 1)
        {
            long ownValue = GetOwnValue();
            int value = GetValue();
            return (int)Mathf.Max(min, ownValue / value);
        }
    }
}