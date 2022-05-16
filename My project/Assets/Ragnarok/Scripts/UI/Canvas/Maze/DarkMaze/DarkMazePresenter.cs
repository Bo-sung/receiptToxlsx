namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIDarkMaze"/>
    /// </summary>
    public sealed class DarkMazePresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly DungeonModel dungeonModel;

        // <!-- Repositories --!>
        private readonly RewardData dailyReward;
        private readonly RewardData clearReward;

        // <!-- Temps --!>
        private int mazeId;

        public DarkMazePresenter()
        {
            dungeonModel = Entity.player.Dungeon;

            int dailyRewardItemId = BasisDarkMazeInfo.DailyRewardItemId.GetInt();
            int clearRewardZeny = BasisDarkMazeInfo.ClearRewardZeny.GetInt();
            dailyReward = new RewardData(RewardType.Item, dailyRewardItemId, 1);
            clearReward = new RewardData(RewardType.Zeny, clearRewardZeny, 0);
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public RewardData GetDailyReward()
        {
            return dailyReward;
        }

        public RewardData GetClearReward()
        {
            return clearReward;
        }

        public bool GetIsReceivedDailyReward()
        {
            return dungeonModel.IsEnteredEventDarkMaze;
        }

        /// <summary>
        /// 현재 미로 아이디 저장
        /// </summary>
        public void SetMazeId(int mazeId)
        {
            this.mazeId = mazeId;
        }

        /// <summary>
        /// 이벤트 미로 시작
        /// </summary>
        public void StartEventMaze()
        {
            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return;
            }

            CloseUI();
            UI.Show<UIBattleMatchReady>(new UIBattleMatchReady.Input { id = mazeId });
        }

        private void CloseUI()
        {
            UI.Close<UIDarkMaze>();
        }
    }
}