namespace Ragnarok
{
    public abstract class BaseDungeonPresenter : ViewPresenter, IDungeonImpl
    {
        protected readonly StageDataManager stageDataRepo;
        protected readonly DungeonModel dungeonModel;
        protected readonly BattleManager battleManager;
        protected readonly MonsterDataManager monsterDataRepo;
        protected readonly ScenarioMazeDataManager scenarioMazeDataRepo;

        public BaseDungeonPresenter()
        {
            stageDataRepo = StageDataManager.Instance;
            dungeonModel = Entity.player.Dungeon;
            battleManager = BattleManager.Instance;
            monsterDataRepo = MonsterDataManager.Instance;
            scenarioMazeDataRepo = ScenarioMazeDataManager.Instance;
        }

        /// <summary>
        /// 던전 오픈 여부
        /// </summary>
        public bool IsOpend(DungeonType dungeonType)
        {
            return dungeonModel.IsOpened(dungeonType, isShowPopup: false);
        }

        /// <summary>
        /// 던전 오픈 조건 텍스트 반환
        /// </summary>
        public string GetOpenConditionalSimpleText(IOpenConditional openConditional)
        {
            return dungeonModel.GetOpenConditionalSimpleText(openConditional);
        }

        /// <summary>
        /// 던전 무료입장 남은 횟수
        /// </summary>
        public int GetFreeEntryCount(DungeonType dungeonType)
        {
            return dungeonModel.GetFreeEntryCount(dungeonType);
        }

        /// <summary>
        /// 던전 무료입장 최대 횟수
        /// </summary>       
        public int GetFreeEntryMaxCount(DungeonType dungeonType)
        {
            return dungeonModel.GetFreeEntryMaxCount(dungeonType);
        }

        /// <summary>
        /// 현재 던전 플레이 진행 여부
        /// </summary>
        public bool IsCurrentDungeonPlaying()
        {
            return battleManager.Mode != BattleMode.Stage;
        }

        /// <summary>
        /// 던전 무료보상 수령가능
        /// </summary>
        public bool PossibleFreeReward(DungeonType dungeonType)
        {
            return dungeonModel.PossibleFreeReward(dungeonType);
        }
    }
}