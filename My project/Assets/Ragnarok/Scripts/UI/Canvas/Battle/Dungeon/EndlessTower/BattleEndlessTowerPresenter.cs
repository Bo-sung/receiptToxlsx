namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIBattleEndlessTower"/>
    /// </summary>
    public class BattleEndlessTowerPresenter : ViewPresenter
    {
        // <!-- Repositories --!>
        private readonly EndlessTowerDataManager endlessTowerDataRepo;

        public BattleEndlessTowerPresenter()
        {
            endlessTowerDataRepo = EndlessTowerDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 보스 층 여부
        /// </summary>
        public bool IsBossFloor(int floor)
        {
            EndlessTowerData data = endlessTowerDataRepo.GetByFloor(floor);
            if (data == null)
                return false;

            return data.IsBossFloor();
        }

        /// <summary>
        /// 해당 층 보상 반환
        /// </summary>
        public RewardData[] GetCurrentRewards(int floor)
        {
            EndlessTowerData data = endlessTowerDataRepo.GetByFloor(floor);
            if (data == null)
                return null;

            return data.GetRewards();
        }
    }
}