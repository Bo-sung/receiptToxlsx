namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIAdventureRanking"/>
    /// </summary>
    public sealed class AdventureRankingPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly RankModel rankModel;
        private readonly UserModel userModel;

        // <!-- Event --!>
        public event System.Action<(RankType rankType, int page)> OnUpdateRankList
        {
            add { rankModel.OnUpdateRankList += value; }
            remove { rankModel.OnUpdateRankList -= value; }
        }

        public AdventureRankingPresenter()
        {
            rankModel = Entity.player.RankModel;
            userModel = Entity.player.User;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
            rankModel.ClearAllRankInfos();
        }

        public AdventureRankingElement.IInput[] GetArrayInfo(RankType rankType)
        {
            return rankModel.GetRankInfos(rankType);
        }

        public AdventureRankingElement.IInput GetMyInfo(RankType rankType)
        {
            return rankModel.GetMyRankInfo(rankType);
        }

        /// <summary>
        /// 랭킹 목록 요청
        /// </summary>
        public void RequestRankList(RankType rankType)
        {
            rankModel.ClearRankInfo(rankType);
            rankModel.RequestRankList(1, rankType).WrapNetworkErrors();
        }

        /// <summary>
        /// 다음페이지 랭킹
        /// </summary>
        public void RequestNextPage(RankType rankType)
        {
            rankModel.RequestNextRankList(rankType).WrapNetworkErrors();
        }

        /// <summary>
        /// 다른 유저 정보 보기
        /// </summary>
        public void RequestOtherCharacterInfo(int uid, int cid)
        {
            userModel.RequestOtherCharacterInfo(uid, cid).WrapNetworkErrors();
        }
    }
}