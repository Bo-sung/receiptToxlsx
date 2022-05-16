namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIBattleFreeFight"/>
    /// </summary>
    public class BattleFreeFightPresenter : ViewPresenter
    {
        // <!-- Repositories --!>
        private readonly FreeFightRewardDataManager freeFightRewardDataRepo;

        private FreeFightEventType freeFightEventType;

        public BattleFreeFightPresenter()
        {
            freeFightRewardDataRepo = FreeFightRewardDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public RewardData[] GetCurrentRewards(int killCount)
        {
            return freeFightRewardDataRepo.GetRewards(freeFightEventType, killCount);
        }

        public int GetNextRemainKillCount(int killCount)
        {
            return freeFightRewardDataRepo.GetNextRemainKillCount(freeFightEventType, killCount);
        }

        public void SetFreeFightEventType(FreeFightEventType type)
        {
            freeFightEventType = type;
        }
    }
}