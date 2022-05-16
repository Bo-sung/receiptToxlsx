using System.Threading.Tasks;
using System.Linq;

namespace Ragnarok
{
    public class DuelPlayerListPresenter : ViewPresenter
    {
        private readonly UIDuelPlayerList view;
        private readonly DuelModel duelModel;
        private readonly AgentModel agentModel;
        private readonly ProfileDataManager profileDataRepo;
        private readonly DuelArenaDataManager duelArenaDataRepo;

        private UIDuel.State state;

        // <!-- Chapter --!>
        private ChapterDuelState curDuelState;
        private int curListAlphabelIndex;

        // <!-- Event --!>
        private int serverId;

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

        public event System.Action OnResetWaitingTime;

        public DuelPlayerListPresenter(UIDuelPlayerList view)
        {
            this.view = view;

            duelModel = Entity.player.Duel;
            agentModel = Entity.player.Agent;
            profileDataRepo = ProfileDataManager.Instance;
            duelArenaDataRepo = DuelArenaDataManager.Instance;
        }

        public override void AddEvent()
        {
            duelModel.OnDualPointChanged += OnDuelPointChanged;
            agentModel.OnUpdateAgentEquipmentState += UpdateCombatAgents;
            duelModel.OnUpdateDuelCharList += OnUpdateDuelCharList;
            duelModel.OnResetWaitingTime += InvokeResetWaitingTime;
            duelModel.OnResetEventWaitingTime += InvokeResetWaitingTime;
            duelModel.OnResetArenaWaitingTime += InvokeResetWaitingTime;
        }

        public override void RemoveEvent()
        {
            duelModel.OnDualPointChanged -= OnDuelPointChanged;
            agentModel.OnUpdateAgentEquipmentState -= UpdateCombatAgents;
            duelModel.OnUpdateDuelCharList -= OnUpdateDuelCharList;
            duelModel.OnResetWaitingTime -= InvokeResetWaitingTime;
            duelModel.OnResetEventWaitingTime -= InvokeResetWaitingTime;
            duelModel.OnResetArenaWaitingTime -= InvokeResetWaitingTime;
        }

        public void OnShowChapter(ChapterDuelState duelState, int alphabetIndex)
        {
            state = UIDuel.State.Chapter;
            curDuelState = duelState;
            curListAlphabelIndex = alphabetIndex;

            view.SetDualPoint(duelModel.DuelPoint);
            view.SetPlayerInfo(duelModel.CurDuelInfo.WinCount, duelModel.CurDuelInfo.DefeatCount);
            view.SetAlphabet(GetJobClass(alphabetIndex), duelState.CurDuelRewardData.color_index, duelState.DuelWord[alphabetIndex]);
            view.SetCombatAgents(agentModel.GetEquipedCombatAgents().ToArray(), agentModel.CombatAgentSlotCount);
            RequestPlayerList();
        }

        public void OnShowEvent(int serverId)
        {
            state = UIDuel.State.Event;
            this.serverId = serverId;

            view.SetDualPoint(duelModel.DuelPoint);
            view.SetPlayerInfo(0, 0);
            view.UseAlphabet(this.serverId, BasisType.EVENT_DUEL_ALPHABET.GetInt(this.serverId));
            view.SetCombatAgents(agentModel.GetEquipedCombatAgents().ToArray(), agentModel.CombatAgentSlotCount);
            RequestPlayerList();
        }

        public void OnShowArena(int arenaPoint)
        {
            state = UIDuel.State.Arena;
            
            view.SetDualPoint(duelModel.DuelPoint);
            view.SetArena(arenaPoint);
            view.SetCombatAgents(agentModel.GetEquipedCombatAgents().ToArray(), agentModel.CombatAgentSlotCount);
            RequestPlayerList();
        }

        private Job GetJobClass(int alphabetIndex)
        {
            int classIndex = alphabetIndex % 4;
            if (classIndex == 0)
                return Job.Swordman; // 검사
            else if (classIndex == 1)
                return Job.Magician; // 마법사
            else if (classIndex == 2)
                return Job.Thief; // 도둑
            else if (classIndex == 3)
                return Job.Archer; // 궁수
            return Job.Swordman;
        }

        private void RequestPlayerList()
        {
            switch (state)
            {
                case UIDuel.State.Chapter:
                    duelModel.RequestDuelCharList(curDuelState.Chapter, curListAlphabelIndex).WrapNetworkErrors();
                    break;

                case UIDuel.State.Event:
                    RequestDuelWorldCharacterList(serverId).WrapNetworkErrors();
                    break;

                case UIDuel.State.Arena:
                    RequestArenaMatchingList().WrapNetworkErrors();
                    break;

                default:
                    throw new System.InvalidOperationException($"유효하지 않은 처리: {nameof(UIDuel.State)} = {state}");
            }
        }

        void OnUpdateDuelCharList(DuelCharList[] duelCharLists)
        {
            view.ShowPlayers(duelCharLists);
        }

        void InvokeResetWaitingTime()
        {
            OnResetWaitingTime?.Invoke();
        }

        public void ViewEventHandler(UIDuelPlayerList.EventType eventType, object data)
        {
            if (eventType == UIDuelPlayerList.EventType.OnClickBattle)
            {
                if (UIBattleMatchReady.IsMatching)
                {
                    string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                    UI.ShowToastPopup(message);
                    return;
                }

                if (BattleManager.Instance.Mode == BattleMode.MultiMazeLobby)
                {
                    UI.ShowToastPopup(LocalizeKey._90226.ToText()); // 미로섬에서는 입장할 수 없습니다.\n사냥 필드로 이동해주세요.
                    return;
                }

                switch (state)
                {
                    case UIDuel.State.Chapter:
                        if (curDuelState.IsOwningAlphabet(curListAlphabelIndex))
                        {
                            UI.ShowToastPopup(LocalizeKey._47826.ToText()); // 이미 소지한 문자입니다.
                            return;
                        }
                        break;

                    case UIDuel.State.Event:
                    case UIDuel.State.Arena:
                        break;

                    default:
                        throw new System.InvalidOperationException($"유효하지 않은 처리: {nameof(UIDuel.State)} = {state}");
                }

                if (Entity.player.Duel.DuelPoint < BasisType.ENTER_DUEL_POINT.GetInt())
                {
                    UI.ShowToastPopup(LocalizeKey._47827.ToText()); // 듀얼 포인트가 부족합니다.
                    return;
                }

                UIDuelPlayerListSlot.IInput target = (UIDuelPlayerListSlot.IInput)data;
                StartDuel(target.CID, target.UID).WrapNetworkErrors();
            }
            else if (eventType == UIDuelPlayerList.EventType.OnClickCharge)
            {
                RequestBuyDuelPoint().WrapNetworkErrors();
            }
            else if (eventType == UIDuelPlayerList.EventType.OnClickRefresh)
            {
                RequestPlayerList();
            }
            else if (eventType == UIDuelPlayerList.EventType.OnClickAgent)
            {
                if (!Entity.player.Quest.IsOpenContent(ContentType.CombatAgent))
                    return;

                UI.Show<UIAgent>(new UIAgent.Input() { viewAgentType = AgentType.CombatAgent });
            }
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

        private void OnDuelPointChanged(int duelPoint)
        {
            view.SetDualPoint(duelPoint);
        }

        private async Task StartDuel(int cid, int uid)
        {
            // 동료 장착이 가능한 경우
            if (agentModel.CanEquipAgent())
            {
                // 장착 가능한 PVP 동료가 있습니다.\n\n현재 상태로 진행하시겠습니까?
                // [동료 장착 바로가기]
                if (!await UI.SelectShortCutPopup(LocalizeKey._90173.ToText(), LocalizeKey._90174.ToText(), ShowCombatAgentUI))
                    return;
            }

            Analytics.TrackEvent(TrackType.Duel);
            switch (state)
            {
                case UIDuel.State.Chapter:
                    duelModel.RequestStartDuel(curDuelState, curListAlphabelIndex, cid, uid);
                    break;

                case UIDuel.State.Event:
                    duelModel.RequestStartEventDuel(serverId, cid, uid);
                    break;

                case UIDuel.State.Arena:
                    duelModel.RequestStartArenaDuel(cid, uid);
                    break;

                default:
                    throw new System.InvalidOperationException($"유효하지 않은 처리: {nameof(UIDuel.State)} = {state}");
            }
        }

        void ShowCombatAgentUI()
        {
            if (!Entity.player.Quest.IsOpenContent(ContentType.CombatAgent))
                return;
            UI.Show<UIAgent>(new UIAgent.Input() { viewAgentType = AgentType.CombatAgent });
        }

        /// <summary>
        /// 동료 장착 가능 여부
        /// </summary>
        public bool CanEquipAgent()
        {
            return agentModel.CanEquipAgent();
        }

        private void UpdateCombatAgents()
        {
            view.SetCombatAgents(agentModel.GetEquipedCombatAgents().ToArray(), Entity.player.Agent.CombatAgentSlotCount);
        }

        public RemainTime GetDuelListWaitingTime()
        {
            return duelModel.GetDuelListWaitingTime(state);
        }

        public int GetArenaNameId(int arenaPoint)
        {
            DuelArenaData find = duelArenaDataRepo.Find(arenaPoint);
            return find == null ? 0 : find.NameId;
        }

        private async Task RequestDuelWorldCharacterList(int serverId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", serverId);
            Response response = await Protocol.REQUEST_DUELWORLD_CHAR_LIST.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            duelModel.StartCooldownEventDuelCharList();
            DuelServerCharacterPacket[] packets = response.ContainsKey("1") ? response.GetPacketArray<DuelServerCharacterPacket>("1") : System.Array.Empty<DuelServerCharacterPacket>();
            foreach (var item in packets)
            {
                item.Initialize(profileDataRepo);
            }

            view.ShowPlayers(packets);

            InvokeResetWaitingTime();
        }

        private async Task RequestArenaMatchingList()
        {
            Response response = await Protocol.REQUEST_ARENA_MATCHING_LIST.SendAsync();
            if (!response.isSuccess)
            {
                // 진행중이 아니다.
                if (response.resultCode == ResultCode.NOT_IN_PROGRESS)
                {
                    string message = LocalizeKey._47881.ToText(); // 아레나가 종료되었습니다.
                    UI.ConfirmPopup(message, view.CloseUI);
                }
                // 최대 횟수를 초과하였습니다.
                else if (response.resultCode == ResultCode.NOT_ENOUGHT_COUNT)
                {
                    string message = LocalizeKey._47882.ToText(); // 아레나 깃발이 부족합니다.
                    UI.ConfirmPopup(message, view.CloseUI);
                }
                else
                {
                    response.ShowResultCode();
                }

                return;
            }

            duelModel.StartCooldownArenaDuelCharList();

            int arenaPoint = response.GetInt("1");
            DuelCharList[] packets = response.ContainsKey("2") ? response.GetPacketArray<DuelCharList>("2") : System.Array.Empty<DuelCharList>();
            foreach (var item in packets)
            {
                item.Initialize(profileDataRepo);
            }

            view.SetArena(arenaPoint);
            view.ShowPlayers(packets);
        }
    }
}