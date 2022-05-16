using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuildMain"/>
    /// </summary>
    public class GuildMainPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
            void SetGuildCoin(long value);
            void SetCatCoin(long value);
        }

        private readonly IView view;
        private readonly GuildModel guild;
        private readonly GoodsModel goodsModel;
        private readonly BattleManager battleManager;
        private readonly GuildSquareManager guildSquareManager;
        private readonly ShopModel shopModel;

        public event System.Action OnTamingMazeOpen;

        public event System.Action OnPurchaseSuccess
        {
            add { shopModel.OnPurchaseSuccess += value; }
            remove { shopModel.OnPurchaseSuccess -= value; }
        }

        public event System.Action OnResetFreeItemBuyCount
        {
            add { shopModel.OnResetFreeItemBuyCount += value; }
            remove { shopModel.OnResetFreeItemBuyCount -= value; }
        }

        public GuildMainPresenter(IView view)
        {
            this.view = view;
            guild = Entity.player.Guild;
            goodsModel = Entity.player.Goods;
            shopModel = Entity.player.ShopModel;
            battleManager = BattleManager.Instance;
            guildSquareManager = GuildSquareManager.Instance;
        }

        public override void AddEvent()
        {
            goodsModel.OnUpdateGuildCoin += view.SetGuildCoin;
            goodsModel.OnUpdateCatCoin += view.SetCatCoin;
            guild.OnUpdateGuildSkill += view.Refresh;
            guild.OnUpdateGuildPoision += view.Refresh;
            guild.OnUpdateGuildMemberCount += view.Refresh;
            guild.OnTamingMazeOpen += InvokeUpdateTamingMazeOpen;
        }

        public override void RemoveEvent()
        {
            goodsModel.OnUpdateGuildCoin -= view.SetGuildCoin;
            goodsModel.OnUpdateCatCoin -= view.SetCatCoin;
            guild.OnUpdateGuildSkill -= view.Refresh;
            guild.OnUpdateGuildPoision -= view.Refresh;
            guild.OnUpdateGuildMemberCount -= view.Refresh;
            guild.OnTamingMazeOpen -= InvokeUpdateTamingMazeOpen;
        }

        public byte MemberCount => guild.MemberCount;
        public byte OnlineMemberCount => guild.OnlineMemberCount;
        public long GuildCoin => goodsModel.GuildCoin;
        public RemainTime GuildSkillBuyTime => guild.GuildSkillBuyTime;
        public byte GuildSkillBuyCount => guild.GuildSkillBuyCount;
        public bool IsBuyCashGuildSkill => guild.GuildSkillBuyCount < BasisType.GUILD_SKILL_BUY_EXP_CAT_COIN_MAX_CNT.GetInt();
        public bool IsBuyCoinGuildSkill => GuildSkillBuyTime.ToRemainTime() <= 0;
        public long CatCoin => goodsModel.CatCoin;
        /// <summary>
        /// 길드 가입요청한 인원수
        /// </summary>
        public int GuildJoinSubmitUserCount => guild.GuildJoinSubmitUserCount;

        public bool IsNeedGuildLevelUp(int needGuildLevel)
        {
            return guild.GuildLevel < needGuildLevel;
        }

        /// <summary>
        /// 길드원 가입신청 정보 보여주기 유무
        /// </summary>
        public bool IsShowJoinMemberList()
        {
            return guild.GuildPosition == GuildPosition.Master || guild.GuildPosition == GuildPosition.PartMaster;
        }

        /// <summary>
        /// 길드 가입요청한 유저 목록
        /// </summary>
        public GuildJoinSubmitInfo[] GetGuildJoinSubmitInfos()
        {
            return guild.GetGuildJoinSubmitInfos();
        }

        /// <summary>
        /// 길드원 목록 반환
        /// </summary>
        /// <returns></returns>
        public GuildMemberInfo[] GetGuildMemberInfos()
        {
            return guild.GetGuildMemberInfos();
        }

        /// <summary>
        /// 길드 스킬 목록 반환
        /// </summary>
        /// <returns></returns>
        public GuildSkill[] GetGuildSkillInfos()
        {
            return guild.GetGuildSkillInfos();
        }

        public async Task RequestGuildMemberList()
        {
            await guild.RequestGuildMemberList();
        }

        public async Task RequestJoinSubmitUserList()
        {
            await guild.RequestJoinSubmitUserList();
        }

        public async Task RequestGuildJoinSumitUserProc(GuildJoinSubmitInfo info, byte isAccept)
        {
            await guild.RequestGuildJoinSumitUserProc(info, isAccept);
            view.Refresh();
        }

        public async Task EnterGuildLobby()
        {
            if (UIBattleMatchReady.IsMatching)
            {
                UI.ShowToastPopup(LocalizeKey._90231.ToText()); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                return;
            }

            if (battleManager.Mode == BattleMode.MultiMazeLobby)
            {
                UI.ShowToastPopup(LocalizeKey._90226.ToText()); // 미로섬에서는 입장할 수 없습니다.\n사냥 필드로 이동해주세요.
                return;
            }

            // 이미 길드 아지트에 존재
            if (guildSquareManager.IsJoined())
            {
                UI.ShowToastPopup(LocalizeKey._90251.ToText()); // 이미 길드 스퀘어에 위치하고 있습니다.
                return;
            }

            string message = LocalizeKey._33134.ToText(); // 길드 스퀘어로 이동하시겠습니까?
            if (!await UI.SelectPopup(message))
                return;

            battleManager.StartBattle(BattleMode.GuildLobby);
        }

        public async void RequestGuildSkillBuyExp(GuildSkill info, byte costType)
        {
            await guild.RequestGuildSkillBuyExp(info, costType);
            view.Refresh();
        }

        public async void RequestSkillLevelUp(GuildSkill info)
        {
            await guild.RequestSkillLevelUp(info);
            view.Refresh();
        }

        public async Task RequestSkillList()
        {
            await guild.RequestGuildSkillList(isLevelUp: false);
        }

        /// <summary>
        /// 길드 게시판 목록
        /// </summary>
        public async Task RequestGuildBoardList()
        {
            await guild.RequestGuildBoardList(1);
        }

        /// <summary>
        /// 길드원 정보 보기
        /// </summary>
        public void ShowGuildMemberInfo(GuildMemberInfo info)
        {
            // 본인정보 클릭시
            if (guild.CID == info.CID)
                return;

            // 일반길드원은 권한 없음
            if (guild.GuildPosition == GuildPosition.Member)
                return;

            if (info.GuildPosition == GuildPosition.Master)
                return;

            guild.SetSelectGuildMeberInfo(info);
            UI.Show<UIGuildMemberInfo>();
        }

        /// <summary>
        /// 테이밍 미로 정보 요청
        /// </summary>
        public async Task RequestTamingMazeData()
        {
            await guild.RequestTamingMazeInfo();
        }

        private void InvokeUpdateTamingMazeOpen(bool isOpen)
        {
            OnTamingMazeOpen?.Invoke();
        }

        /// <summary>
        /// 길드 아지트 알림 여부
        /// </summary>
        public bool HasGuildLobbyNotice()
        {
            if (!guild.HaveGuild)
                return false;

            return guild.HasTamingMazeNotice || shopModel.HasFreeGuildShopItem();
        }
    }
}