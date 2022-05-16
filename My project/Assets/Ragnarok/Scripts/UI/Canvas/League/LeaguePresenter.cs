using UnityEngine;
using Ragnarok.View.League;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UILeague"/>
    /// </summary>
    public sealed class LeaguePresenter : ViewPresenter
    {
        public const int IN_CLAC_SCORE = -1; // 정산 중
        private const int TAB_SINGLE = 0; // 싱글 보상
        private const int TAB_AGNET = 1; // 협동 보상

        private readonly ILeagueCanvas canvas;
        private readonly GoodsModel goodsModel;
        private readonly LeagueModel leagueModel;
        private readonly InventoryModel inventoryModel;
        private readonly RankModel rankModel;
        private readonly AlarmModel alarmModel;
        private readonly AgentModel agentModel;
        private readonly QuestModel questModel;
        private readonly UserModel userModel;
        private readonly PvETierDataManager leagueTiearDataRepo;
        private readonly PvERankRewardDataManager leagueRankRewardDataRepo;
        private readonly ProfileDataManager profileDataRepo;
        private readonly BattleManager battleManager;
        private readonly int leagueTicketId;
        private readonly int maxFreeTicket; // 하루에 최대 들어갈 수 있는 무료 횟수

        private readonly LeagueMainViewInfo leagueMainViewInfo;
        private readonly LeagueRankViewInfo leagueRankViewInfo, leagueSingleRankViewInfo;

        bool isStartBattle;

        /// <summary>
        /// 입장권 티켓 수
        /// </summary>
        public int LeagueTicketCount => inventoryModel.GetItemCount(leagueTicketId);

        /// <summary>
        /// 무료 입장 가능 수
        /// </summary>
        public int LeagueFreeCount => leagueModel.LeagueFreeTicket;

        /// <summary>
        /// 하루에 최대 들어갈 수 있는 무료 횟수
        /// </summary>
        public int LeagueFreeMaxCount => maxFreeTicket;

        private RankType curRankType;

        public event System.Action OnUpdateEquippedAgent
        {
            add { agentModel.OnUpdateAgentEquipmentState += value; }
            remove { agentModel.OnUpdateAgentEquipmentState -= value; }
        }

        public event System.Action OnUpdateNewAgent
        {
            add { agentModel.OnGetNewAgent += value; }
            remove { agentModel.OnGetNewAgent -= value; }
        }

        public event System.Action<long> OnUpdateZeny
        {
            add { goodsModel.OnUpdateZeny += value; }
            remove { goodsModel.OnUpdateZeny -= value; }
        }

        public event System.Action<long> OnUpdateCatCoin
        {
            add { goodsModel.OnUpdateCatCoin += value; }
            remove { goodsModel.OnUpdateCatCoin -= value; }
        }

        public LeaguePresenter(ILeagueCanvas canvas)
        {
            this.canvas = canvas;
            goodsModel = Entity.player.Goods;
            leagueModel = Entity.player.League;
            inventoryModel = Entity.player.Inventory;
            rankModel = Entity.player.RankModel;
            alarmModel = Entity.player.AlarmModel;
            agentModel = Entity.player.Agent;
            questModel = Entity.player.Quest;
            userModel = Entity.player.User;
            leagueTiearDataRepo = PvETierDataManager.Instance;
            leagueRankRewardDataRepo = PvERankRewardDataManager.Instance;
            profileDataRepo = ProfileDataManager.Instance;
            battleManager = BattleManager.Instance;
            leagueTicketId = BasisItem.LeagueTicket.GetID();
            maxFreeTicket = BasisType.PVE_FREE_JOIN_CNT.GetInt();

            leagueMainViewInfo = new LeagueMainViewInfo(Entity.player);
            leagueRankViewInfo = new LeagueRankViewInfo(Entity.player);
            leagueSingleRankViewInfo = new LeagueRankViewInfo(Entity.player);

            ReloadMainViewInfo();
            ReloadRankViewInfo();
        }

        public override void AddEvent()
        {
            BattleManager.OnStart += OnStartBattle;
            rankModel.OnUpdateRankList += OnUpdateRankList;
        }

        public override void RemoveEvent()
        {
            BattleManager.OnStart -= OnStartBattle;
            rankModel.OnUpdateRankList -= OnUpdateRankList;
        }

        void OnStartBattle(BattleMode mode)
        {
            if (isStartBattle)
            {
                this.canvas.CloseUI();
                isStartBattle = false;
                return;
            }
        }

        /// <summary>
        /// 제니 반환
        /// </summary>
        public long GetZeny()
        {
            return goodsModel.Zeny;
        }

        /// <summary>
        /// 캣코인 반환
        /// </summary>
        public long GetCatCoin()
        {
            return goodsModel.CatCoin;
        }

        /// <summary>
        /// 티어 별 보상 정보
        /// </summary>
        public UILeagueGradeRewardBar.IInput[] GetGradeRewardInfos()
        {
            return leagueTiearDataRepo.GetArray();
        }

        /// <summary>
        /// 랭킹 별 보상 정보
        /// </summary>
        public UILeagueRankRewardBar.IInput[] GetRankRewardInfos(int index)
        {
            switch (index)
            {
                default:
                case TAB_SINGLE: return leagueRankRewardDataRepo.GetSingleRewards();
                case TAB_AGNET: return leagueRankRewardDataRepo.GetAgentRewards();
            }
        }

        /// <summary>
        /// 서버 정보 초기화
        /// </summary>
        public void ResetServerInfo()
        {
            leagueMainViewInfo.ResetServerInfo(); // 메인 정보 - 서버 정보 초기화
            leagueRankViewInfo.ResetServerInfo(); // 랭킹 정보 - 서버 정보 초기화
            leagueSingleRankViewInfo.ResetServerInfo(); // 싱글 랭킹 정보 - 서버 정보 초기화
        }

        /// <summary>
        /// 리그 오픈 여부
        /// </summary>
        public bool IsOpendLeague()
        {
            return leagueMainViewInfo.IsOpendLeague();
        }

        /// <summary>
        /// 멀티로비 여부
        /// </summary>
        public bool IsCheckMultiLobby()
        {
            if (battleManager.Mode == BattleMode.MultiMazeLobby)
            {
                UI.ShowToastPopup(LocalizeKey._90226.ToText()); // 미로섬에서는 입장할 수 없습니다.\n사냥 필드로 이동해주세요.
                return true;
            }

            return false;
        }

        /// <summary>
        /// 티켓 제크
        /// </summary>
        public bool IsTicket()
        {
            // 입장권 없음
            if (LeagueFreeCount == 0 && LeagueTicketCount == 0)
            {
                UI.ShowToastPopup(LocalizeKey._481.ToText()); // 대전 입장권이 부족합니다.
                return false;
            }
            return true;
        }

        /// <summary>
        /// 남은 오픈 시간 보여주기
        /// </summary>
        public void ShowOpenTimeMessage()
        {
            // 리그 오픈 중
            if (IsOpendLeague())
                return;

            // 남은 시간 값이 존재하지 않음
            float openRemainTime = leagueMainViewInfo.SeasonOpenTime.ToRemainTime();
            if (openRemainTime <= 0f)
            {
                UI.ConfirmPopup(LocalizeKey._47031.ToText()); // 시즌이 종료되었습니다.
                return;
            }

            var timeSpan = openRemainTime.ToTimeSpan();
            string text = StringBuilderPool.Get()
                .Append(LocalizeKey._47031.ToText()) // 시즌이 종료되었습니다.
                .AppendLine()
                .Append("(")
                .Append(LocalizeKey._47002.ToText() // 시즌 오픈일 까지: {DAYS}일 {HOURS}시간 {MINUTES}분 {SECONDS}초
                    .Replace(ReplaceKey.DAYS, timeSpan.Days)
                    .Replace(ReplaceKey.HOURS, timeSpan.Hours)
                    .Replace(ReplaceKey.MINUTES, timeSpan.Minutes)
                    .Replace(ReplaceKey.SECONDS, timeSpan.Seconds))
                .Append(")")
                .Release();

            UI.ConfirmPopup(text);
        }

        /// <summary>
        /// 신규 컨텐츠 플래그 제거
        /// </summary>
        public void RemoveNewOpenContent_Pvp()
        {
            questModel.RemoveNewOpenContent(ContentType.Pvp); // 신규 컨텐츠 플래그 제거 (대전)
        }

        /// <summary>
        /// [서버 호출] 대전 정보
        /// </summary>
        public void RequestPveInfo()
        {
            SetLeagueMainViewData(); // 리그 메인 정보 세팅

            // 서버에서 받은 대전 정보가 없을 경우: 대전 정보 호출 후 다시 세팅
            if (leagueMainViewInfo.IsEmptyServerInfo)
                RequestPveInfoAsync().WrapNetworkErrors();
        }

        public void OnSelectRankTab(int index)
        {
            switch (index)
            {
                default:
                case TAB_SINGLE:
                    RequestPveRank(RankType.SingleLeague);
                    break;

                case TAB_AGNET:
                    RequestPveRank(RankType.League);
                    break;
            }
        }

        /// <summary>
        /// [서버 호출] 전체 랭킹
        /// </summary>
        public void RequestPveRank(RankType rankType)
        {
            curRankType = rankType;
            SetLeagueRankViewData(); // 리그 랭킹 정보 세팅

            // 서버에서 받은 정보가 없을 경우: 랭킹 정보 호출 후 다시 세팅
            if (GetCurrentRankViewInfo().IsEmptyServerInfo)
                RequestPveRankAsync();
        }

        /// <summary>
        /// [서버 호출] 전체 랭킹 - 다음 페이지
        /// </summary>
        public void RequestNextPveRank()
        {
            RequestPveNextRankAsync();
        }

        /// <summary>
        /// 대전 시작
        /// </summary>
        public void StartBattlePve()
        {
            StartBattlePve(0);
        }

        /// <summary>
        /// 대전 시작
        /// </summary>
        public void StartBattlePve(int count)
        {
            if (!IsOpendLeague())
            {
                ShowOpenTimeMessage();
                return;
            }

            isStartBattle = true;
            battleManager.StartBattle(BattleMode.League, count);
        }

        /// <summary>
        /// 동료 장착 가능 여부
        /// </summary>
        public bool CanEquipAgent()
        {
            if (!questModel.IsOpenContent(ContentType.CombatAgent))
                return false;

            return agentModel.CanEquipAgent();
        }

        /// <summary>
        /// 대전 모드 선택 세팅
        /// </summary>
        public void SetSelectMode(bool isSingle)
        {
            leagueModel.SetSelectMode(isSingle);
        }

        /// <summary>
        /// 대전 싱글 모드 여부
        /// </summary>
        public bool IsSingle()
        {
            return leagueModel.IsSingle;
        }

        public void RequestOtherCharacterInfo((int uid, int cid) info)
        {
            userModel.RequestOtherCharacterInfo(info.uid, info.cid).WrapNetworkErrors();
        }

        private async Task RequestPveInfoAsync()
        {
            var response = await Protocol.REQUEST_PVE_INFO.SendAsync();
            if (response.isSuccess)
            {
                CharacterPvePacket packet = response.GetPacket<CharacterPvePacket>("1"); // 이전 시즌 정보
                long seasonCloseTime = response.ContainsKey("2") ? response.GetLong("2") : 0L; // 종료까지 남은 시간
                long seasonOpenTime = response.ContainsKey("3") ? response.GetLong("3") : 0L; // 다음 시즌 시작까지 남은 시간
                Rank[] arrRank = response.GetPacketArray<Rank>("a"); // 랭킹 정보
                long rankUpdateTime = response.GetLong("t"); // 랭킹 업데이트 시간
                long myRank = response.GetLong("r"); // 내 현재 랭킹 (-1: 집계중)

                if (arrRank != null)
                    System.Array.Sort(arrRank, SortByRanking); // 랭킹 정보 정렬

                leagueMainViewInfo.SetServerInfo(packet, seasonCloseTime, seasonOpenTime, rankUpdateTime, myRank, CreateArrayRank(arrRank)); // 리그 메인 정보 세팅

                ReloadMainViewInfo(); // 메인 정보 새로고침
                SetLeagueMainViewData(); // 리그 메인 정보 세팅

                // 리그 보상 정보 세팅
                if (packet.isReward)
                {
                    alarmModel.AddAlarm(AlarmType.MailCharacter); // 캐릭터 우편함 알림 추가

                    canvas.ShowRewardPopup(new LeagueRewardViewInfo(packet.pve_season_no
                        , CreateReward(packet.old_rank, packet.old_tier)
                        , CreateReward(packet.reward_rank, packet.reward_tier)
                        , leagueTiearDataRepo.Get(packet.reward_tier)
                        , leagueRankRewardDataRepo.Get(packet.reward_rank)
                        , leagueRankRewardDataRepo.GetSingleReward(packet.single_rank)));
                }

                leagueModel.SetCurrentScore(packet.pve_season_score); // 현 시즌 점수 저장
                leagueModel.SetCurrentSingleScore(packet.single_score); // 싱글 점수 저장
            }
            else if (response.resultCode == ResultCode.IN_CLAC_PVP_SEASON_RANK) // (82) 정산중일 때
            {
                long seasonOpenTime = response.GetLong("3"); // 다음 시즌 시작까지 남은 시간
                leagueMainViewInfo.SetServerInfo(null, 0L, seasonOpenTime, 0L, IN_CLAC_SCORE, null); // 리그 메인 정보 세팅
                SetLeagueMainViewData(); // 리그 메인 정보 세팅

                ShowOpenTimeMessage(); // 남은 오픈 시간 보여주기
            }
            else
            {
                response.ShowResultCode();
            }
        }

        private void RequestPveRankAsync()
        {
            rankModel.ClearRankInfo(curRankType);
            rankModel.RequestRankList(page: 1, curRankType).WrapNetworkErrors();
        }

        private void RequestPveNextRankAsync()
        {
            rankModel.RequestNextRankList(curRankType).WrapNetworkErrors();
        }

        private void OnUpdateRankList((RankType rankType, int page) info)
        {
            // 첫 페이지 호출
            if (info.page == 1)
            {
                RankInfo myInfo = rankModel.GetMyRankInfo(curRankType);
                GetCurrentRankViewInfo().SetServerInfo(CreateRank(myInfo));
                ReloadRankViewInfo();
            }

            RankInfo[] arrRank = rankModel.GetRankInfos(curRankType);
            bool hasNextPage = rankModel.HasNextPage(curRankType);
            GetCurrentRankViewInfo().SetServerInfo(CreateArrayRank(arrRank), hasNextPage);
            SetLeagueRankViewData(); // 리그 랭킹 정보 세팅
        }

        /// <summary>
        /// 메인 정보 다시 로드
        /// </summary>
        private void ReloadMainViewInfo()
        {
            int tier = leagueTiearDataRepo.GetTier(leagueMainViewInfo.Score); // Score 에서 Tier로 전환
            UILeagueGradeRewardBar.IInput data = leagueTiearDataRepo.Get(tier); // Tier 정보
            leagueMainViewInfo.ReloadDetailInfo(data.GetName(), data.GetIconName(), LeagueTicketCount, LeagueFreeCount, LeagueFreeMaxCount); // 디테일 정보 세팅
        }

        /// <summary>
        /// 랭크 정보 다시 로드
        /// </summary>
        private void ReloadRankViewInfo()
        {
            // 싱글 대전 랭킹은 티어 아이콘 갱신 불필요
            if (curRankType == RankType.SingleLeague)
                return;

            int tier = leagueTiearDataRepo.GetTier(GetCurrentRankViewInfo().Score); // Score 에서 Tier로 전환
            UILeagueGradeRewardBar.IInput data = leagueTiearDataRepo.Get(tier); // Tier 정보
            GetCurrentRankViewInfo().ReloadDetailInfo(data.GetIconName()); // 디테일 정보 세팅
        }

        /// <summary>
        /// 리그 메인 정보 세팅
        /// </summary>
        private void SetLeagueMainViewData()
        {
            canvas.SetData(leagueMainViewInfo);
        }

        /// <summary>
        /// 리그 랭킹 정보 세팅
        /// </summary>
        private void SetLeagueRankViewData()
        {
            canvas.SetData(GetCurrentRankViewInfo());
        }

        private UILeagueRankBar.IInput[] CreateArrayRank(Rank[] input)
        {
            UILeagueRankBar.IInput[] output = new UILeagueRankBar.IInput[input == null ? 0 : input.Length];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = CreateRank(input[i]);
            }

            return output;
        }

        private UILeagueRankBar.IInput[] CreateArrayRank(RankInfo[] input)
        {
            UILeagueRankBar.IInput[] output = new UILeagueRankBar.IInput[input == null ? 0 : input.Length];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = CreateRank(input[i]);
            }

            return output;
        }

        private UILeagueRankBar.IInput CreateRank(Rank rank)
        {
            int tier = leagueTiearDataRepo.GetTier((int)rank.score);
            UILeagueGradeRewardBar.IInput data = leagueTiearDataRepo.Get(tier);
            return new LeagueRankInfo(profileDataRepo, rank, data.GetName(), data.GetIconName());
        }

        private UILeagueRankBar.IInput CreateRank(RankInfo rankInfo)
        {
            switch (curRankType)
            {
                default:
                case RankType.SingleLeague:
                    return new LeagueRankInfo(rankInfo, string.Empty, PvETierDataManager.SINGLE_GRADE_ICON_NAME);

                case RankType.League:
                    int tier = leagueTiearDataRepo.GetTier((int)rankInfo.Score);
                    UILeagueGradeRewardBar.IInput data = leagueTiearDataRepo.Get(tier);
                    return new LeagueRankInfo(rankInfo, data.GetName(), data.GetIconName());
            }
        }

        private UILeagueGradeResult.IInput CreateReward(int rank, int tier)
        {
            UILeagueGradeRewardBar.IInput data = leagueTiearDataRepo.Get(Mathf.Max(1, tier)); // 서버에서 이전 시즌 티어 등급이 0으로 올 수 있다
            return new LeagueRewardInfo(rank, data.GetName(), data.GetIconName());
        }

        /// <summary>
        /// 낮은 랭킹 순으로 정렬
        /// </summary>
        private int SortByRanking(Rank a, Rank b)
        {
            return a.ranking.CompareTo(b.ranking);
        }

        /// <summary>
        /// 현재 랭킹 정보 반환
        /// </summary>
        private LeagueRankViewInfo GetCurrentRankViewInfo()
        {
            switch (curRankType)
            {
                default:
                case RankType.SingleLeague:
                    return leagueSingleRankViewInfo;

                case RankType.League:
                    return leagueRankViewInfo;
            }
        }

        private class LeagueMainViewInfo : UILeagueMainInfo.IInput
        {
            private readonly CharacterEntity entity;

            /******************** 서버 메인 정보 ********************/
            private int scroe;
            private int winCount;
            private int loseCount;
            private RemainTime seasonCloseTime;
            private RemainTime seasonOpenTime;
            private UILeagueRankBar.IInput[] arrRank;
            private long myRank;
            private System.DateTime rankUpdateTime;
            /******************** 상세 정보 ********************/
            private string tierName;
            private string tierIconName;
            private int ticketCount;
            private int freeCount;
            private int freeMaxCount;

            /// <summary>
            /// 서버 정보가 비어있을 경우
            /// </summary>
            public bool IsEmptyServerInfo { get; private set; }

            int UILeagueRankBar.IInput.UID => entity.User.UID;
            int UILeagueRankBar.IInput.CID => entity.Character.Cid;
            int UILeagueRankBar.IInput.Ranking => (int)myRank;
            Job UILeagueRankBar.IInput.Job => entity.Character.Job;
            Gender UILeagueRankBar.IInput.Gender => entity.Character.Gender;
            string UILeagueRankBar.IInput.Name => entity.Character.Name;
            string UILeagueRankBar.IInput.CidHex => entity.Character.CidHex;
            int UILeagueRankBar.IInput.JobLevel => entity.Character.JobLevel;
            int UILeagueRankBar.IInput.Power => entity.GetTotalAttackPower();
            public int Score => scroe;
            string UILeagueRankBar.IInput.TierIconName => tierIconName;
            string UILeagueMyRank.IInput.TierName => tierName;
            int UILeagueMyRank.IInput.WinCount => winCount;
            int UILeagueMyRank.IInput.LoseCount => loseCount;

            int UILeagueMainInfo.IInput.RankSize => arrRank == null ? 0 : arrRank.Length;
            RemainTime UILeagueMainInfo.IInput.SeasonCloseTime => seasonCloseTime;
            public RemainTime SeasonOpenTime => seasonOpenTime;
            System.DateTime UILeagueMainInfo.IInput.RankUpdateTime => rankUpdateTime;
            int UILeagueMainInfo.IInput.TicketCount => ticketCount;
            int UILeagueMainInfo.IInput.FreeCount => freeCount;
            int UILeagueMainInfo.IInput.FreeMaxCount => freeMaxCount;
            string UILeagueRankBar.IInput.ProfileName => entity.GetProfileName();

            public LeagueMainViewInfo(CharacterEntity entity)
            {
                this.entity = entity;
                ResetServerInfo();
            }

            public void ReloadDetailInfo(string tierName, string tierIconName, int ticketCount, int freeCount, int freeMaxCount)
            {
                this.tierName = tierName;
                this.tierIconName = tierIconName;
                this.ticketCount = ticketCount;
                this.freeCount = freeCount;
                this.freeMaxCount = freeMaxCount;
            }

            public void ResetServerInfo()
            {
                IsEmptyServerInfo = false;
                SetServerInfo(null, 0L, 0L, 0L, 0, null);
            }

            public void SetServerInfo(CharacterPvePacket packet, long seasonCloseTime, long seasonOpenTime, long rankUpdateTime, long myRank, UILeagueRankBar.IInput[] arrRank)
            {
                IsEmptyServerInfo = true;
                scroe = (packet == null) ? 0 : packet.pve_season_score;
                winCount = (packet == null) ? 0 : packet.pve_season_win_count;
                loseCount = (packet == null) ? 0 : packet.pve_season_lose_count;
                this.seasonCloseTime = seasonCloseTime;
                this.seasonOpenTime = seasonOpenTime;
                this.rankUpdateTime = rankUpdateTime.ToDateTime();
                this.myRank = myRank;
                this.arrRank = arrRank;
            }

            public bool IsOpendLeague()
            {
                return seasonCloseTime.ToRemainTime() > 0f; // 종료까지 남아있는 시간이 존재할 경우
            }

            public UILeagueRankBar.IInput[] RankArray()
            {
                return arrRank;
            }
        }

        private class LeagueRewardViewInfo : LeagueResultPopupView.IInput
        {
            /******************** 서버 보상 정보 ********************/
            private int seasonNo;
            private UILeagueGradeResult.IInput before;
            private UILeagueGradeResult.IInput after;

            int LeagueResultPopupView.IInput.SeasonNo => seasonNo;
            UILeagueGradeResult.IInput LeagueResultPopupView.IInput.Before => before;
            UILeagueGradeResult.IInput LeagueResultPopupView.IInput.After => after;

            private readonly BetterList<RewardData> rewardList;

            public LeagueRewardViewInfo(int seasonNo, UILeagueGradeResult.IInput before, UILeagueGradeResult.IInput after, PvETierData pveTierData, PvERankRewardData pveRankRewardData, PvERankRewardData pveSingleRankRewardData)
            {
                rewardList = new BetterList<RewardData>();
                this.seasonNo = seasonNo;
                this.before = before;
                this.after = after;

                foreach (var item in pveTierData.rewards)
                {
                    if (item.RewardType == RewardType.None)
                        continue;

                    rewardList.Add(item);
                }

                foreach (var item in pveRankRewardData.rewards)
                {
                    if (item.RewardType == RewardType.None)
                        continue;

                    rewardList.Add(item);
                }

                if (pveSingleRankRewardData != null)
                {
                    foreach (var item in pveSingleRankRewardData.rewards)
                    {
                        if (item.RewardType == RewardType.None)
                            continue;

                        rewardList.Add(item);
                    }
                }
            }

            RewardData LeagueResultPopupView.IInput.GetReward(int index)
            {
                if (index < rewardList.size)
                    return rewardList[index];

                return null;
            }
        }

        private class LeagueRankViewInfo : UILeagueRankInfo.IInput
        {
            private readonly CharacterEntity entity;

            /******************** 서버 랭킹 정보 ********************/
            private UILeagueRankBar.IInput myRank;
            private UILeagueRankBar.IInput[] arrRank;
            private bool hasNextPage;
            /******************** 상세 정보 ********************/
            private string tierIconName;

            /// <summary>
            /// 서버 정보가 비어있을 경우
            /// </summary>
            public bool IsEmptyServerInfo { get; private set; }

            int UILeagueRankBar.IInput.UID => entity.User.UID;
            int UILeagueRankBar.IInput.CID => entity.Character.Cid;
            int UILeagueRankBar.IInput.Ranking => IsEmptyServerInfo ? 0 : myRank.Ranking;
            Job UILeagueRankBar.IInput.Job => entity.Character.Job;
            Gender UILeagueRankBar.IInput.Gender => entity.Character.Gender;
            string UILeagueRankBar.IInput.Name => entity.Character.Name;
            string UILeagueRankBar.IInput.CidHex => entity.Character.CidHex;
            int UILeagueRankBar.IInput.JobLevel => IsEmptyServerInfo ? 0 : myRank.JobLevel;
            int UILeagueRankBar.IInput.Power => entity.GetTotalAttackPower();
            public int Score => IsEmptyServerInfo ? 0 : myRank.Score;
            string UILeagueRankBar.IInput.TierIconName => IsEmptyServerInfo ? tierIconName : myRank.TierIconName;
            int UILeagueRankInfo.IInput.RankSize => arrRank == null ? 0 : arrRank.Length;
            bool UILeagueRankInfo.IInput.HasNextPage => hasNextPage;
            string UILeagueRankBar.IInput.ProfileName => entity.GetProfileName();

            public LeagueRankViewInfo(CharacterEntity entity)
            {
                this.entity = entity;
                ResetServerInfo();
            }

            public void ReloadDetailInfo(string tierIconName)
            {
                this.tierIconName = tierIconName;
            }

            public void ResetServerInfo()
            {
                SetServerInfo(null);
                SetServerInfo(null, false);
            }

            public void SetServerInfo(UILeagueRankBar.IInput myRank)
            {
                IsEmptyServerInfo = myRank == null;
                this.myRank = myRank;
            }

            public void SetServerInfo(UILeagueRankBar.IInput[] arrRank, bool hasNextPage)
            {
                this.arrRank = arrRank;
                this.hasNextPage = hasNextPage;
            }

            public UILeagueRankBar.IInput[] RankArray()
            {
                return arrRank;
            }
        }

        private class LeagueRankInfo : UILeagueRankBar.IInput
        {
            public int UID { get; private set; }
            public int CID { get; private set; }
            public int Ranking { get; private set; }
            public Job Job { get; private set; }
            public Gender Gender { get; private set; }
            public string Name { get; private set; }
            public string CidHex { get; private set; }
            public int JobLevel { get; private set; }
            public int Power { get; private set; }
            public int Score { get; private set; }
            public string TierIconName { get; private set; }
            public string TierName { get; private set; }
            private int profileId;
            public string ProfileName { get; private set; }

            public LeagueRankInfo(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl, Rank rank, string tierName, string tierIconName)
            {
                UID = rank.uid;
                CID = rank.cid;
                Ranking = rank.ranking;
                Job = rank.job.ToEnum<Job>();
                Gender = rank.gender.ToEnum<Gender>();
                Name = rank.char_name;
                CidHex = rank.cidHex;
                JobLevel = rank.job_level;
                Power = rank.battle_score;
                Score = (int)rank.score;
                profileId = rank.profileId;
                TierIconName = tierIconName;
                TierName = tierName;
                ProfileName = GetProfileName(profileDataRepoImpl);
            }

            public LeagueRankInfo(RankInfo rank, string tierName, string tierIconName)
            {
                UID = rank.UID;
                CID = rank.CID;
                Ranking = (int)rank.Rank;
                Job = rank.Job;
                Gender = rank.Gender;
                Name = rank.CharName;
                CidHex = rank.CIDHex;
                JobLevel = rank.JobLevel;
                Power = rank.BattleScore;
                Score = (int)rank.Score;
                TierIconName = tierIconName;
                TierName = tierName;
                ProfileName = rank.ProfileName;
            }

            private string GetProfileName(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl)
            {
                if (profileId > 0)
                {
                    ProfileData profileData = profileDataRepoImpl.Get(profileId);
                    if (profileData != null)
                        return profileData.ProfileName;
                }

                return Job.GetJobProfile(Gender);
            }
        }

        private class LeagueRewardInfo : UILeagueGradeResult.IInput
        {
            public string TierName { get; private set; }
            public string TierIconName { get; private set; }
            public int Ranking { get; private set; }

            public LeagueRewardInfo(int rank, string tierName, string tierIconName)
            {
                Ranking = rank;
                TierName = tierName;
                TierIconName = tierIconName;
            }
        }
    }
}