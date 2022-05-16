namespace Ragnarok
{
    /// <summary>
    /// <see cref="UITimePatrol"/>
    /// </summary>
    public class TimePatrolPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly DungeonModel dungeonModel;

        // <!-- Repositories --!>
        private readonly TimePatrolStageDataManager timePatrolStageDataRepo;
        private readonly BattleManager battleManager;
        private readonly BetterList<RewardElement> rewards;

        private bool isStartBattle;

        public TimePatrolPresenter()
        {
            dungeonModel = Entity.player.Dungeon;
            timePatrolStageDataRepo = TimePatrolStageDataManager.Instance;
            battleManager = BattleManager.Instance;
            rewards = new BetterList<RewardElement>();
            var normalRewardList = BasisType.TIME_PATROL_NORMAL_MONSTER_REWARD.GetKeyList();
            for (int i = 0; i < normalRewardList.Count; i++)
            {
                int itemId = BasisType.TIME_PATROL_NORMAL_MONSTER_REWARD.GetInt(normalRewardList[i]);
                rewards.Add(new RewardElement(itemId));
            }
        }

        public override void AddEvent()
        {
            BattleManager.OnStart += OnStartBattle;
        }

        public override void RemoveEvent()
        {
            BattleManager.OnStart -= OnStartBattle;
        }

        void OnStartBattle(BattleMode mode)
        {
            if (!isStartBattle)
                return;

            UI.Close<UITimePatrol>();
            UI.Close<UIAdventureMazeSelect>();
        }

        /// <summary>
        /// 도달한 타임패트롤 레벨
        /// </summary>
        public int GetFinalTimePatrolLevel()
        {
            return dungeonModel.FinalTimePatrolLevel;
        }

        /// <summary>
        /// 마지막으로 입장한 타임패트롤 ID
        /// </summary>
        public int GetLastEnterTimePatrolId()
        {
            return dungeonModel.LastEnterTimePatrolId;
        }

        /// <summary>
        /// 마지막으로 입장한 타임패트롤 레벨
        /// </summary>
        /// <returns></returns>
        public int GetLastEnterTimePatrolLevel()
        {
            var data = timePatrolStageDataRepo.Get(GetLastEnterTimePatrolId());
            if (data != null)
                return data.level;

            return 1;
        }

        public int GetPatrolZone(int level)
        {
            if (level == GetLastEnterTimePatrolLevel())
            {
                var data = timePatrolStageDataRepo.Get(GetLastEnterTimePatrolId());
                if (data != null)
                    return data.zone_id;
            }
            return 0;
        }

        public void StartTimePatrol(int level)
        {
            if (level == GetLastEnterTimePatrolLevel())
            {
                if (battleManager.LastAutoBattleMode == BattleMode.TimePatrol)
                {
                    // 진행중인 타임패트롤
                    UI.ShowToastPopup(LocalizeKey._48237.ToText()); // 현재 진행중인 던전입니다.
                    return;
                }
                // 마지막 진입한 타임패트롤 진입
                battleManager.StartBattle(BattleMode.TimePatrol);
                isStartBattle = true;
            }
            else
            {
                // 다른 레벨의 타임패트롤 진입
                var data = timePatrolStageDataRepo.Get(level, 1);
                if (data != null)
                {
                    battleManager.StartBattle(BattleMode.TimePatrol, data.id);
                    isStartBattle = true;
                }
            }
        }

        public UIRewardListElement.IInput[] GetRewards()
        {
            return rewards.ToArray();
        }

        private class RewardElement : UIRewardListElement.IInput
        {
            public RewardData Reward { get; }

            public int Rate => 0;

            public int TotalRate => 0;

            public RewardElement(int itemId)
            {
                Reward = new RewardData(RewardType.Item, itemId, 1);
            }          

            public void SetTotalRate(int totalRate)
            {
            }
        }
    }
}