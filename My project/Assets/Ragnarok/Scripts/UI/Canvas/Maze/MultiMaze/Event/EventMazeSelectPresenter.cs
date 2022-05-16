using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIEventMazeSelect"/>
    /// </summary>
    public sealed class EventMazeSelectPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly DungeonModel dungeonModel;

        // <!-- Managers --!>
        private readonly BattleManager battleManager;

        // <!-- Repositories --!>
        private readonly int buyEventMazeTicketCatCoin; // 미로 유료입장 비용
        private readonly int buyEventMazeTicketIncCatCoin; // 미로 유료입장 비용 가중치
        private readonly int eventPlayMilliSecondsTime; // 난전 플레이 시간
        public readonly string[] eventFreeFightOpenTimes; // 난전 입장시간
        public readonly UIEventMazeSkill.IInput[] eventFreeFightSkills; // 난전 스킬
        public UIFreeFightReward.IInput[] eventFreeFightRewards; // 난전 보상

        // <!-- Event --!>
        public event System.Action OnUpdateEventMultiMazeTicket
        {
            add { dungeonModel.OnUpdateEventMultiMazeTicket += value; }
            remove { dungeonModel.OnUpdateEventMultiMazeTicket -= value; }
        }
        public event System.Action OnUpdateEventFreeFightTime;

        // <!-- Temps --!>
        private int mazeId;
        private System.DateTime startDateTime;
        private System.DateTime endDateTime;
        private bool isStartBattle;

        public EventMazeSelectPresenter()
        {
            dungeonModel = Entity.player.Dungeon;
            battleManager = BattleManager.Instance;

            buyEventMazeTicketCatCoin = BasisType.EVENT_MULTI_MAZE_CAT_COIN_JOIN.GetInt();
            buyEventMazeTicketIncCatCoin = BasisType.EVENT_MULTI_MAZE_CAT_COIN_INC.GetInt();

            eventPlayMilliSecondsTime = BasisType.EVENT_FF_PLAY_TIEM.GetInt();
            var list = BasisType.EVENT_FREE_FIGHT_OPEN_HOUR.GetKeyList();
            eventFreeFightOpenTimes = new string[list.Count];
            for (int i = 0; i < eventFreeFightOpenTimes.Length; i++)
            {
                // 참조: https://www.freeformatter.com/epoch-timestamp-to-date-converter.html
                int epochTime = BasisType.EVENT_FREE_FIGHT_OPEN_HOUR.GetInt(list[i]);
                System.TimeSpan startTime = System.TimeSpan.FromSeconds(epochTime); // UTC 시작시간 (시,분,초 만 사용)
                System.DateTime startLocalTime = ((long)startTime.TotalMilliseconds).ToDateTime();
                System.DateTime endLocalTime = startLocalTime.AddMilliseconds(eventPlayMilliSecondsTime);

                eventFreeFightOpenTimes[i] = StringBuilderPool.Get()
                    .Append(startLocalTime.ToString("HH:mm"))
                    .Append(" ~ ")
                    .Append(endLocalTime.ToString("HH:mm"))
                    .Release();
            }

            SkillDataManager skillDataRepo = SkillDataManager.Instance;
            FreeFightConfig config = FreeFightConfig.CHRISTMAS;
            int length = config.useSkills == null ? 0 : config.useSkills.Length;
            eventFreeFightSkills = new UIEventMazeSkill.IInput[length];
            for (int i = 0; i < eventFreeFightSkills.Length; i++)
            {
                eventFreeFightSkills[i] = skillDataRepo.Get(config.useSkills[i], level: 1);
            }

            FreeFightRewardDataManager freeFightRewardDataRepo = FreeFightRewardDataManager.Instance;
            eventFreeFightRewards = freeFightRewardDataRepo.GetArrayData(FreeFightEventType.Christmas);
        }

        public override void AddEvent()
        {
            BattleManager.OnStart += OnStartBattle;
        }

        public override void RemoveEvent()
        {
            BattleManager.OnStart -= OnStartBattle;
        }

        void OnStartBattle(BattleMode mode)
        {
            if (isStartBattle)
                CloseUI();
        }

        /// <summary>
        /// 현재 미로 아이디 저장
        /// </summary>
        public void SetMazeId(int mazeId)
        {
            this.mazeId = mazeId;
        }

        /// <summary>
        /// 크리스마스 미로 무료 입장 수
        /// </summary>
        public int GetTicketCount()
        {
            return dungeonModel.GetFreeEntryCount(DungeonType.EventMultiMaze);
        }

        /// <summary>
        /// 크리스마스 미로 최대 입장 수
        /// </summary>
        public int GetTicketMaxCount()
        {
            return dungeonModel.GetFreeEntryMaxCount(DungeonType.EventMultiMaze);
        }

        /// <summary>
        /// 크리스마스 미로 필요 냥다래
        /// </summary>
        public int GetNeedCatCoint()
        {
            int entryCount = dungeonModel.GetEntryCount(DungeonType.EventMultiMaze); // 실제 입장 횟수
            int overCount = entryCount - dungeonModel.GetFreeEntryMaxCount(DungeonType.EventMultiMaze); // 추가 입장 하려는 횟수
            int needCoin = buyEventMazeTicketCatCoin + (buyEventMazeTicketIncCatCoin * overCount); // 필요 냥다래
            return needCoin;
        }

        /// <summary>
        /// 크리스마스 난전 시작 시간
        /// </summary>
        public System.DateTime GetStartTime()
        {
            return startDateTime;
        }

        /// <summary>
        /// 크리스마스 난전 종료 시간
        /// </summary>
        public System.DateTime GetEndTime()
        {
            return endDateTime;
        }

        /// <summary>
        /// 크리스마스 난전 정보 호출
        /// </summary>
        public void RequestFreeFightInfo()
        {
            AsyncRequestEventFreeFightInfo().WrapNetworkErrors();
        }

        /// <summary>
        /// 크리스마스 미로 시작
        /// </summary>
        public void StartEventMaze()
        {
            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return;
            }

            // 티켓 & 냥다래 체크  
            if (!dungeonModel.IsFreeEntry(DungeonType.EventMultiMaze))
            {
                int needCoin = GetNeedCatCoint();
                if (!CoinType.CatCoin.Check(needCoin))
                    return;
            }

            CloseUI();
            UI.Show<UIBattleMatchReady>(new UIBattleMatchReady.Input { id = mazeId });
        }

        /// <summary>
        /// 크리스마스 난전 시작
        /// </summary>
        public void StartEventFreeFight()
        {
            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return;
            }

            StartBattle(BattleMode.ChristmasFreeFight); // 크리스마스 난전 입장
        }

        private void StartBattle(BattleMode battleMode)
        {
            isStartBattle = true;
            battleManager.StartBattle(battleMode, mazeId);
        }

        private void CloseUI()
        {
            UI.Close<UIEventMazeSelect>();
        }

        private async Task AsyncRequestEventFreeFightInfo()
        {
            Response response = await Protocol.REQUEST_FF_EVENTNOTICETIME.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            long serverTime = response.GetLong("1"); // 서버 현재 시각
            long startTime = response.GetLong("2"); // 서버 시작 시각
            ServerTime.Initialize(serverTime); // 서버 시간 세팅

            startDateTime = startTime.ToDateTime();
            endDateTime = startDateTime.AddMilliseconds(eventPlayMilliSecondsTime);

            OnUpdateEventFreeFightTime?.Invoke();
        }
    }
}