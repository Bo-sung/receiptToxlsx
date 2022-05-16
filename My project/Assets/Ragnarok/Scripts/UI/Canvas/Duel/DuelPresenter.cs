using Ragnarok.View;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIDuel"/>
    /// </summary>
    public class DuelPresenter : ViewPresenter
    {
        public enum EventDuelState
        {
            InProgress = 1, // 진행중
            Calculate = 2, // 정산중
            Finished = 3, // 종료
            NotInProgress = 4, // 진행중이 아님
        }

        private readonly UIDuel view;
        private readonly DuelModel duelModel;
        private readonly CharacterModel characterModel;
        private readonly AgentModel agentModel;
        private readonly GoodsModel goodsModel;
        private readonly QuestModel questModel;
        private readonly UserModel userModel;
        private readonly JobDataManager jobDataRepo;
        private readonly ConnectionManager connectionManager;
        private readonly DuelRewardDataManager duelRewardDataRepo;
        private readonly EventDuelRewardDataManager eventDuelRewardDataRepo;
        private readonly EventDualBuffDataManager eventDualBuffDataRepo;
        private readonly DuelArenaDataManager duelArenaDataRepo;
        private readonly ProfileDataManager profileDataRepo;
        public readonly int arenaNeedJobLevel;
        public readonly int arenaFlagCatCoin;

        private readonly AdventureData[] adventures;
        private readonly AdventureData[] chapters;

        private int lastClearedChpater;
        private int curShowingChapter;
        private ChapterDuelState curDuelState;
        private RewardData[] eventRewards, arenaRewards;
        private int[] eventConditionValues, arenaConditionValues;
        private int arenaPoint, arenaWinCount, arenaLoseCount;

        private int registeredShortCutChapter = -1;
        private UIDuel.State duelState;

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

        public event System.Action OnResetWaitingTime
        {
            add { duelModel.OnResetWaitingTime += value; }
            remove { duelModel.OnResetWaitingTime -= value; }
        }

        public event System.Action OnResetEventWaitingTime
        {
            add { duelModel.OnResetEventWaitingTime += value; }
            remove { duelModel.OnResetEventWaitingTime -= value; }
        }

        public event System.Action OnUpdateCharacterInfo;

        public event System.Action OnUpdateCharacterGender
        {
            add { characterModel.OnChangedGender += value; }
            remove { characterModel.OnChangedGender -= value; }
        }

        public event System.Action OnResetArenaWaitingTime
        {
            add { duelModel.OnResetArenaWaitingTime += value; }
            remove { duelModel.OnResetArenaWaitingTime -= value; }
        }

        public DuelPresenter(UIDuel view)
        {
            this.view = view;
            duelModel = Entity.player.Duel;
            characterModel = Entity.player.Character;
            agentModel = Entity.player.Agent;
            goodsModel = Entity.player.Goods;
            questModel = Entity.player.Quest;
            userModel = Entity.player.User;
            jobDataRepo = JobDataManager.Instance;
            connectionManager = ConnectionManager.Instance;
            duelRewardDataRepo = DuelRewardDataManager.Instance;
            eventDuelRewardDataRepo = EventDuelRewardDataManager.Instance;
            eventDualBuffDataRepo = EventDualBuffDataManager.Instance;
            duelArenaDataRepo = DuelArenaDataManager.Instance;
            profileDataRepo = ProfileDataManager.Instance;

            adventures = AdventureDataManager.Instance.GetArrData();
            List<AdventureData> townList = new List<AdventureData>();

            curShowingChapter = -1;
            for (int i = 0; i < adventures.Length; ++i)
            {
                if (curShowingChapter == -1)
                    curShowingChapter = adventures[i].chapter;
                if (adventures[i].link_type == 1)
                    townList.Add(adventures[i]);
            }

            if (duelModel.RecentlyChapter != -1)
                curShowingChapter = duelModel.RecentlyChapter;

            chapters = townList.ToArray();

            arenaNeedJobLevel = BasisDuelArenaInfo.NeedJobLevel.GetInt();
            arenaFlagCatCoin = BasisDuelArenaInfo.ArenaFlagCatCoin.GetInt();
        }

        public override void AddEvent()
        {
            agentModel.OnUpdateAgentEquipmentState += UpdateCombatAgents;
            characterModel.OnChangedJob += OnChangeCharacterJob;
            characterModel.OnChangedName += OnChangeCharacterName;
            duelModel.OnUpdateDuelInfo += OnUpdateDuelInfo;
        }

        public override void RemoveEvent()
        {
            agentModel.OnUpdateAgentEquipmentState -= UpdateCombatAgents;
            characterModel.OnChangedJob -= OnChangeCharacterJob;
            characterModel.OnChangedName -= OnChangeCharacterName;
            duelModel.OnUpdateDuelInfo -= OnUpdateDuelInfo;
        }

        public void OnShow(bool needRequestDuelInfo, UIDuel.State duelState)
        {
            this.duelState = duelState;
            int lastClearStageID = Entity.player.Dungeon.FinalStageId;
            lastClearedChpater = 1;

            for (int i = 0; i < adventures.Length; ++i)
            {
                var each = adventures[i];
                if (each.link_type == 2 && each.link_value <= lastClearStageID)
                    lastClearedChpater = each.chapter;
            }

            if (needRequestDuelInfo)
            {
                RequestServerInfo();
            }
            else
            {
                if (duelModel.RecentlyChapter != -1)
                    curShowingChapter = duelModel.RecentlyChapter;

                UpdateView(true);
            }
        }

        private void RequestServerInfo()
        {
            duelModel.RequestDuelInfo().WrapNetworkErrors();
        }

        void OnUpdateDuelInfo()
        {
            if (registeredShortCutChapter != -1)
            {
                curShowingChapter = registeredShortCutChapter;
                duelModel.RecentlyChapter = curShowingChapter;
                registeredShortCutChapter = -1;
            }

            UpdateView(true);
        }

        private void UpdateView(bool isOnShow = false)
        {
            curDuelState = duelModel.GetDuelState(curShowingChapter);

            if (curDuelState == null)
                return;

            view.SetActiveCompletion(curDuelState.RewardedCount == 2);
            view.SetChapters(chapters, lastClearedChpater);
            view.SetDuelHistory(duelModel.GetDuelHistoryList());
            view.SetMyJobIcon(GetJobGroup());
            view.UpdateNoticeLabel();
            view.SetCombatAgents(agentModel.GetEquipedCombatAgents().ToArray(), Entity.player.Agent.CombatAgentSlotCount);

            switch (duelState)
            {
                case UIDuel.State.Chapter:
                    view.ShowDefaultView();
                    view.SetDuelState(curDuelState, isOnShow);
                    break;

                case UIDuel.State.Event:
                    view.ShowEventView();
                    break;

                case UIDuel.State.Arena:
                    view.ShowArenaView();
                    break;
            }
        }

        private void UpdateCombatAgents()
        {
            view.SetCombatAgents(agentModel.GetEquipedCombatAgents().ToArray(), Entity.player.Agent.CombatAgentSlotCount);
        }

        public void ViewEventHandler(UIDuel.EventType eventType, object data)
        {
            if (eventType == UIDuel.EventType.OnClickChapter)
            {
                var chapter = (int)data;

                if (chapter > lastClearedChpater)
                {
                    UI.ShowToastPopup(LocalizeKey._47835.ToText());
                    return;
                }

                curShowingChapter = chapter;
                duelModel.SelectedState = UIDuel.State.Chapter;
                duelState = UIDuel.State.Chapter;
                duelModel.RecentlyChapter = curShowingChapter;
                UpdateView();
            }
            else if (eventType == UIDuel.EventType.OnClickAlphabet)
            {
                int alphabetIndex = (int)data;

                if (!curDuelState.IsValidAlphabet(alphabetIndex))
                    return;

                UI.Show<UIDuelPlayerList>(new UIDuelPlayerList.Input()
                {
                    duelState = curDuelState,
                    alphabetIndex = alphabetIndex
                });
            }
            else if (eventType == UIDuel.EventType.OnClickCharge)
            {
                RequestBuyDuelPoint().WrapNetworkErrors();
            }
            else if (eventType == UIDuel.EventType.OnClickRecieveReward)
            {
                if (curDuelState.RewardedCount == 2)
                {
                    UI.ShowToastPopup(LocalizeKey._47819.ToText()); // 이미 모든 보상을 다 받았습니다.
                    return;
                }

                bool canGetReward = true;
                for (int i = 0; i < 8; ++i)
                {
                    if (curDuelState.IsValidAlphabet(i) && !curDuelState.IsOwningAlphabet(i))
                    {
                        canGetReward = false;
                        break;
                    }
                }

                if (!canGetReward)
                {
                    UI.ShowToastPopup(LocalizeKey._47820.ToText()); // 듀얼 조각이 부족합니다.
                    return;
                }

                RequestDuelReward().WrapNetworkErrors();
                UpdateView();
            }
            else if (eventType == UIDuel.EventType.OnClickAgent)
            {
                if (!Entity.player.Quest.IsOpenContent(ContentType.CombatAgent))
                    return;
                UI.Show<UIAgent>(new UIAgent.Input() { viewAgentType = AgentType.CombatAgent });
            }
            else if (eventType == UIDuel.EventType.OnSysRequestShowChapter)
            {
                int chapter = (int)data;

                if (chapter > lastClearedChpater)
                    return;

                if (!duelModel.IsDuelInfoInitialized)
                {
                    registeredShortCutChapter = chapter;
                    return;
                }

                curShowingChapter = chapter;
                UpdateView();
            }
            else if (eventType == UIDuel.EventType.OnClickEvent)
            {
                RequestDuelWorldInfo().WrapNetworkErrors();
            }
            else if (eventType == UIDuel.EventType.OnClickArena)
            {
                bool isShowIndicator = (bool)data;
                RequestDuelArenaInfo(isShowIndicator).WrapNetworkErrors();
            }
        }

        /// <summary>
        /// 동료 장착 가능 여부
        /// </summary>
        public bool CanEquipAgent()
        {
            return agentModel.CanEquipAgent();
        }

        /// <summary>
        /// 신규 컨텐츠 플래그 제거
        /// </summary>
        public void RemoveNewOpenContent_Duel()
        {
            questModel.RemoveNewOpenContent(ContentType.Duel); // 신규 컨텐츠 플래그 제거 (듀얼)
        }

        public void InitializeRewardData()
        {
            if (eventRewards == null)
            {
                DuelRewardData[] eventDuelRewardData = duelRewardDataRepo.GetEventRewards();
                int length = eventDuelRewardData == null ? 0 : eventDuelRewardData.Length;
                eventRewards = new RewardData[length];
                eventConditionValues = new int[length];
                for (int i = 0; i < length; i++)
                {
                    DuelRewardData data = eventDuelRewardData[i];
                    eventRewards[i] = data.GetReward();
                    eventConditionValues[i] = data.need_bit_type;
                }
            }

            if (arenaRewards == null)
            {
                DuelRewardData[] arenaDuelRewardData = duelRewardDataRepo.GetArenaRewards();
                int length = arenaDuelRewardData == null ? 0 : arenaDuelRewardData.Length;
                arenaRewards = new RewardData[length];
                for (int i = 0; i < length; i++)
                {
                    DuelRewardData data = arenaDuelRewardData[i];
                    arenaRewards[i] = data.GetReward();
                }

                arenaConditionValues = new int[2];
                arenaConditionValues[0] = BasisDuelArenaInfo.NeedFlagCount_Reward1.GetInt();
                arenaConditionValues[1] = BasisDuelArenaInfo.NeedFlagCount_Reward2.GetInt();
            }
        }

        public RewardData[] GetEventRewards()
        {
            return eventRewards;
        }

        public int[] GetEventConditionValues()
        {
            return eventConditionValues;
        }

        public RewardData[] GetArenaRewards()
        {
            return arenaRewards;
        }

        public int[] GetArenaConditionValues()
        {
            return arenaConditionValues;
        }

        public UIDuelArenaInfo.IInput FindArena(int arenaPoint)
        {
            return duelArenaDataRepo.Find(arenaPoint);
        }

        public UIDuelArenaInfo.IInput FindNextArena(int arenaPoint)
        {
            return duelArenaDataRepo.FindNext(arenaPoint);
        }

        public bool CanEnterArena(bool isShowMessage)
        {
            return characterModel.IsCheckJobLevel(arenaNeedJobLevel, isShowMessage);
        }
        public bool ArenaIsAvalable()
        {
            return BasisExtensions.IsOpend(BasisOpenContetsType.DuelArena);
        }

        public void RequestEventReward(int step)
        {
            AsyncRequestEventReward(step).WrapNetworkErrors();
        }

        public void ShowOtherUserInfo(int uid, int cid)
        {
            userModel.RequestOtherCharacterInfo(uid, cid).WrapNetworkErrors();
        }

        public void RequestArenaPointBuy()
        {
            AsyncRequestArenaPointBuy().WrapNetworkErrors();
        }

        public void RequestArenaReward(int step)
        {
            AsyncRequestArenaReward(step).WrapNetworkErrors();
        }

        private async Task RequestDuelReward()
        {
            var response = await duelModel.RequestDuelReward(curShowingChapter);

            if (!response)
            {
                RequestServerInfo();
                return;
            }

            UpdateView();
        }

        private async Task RequestBuyDuelPoint()
        {
            if (duelModel.DuelPoint + BasisType.DUEL_POINT_CHARGE.GetInt() > BasisType.DUEL_POINT_TOTAL_MAX.GetInt())
            {
                UI.ShowToastPopup(LocalizeKey._47821.ToText()); // 듀얼 포인트가 최대치입니다.
                return;
            }

            int price = BasisType.DUEL_CAT_COIN_JOIN.GetInt() + duelModel.DuelPointBuyCount * BasisType.DUEL_CAT_COIN_INC.GetInt();
            string title = LocalizeKey._47822.ToText(); // 듀얼 포인트 충전
            string message = LocalizeKey._47823.ToText(); // 듀얼 포인트를 충전하시겠습니까?
            if (!await UI.CostPopup(CoinType.CatCoin, price, title, message))
                return;

            if (Entity.player.Goods.CatCoin < price)
            {
                UI.ShowToastPopup(LocalizeKey._47824.ToText()); // 냥다래가 부족합니다.
                return;
            }

            Response response = await duelModel.RequestBuyDuelPoint();

            if (response.isSuccess)
                UI.ShowToastPopup(LocalizeKey._47825.ToText()); // 듀얼 포인트를 구매하였습니다.
        }

        private async Task RequestDuelWorldInfo()
        {
            Response response = await Protocol.REQUEST_DUELWORLD_INFO.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                UpdateView();
                return;
            }

            EventDuelState state = response.GetInt("s").ToEnum<EventDuelState>();
            #region 이벤트탭 선택여부
            switch (state)
            {
                case EventDuelState.InProgress:
                case EventDuelState.Calculate:
                case EventDuelState.Finished:
                    duelModel.SelectedState = UIDuel.State.Event; // 이벤트탭 선택
                    break;
                default:
                    duelModel.SelectedState = UIDuel.State.Chapter;
                    break;
            }
            #endregion
            if (state == EventDuelState.InProgress)
            {
                DuelServerPacket[] packets = response.GetPacketArray<DuelServerPacket>("1");
                int seasonSeq = response.GetInt("2");
                long remainTime = response.GetLong("3");
                long nextRemainTime = response.GetLong("4");
                int myRank = response.GetInt("5");
                int winCount = response.GetInt("6");
                int rewardStep = response.GetInt("8");
                RewardData[] serverRewards = eventDuelRewardDataRepo.GetServerReward(myRank);

                // 내가 선택한 서버
                int selectedServerId = connectionManager.GetSelectServerGroupId();

                // Initialize
                for (int i = 0; i < packets.Length; i++)
                {
                    int nameId = BasisType.SERVER_NAME_ID.GetInt(i);
                    int alphabetType = BasisType.EVENT_DUEL_ALPHABET.GetInt(i);
                    packets[i].Initialize(i, nameId, alphabetType, selectedServerId);
                }

                // 정렬 (내가 선택한 서버가 제일 처음, CompareTo 참조 확인)
                System.Array.Sort(packets);

                view.SetEventView(seasonSeq, remainTime, nextRemainTime, myRank, winCount, serverRewards);
                view.SetEventViewRewardStep(rewardStep);
                view.ShowEventViewServers(packets);
            }
            else if (state == EventDuelState.Calculate)
            {
                int seasonSeq = response.GetInt("2");
                long remainTime = 0L;
                long nextRemainTime = response.GetLong("4");
                int myRank = response.GetInt("5");
                int winCount = response.GetInt("6");
                int rewardStep = response.GetInt("8");
                RewardData[] serverRewards = eventDuelRewardDataRepo.GetServerReward(myRank);

                view.SetEventView(seasonSeq, remainTime, nextRemainTime, myRank, winCount, serverRewards);
                view.SetEventViewRewardStep(rewardStep);
                view.ShowEventViewCalculateNotice();
            }
            else if (state == EventDuelState.Finished)
            {
                DuelServerRankPacket[] packets = response.GetPacketArray<DuelServerRankPacket>("1");
                int seasonSeq = response.GetInt("2");
                long remainTime = 0L;
                long nextRemainTime = response.GetLong("4");
                int myRank = response.GetInt("5");
                int winCount = response.GetInt("6");
                int rewardStep = response.GetInt("8");
                RewardData[] serverRewards = eventDuelRewardDataRepo.GetServerReward(myRank);

                int serverRank = 0; // 서버 순위
                bool isPerfectWin = true;

                // 내가 선택한 서버
                int selectedServerId = connectionManager.GetSelectServerGroupId();
                for (int i = 0; i < packets.Length; i++)
                {
                    // 내 서버
                    if (i == selectedServerId)
                    {
                        serverRank = packets[i].serverRank;
                    }
                    else
                    {
                        // rank가 존재하는 서버가 있을 경우
                        if (packets[i].serverRank != 0)
                            isPerfectWin = false;
                    }
                }

                view.SetEventView(seasonSeq, remainTime, nextRemainTime, myRank, winCount, serverRewards);
                view.SetEventViewRewardStep(rewardStep);
                EventDualBuffData myPerfectRank = isPerfectWin ? eventDualBuffDataRepo.GetPerfectReward() : null;
                EventDualBuffData myNormalRank = isPerfectWin ? null : eventDualBuffDataRepo.GetNormalReward(serverRank);
                view.ShowEventViewRank(myPerfectRank, myNormalRank);
            }
            else if (state == EventDuelState.NotInProgress)
            {
                string notice = LocalizeKey._47860.ToText(); // 이벤트 기간이 아닙니다.
                UI.ConfirmPopup(notice);
                UpdateView();
            }
            else
            {
                string notice = LocalizeKey._47862.ToText(); // 잠시 후 다시 이용해 주십시오.
                UI.ConfirmPopup(notice);
                UpdateView();
            }
        }

        private async Task AsyncRequestEventReward(int step)
        {
            int nextStep = step + 1; // 다음 스텝
            bool result = await duelModel.RequestEventReward(nextStep);
            if (!result)
                return;

            view.SetEventViewRewardStep(nextStep); // 스텝 세팅
        }

        private async Task RequestDuelArenaInfo(bool isShowIndicator)
        {
            if (isShowIndicator)
                UI.ShowIndicator();

            Response response = await Protocol.REQUEST_ARENA_INFO.SendAsync();
            if (isShowIndicator)
                UI.HideIndicator();

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                UpdateView();
                return;
            }

            const int IN_PROGRESS = 1; // 진행 중
            const int IN_NOT_PROGRESS = 2; // 진행 종료 (준비 기간)

            int flag = response.GetInt("3");
            long remainTime = response.GetLong("99"); // 남은 시간

            // 방어코드
            if (remainTime == 0L)
            {
                string message = LocalizeKey._435.ToText();
                UI.ConfirmPopup(message);
                UpdateView();
                return;
            }

            if (flag == IN_PROGRESS)
            {
                DuelArenaInfoPacket duelArenaInfoPacket = response.GetPacket<DuelArenaInfoPacket>("1");

                // 듀얼 기록 세팅
                CharDuelHistory[] histories = response.ContainsKey("2") ? response.GetPacketArray<CharDuelHistory>("2") : System.Array.Empty<CharDuelHistory>();
                for (int i = 0; i < histories.Length; i++)
                {
                    histories[i].Initialize(profileDataRepo);
                }
                System.Array.Sort(histories, (a, b) => a.InsertDate > b.InsertDate ? -1 : 1);

                arenaPoint = duelArenaInfoPacket.ArenaPoint;
                arenaWinCount = duelArenaInfoPacket.WinCount;
                arenaLoseCount = duelArenaInfoPacket.LoseCount;
                view.SetArenaView(remainTime, histories);
                view.SetArenaViewPoint(arenaPoint);
                view.SetArenaViewRewardStep(duelArenaInfoPacket.RewardStep);
            }
            else if (flag == IN_NOT_PROGRESS)
            {
                view.ShowArenaViewCalculateNotice(remainTime);
            }
        }

        private async Task AsyncRequestArenaPointBuy()
        {
            string title = LocalizeKey._47878.ToText(); // 아레나 깃발 구매
            string message = LocalizeKey._47879.ToText(); // 아레나 깃발을 구매하시겠습니까?
            if (!await UI.CostPopup(CoinType.CatCoin, arenaFlagCatCoin, title, message))
                return;

            int result = await duelModel.RequestArenaPointBuy();

            // 실패
            if (result == -1)
                return;

            arenaPoint = result;
            view.SetArenaViewPoint(arenaPoint);

            UI.ShowToastPopup(LocalizeKey._47880.ToText()); // 아레나 깃발을 구매하였습니다.
        }

        private async Task AsyncRequestArenaReward(int step)
        {
            int nextStep = step + 1; // 다음 스텝
            int result = await duelModel.RequestArenaReward(nextStep);

            // 실패
            if (result == -1)
                return;

            arenaPoint = result;

            view.SetArenaViewPoint(arenaPoint);
            view.SetArenaViewRewardStep(nextStep); // 스텝 세팅
        }

        public Job GetJobGroup()
        {
            JobData jobGroupData = jobDataRepo.GetJobGroup((int)characterModel.Job);
            return jobGroupData.id.ToEnum<Job>();
        }

        public string GetJobGroupName()
        {
            JobData jobGroupData = jobDataRepo.GetJobGroup((int)characterModel.Job);
            return jobGroupData.name_id.ToText();
        }

        public bool IsOpenContent()
        {
            return questModel.IsOpenContent(ContentType.Duel, isShowPopup: false);
        }

        public string OpenContentText()
        {
            return questModel.OpenContentText(ContentType.Duel);
        }

        public UIDuel.State GetRecentlyState()
        {
            return duelModel.SelectedState;
        }

        public RemainTime GetDuelListWaitingTime(UIDuel.State state)
        {
            return duelModel.GetDuelListWaitingTime(state);
        }

        public Job GetJob()
        {
            return characterModel.Job;
        }

        public Gender GetGender()
        {
            return characterModel.Gender;
        }

        public string GetProfileName()
        {
            return characterModel.GetProfileName();
        }

        public string GetName()
        {
            return characterModel.Name;
        }

        public string GetHexCid()
        {
            return characterModel.CidHex;
        }

        public int GetArenaPoint()
        {
            return arenaPoint;
        }

        public int GetArenaWinCount()
        {
            return arenaWinCount;
        }

        public int GetArenaLoseCount()
        {
            return arenaLoseCount;
        }

        void OnChangeCharacterJob(bool isInit)
        {
            OnUpdateCharacterInfo?.Invoke();
        }

        void OnChangeCharacterName(string name)
        {
            OnUpdateCharacterInfo?.Invoke();
        }
    }
}