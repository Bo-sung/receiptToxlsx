namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIBattleDuel"/>
    /// </summary>
    public class BattleDuelPresenter : ViewPresenter
    {
        // <!-- Repositories --!>
        private readonly PvETierDataManager leagueTiearDataRepo;

        public BattleDuelPresenter()
        {
            leagueTiearDataRepo = PvETierDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        //public string GetTierName(int point)
        //{
        //    int tier = leagueTiearDataRepo.GetTier(point);
        //    PvETierData data = leagueTiearDataRepo.Get(tier);
        //    return data == null ? string.Empty : data.GetName();
        //}

        public string GetTierIconName(int point)
        {
            int tier = leagueTiearDataRepo.GetTier(point);
            PvETierData data = leagueTiearDataRepo.Get(tier);
            return data == null ? string.Empty : data.GetIconName();
        }
    }
}