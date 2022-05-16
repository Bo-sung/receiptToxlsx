using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuildRank"/>
    /// </summary>
    public sealed class GuildRankPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
        }

        private readonly IView view;
        private readonly RankModel rankModel;

        private RankType rankType;
        private bool hasMyGuildRank;
        private int mainTitleLocalKey = LocalizeKey._33035; // 길드 랭킹
        private int contentLocalKey = LocalizeKey._33096; // 누적 경험치

        public GuildRankPresenter(IView view)
        {
            this.view = view;
            rankModel = Entity.player.RankModel;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void SelectGuildType(UIGuildRank.GuildRankType guildRankType)
        {
            rankType = ConvertToRankType(guildRankType);
            hasMyGuildRank = GetHasMyGuildRank(guildRankType);
            mainTitleLocalKey = GetMainTitleLocalKey(guildRankType);
            contentLocalKey = GetContentLocalKey(guildRankType);
        }

        public int GetMainTitleLocalKey()
        {
            return mainTitleLocalKey;
        }

        public int GetContentLocalKey()
        {
            return contentLocalKey;
        }

        public bool HasNextPage()
        {
            return rankModel.HasNextPage(rankType);
        }

        /// <summary>
        /// 길드 랭킹 정보 리스트
        /// </summary>
        public RankInfo[] GetRankInfos()
        {
            return rankModel.GetRankInfos(rankType);
        }

        /// <summary>
        /// 내 길드 랭킹 정보
        /// </summary>
        public RankInfo GetMyRankInfo()
        {
            return rankModel.GetMyRankInfo(rankType);
        }

        /// <summary>
        /// 길드 랭킹 요청 (호출)
        /// </summary>
        public async Task RequestGuildRank()
        {
            rankModel.ClearRankInfo(rankType);
            await rankModel.RequestRankList(1, rankType, hasMyGuildRank);
            view.Refresh();
        }

        /// <summary>
        /// 다음페이지 랭킹
        /// </summary>
        public async void RequestNextPage()
        {
            await rankModel.RequestNextRankList(rankType, hasMyGuildRank);
            view.Refresh();
        }

        /// <summary>
        /// 내 길드 랭킹 존재 여부
        /// </summary>
        public bool HasMyGuildRank()
        {
            return hasMyGuildRank;
        }

        /// <summary>
        /// 내 길드 존재 여부
        /// </summary>
        private bool GetHasMyGuildRank(UIGuildRank.GuildRankType guildRankType)
        {
            if (guildRankType == UIGuildRank.GuildRankType.NoGuild)
                return false;

            return true;
        }

        /// <summary>
        /// 랭크타입 반환
        /// </summary>
        private RankType ConvertToRankType(UIGuildRank.GuildRankType guildRankType)
        {
            switch (guildRankType)
            {
                default:
                case UIGuildRank.GuildRankType.HasGuild:
                case UIGuildRank.GuildRankType.NoGuild:
                    return RankType.Guild;

                case UIGuildRank.GuildRankType.GuildBattle:
                    return RankType.GuildBattle;
            }
        }

        /// <summary>
        /// 타이틀 LocalKey 반환
        /// </summary>
        private int GetMainTitleLocalKey(UIGuildRank.GuildRankType guildRankType)
        {
            switch (guildRankType)
            {
                default:
                case UIGuildRank.GuildRankType.HasGuild:
                case UIGuildRank.GuildRankType.NoGuild:
                    return LocalizeKey._33035; // 길드 랭킹

                case UIGuildRank.GuildRankType.GuildBattle:
                    return LocalizeKey._33137; // 길드전 랭킹
            }
        }

        /// <summary>
        /// 내용 LocalKey 반환
        /// </summary>
        private int GetContentLocalKey(UIGuildRank.GuildRankType guildRankType)
        {
            switch (guildRankType)
            {
                default:
                case UIGuildRank.GuildRankType.HasGuild:
                case UIGuildRank.GuildRankType.NoGuild:
                    return LocalizeKey._33096; // 누적 경험치

                case UIGuildRank.GuildRankType.GuildBattle:
                    return LocalizeKey._33138; // 누적 피해량
            }
        }
    }
}