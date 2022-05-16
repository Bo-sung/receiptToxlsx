namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuildBattleRankEvent"/>
    /// </summary>
    public class GuildBattleRankEventPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly RankModel rankModel;
        private readonly GuildModel guildModel;

        // <!-- Event --!>
        public event System.Action OnRankList;

        public GuildBattleRankEventPresenter()
        {
            rankModel = Entity.player.RankModel;
            guildModel = Entity.player.Guild;
        }

        public override void AddEvent()
        {
            rankModel.OnUpdateRankList += OnUpdateRankList;
        }

        public override void RemoveEvent()
        {
            rankModel.OnUpdateRankList -= OnUpdateRankList;
        }

        void OnUpdateRankList((RankType rankType, int page) info)
        {
            if (info.rankType != RankType.EventGuildBattle)
                return;

            OnRankList?.Invoke();
        }

        /// <summary>
        /// 내 길드 랭킹 존재 여부
        /// </summary>
        public bool HasMyGuildRank()
        {
            return guildModel.HaveGuild;
        }

        /// <summary>
        /// 길드전 랭킹 요청 (호출)
        /// </summary>
        public void RequestGuildRank()
        {
            rankModel.ClearRankInfo(RankType.EventGuildBattle);
            rankModel.RequestRankList(1, RankType.EventGuildBattle, HasMyGuildRank()).WrapNetworkErrors();
        }

        /// <summary>
        /// 다음페이지 랭킹
        /// </summary>
        public void RequestNextPage()
        {
            rankModel.RequestNextRankList(RankType.EventGuildBattle, HasMyGuildRank()).WrapNetworkErrors();
        }

        /// <summary>
        /// 길드전 랭킹 정보 리스트
        /// </summary>
        public UIGuildBattleRankElement.IInput[] GetRankInfos()
        {
            return rankModel.GetRankInfos(RankType.EventGuildBattle);
        }

        public UIGuildBattleRankElement.IInput GetMyRankInfo()
        {
            return rankModel.GetMyRankInfo(RankType.EventGuildBattle);
        }
    }
}