namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIDuelArenaRank"/>
    /// </summary>
    public sealed class DuelArenaRankPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly UserModel userModel;
        private readonly RankModel rankModel;

        // <!-- Repositories --!>
        private readonly DuelArenaDataManager duelArenaDataRepo;
        private readonly DuelArenaRankDataManager duelArenaRankDataRepo;

        public event System.Action OnUpdateRank;

        public DuelArenaRankPresenter()
        {
            userModel = Entity.player.User;
            rankModel = Entity.player.RankModel;

            duelArenaDataRepo = DuelArenaDataManager.Instance;
            duelArenaRankDataRepo = DuelArenaRankDataManager.Instance;
        }

        public override void AddEvent()
        {
            rankModel.OnUpdateRankList += OnUpdateRankList;
        }

        public override void RemoveEvent()
        {
            rankModel.OnUpdateRankList -= OnUpdateRankList;

            rankModel.ClearAllRankInfos();
        }

        void OnUpdateRankList((RankType rankType, int page) tuple)
        {
            if (tuple.rankType != RankType.DuelArena)
                return;

            OnUpdateRank?.Invoke();
        }

        public int GetLastArenaNameId()
        {
            DuelArenaData find = duelArenaDataRepo.FindLast();
            return find == null ? 0 : find.NameId;
        }

        public UIRankElement.IInput[] GetRanks()
        {
            return rankModel.GetRankInfos(RankType.DuelArena);
        }

        public UIRankElement.IInput GetMyRank()
        {
            return rankModel.GetMyRankInfo(RankType.DuelArena);
        }

        public UIDuelReward.IInput[] GetRewards()
        {
            return duelArenaRankDataRepo.GetRewards();
        }

        /// <summary>
        /// 랭킹 목록 요청
        /// </summary>
        public void RequestRankList()
        {
            rankModel.ClearRankInfo(RankType.DuelArena);
            rankModel.RequestRankList(1, RankType.DuelArena).WrapNetworkErrors();
        }

        /// <summary>
        /// 다음페이지 랭킹
        /// </summary>
        public void RequestNextPage()
        {
            rankModel.RequestNextRankList(RankType.DuelArena).WrapNetworkErrors();
        }

        public void ShowOtherUserInfo(int uid, int cid)
        {
            userModel.RequestOtherCharacterInfo(uid, cid).WrapNetworkErrors();
        }
    }
}