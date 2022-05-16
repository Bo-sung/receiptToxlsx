using Ragnarok.View.League;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIResultLeague"/>
    /// </summary>
    public class ResultLeaguePresenter : ViewPresenter
    {
        private readonly LeagueModel leagueModel;
        private readonly BattleManager battleManager;
        private readonly PvETierDataManager leagueTiearDataRepo;
        private bool isClickedBtnConfirm;

        public event System.Action OnClose;

        public ResultLeaguePresenter()
        {
            leagueModel = Entity.player.League;
            battleManager = BattleManager.Instance;
            leagueTiearDataRepo = PvETierDataManager.Instance;
        }

        public override void AddEvent()
        {
            BattleManager.OnStart += OnPreStartBattle;
        }

        public override void RemoveEvent()
        {
            BattleManager.OnStart -= OnPreStartBattle;
        }

        void OnPreStartBattle(BattleMode mode)
        {
            if (isClickedBtnConfirm)
            {
                isClickedBtnConfirm = false;
                UI.Close<UIResultLeague>();
                UI.Show<UILeague>();
            }
        }

        public void OnClickedBtnConfirm()
        {
            isClickedBtnConfirm = true;
            battleManager.StartBattle(BattleMode.Stage); // 스토리 던전 시작
        }

        public UIResultLeagueSlot.IInput GetSlotInfo(int point)
        {
            if (leagueModel.IsSingle)
                return new ResultLeagueInfo(string.Empty, PvETierDataManager.SINGLE_GRADE_ICON_NAME, point);

            int tier = leagueTiearDataRepo.GetTier(point);
            UILeagueGradeRewardBar.IInput data = leagueTiearDataRepo.Get(tier);
            return new ResultLeagueInfo(data.GetName(), data.GetIconName(), point);
        }

        public bool IsChangeTier(int beforePoint, int afterPoint)
        {
            if (leagueModel.IsSingle)
                return false;

            int beforeTier = leagueTiearDataRepo.GetTier(beforePoint);
            int afterTier = leagueTiearDataRepo.GetTier(afterPoint);
            return beforeTier != afterTier;
        }

        private class ResultLeagueInfo : UIResultLeagueSlot.IInput
        {
            public string IconName { get; private set; }

            public string Name { get; private set; }

            public int Point { get; private set; }

            public ResultLeagueInfo(string tierName, string tierIconName, int point)
            {
                IconName = tierIconName;
                Name = tierName;
                Point = point;
            }
        }
    }
}