using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIBattleMatchReady"/>
    /// </summary>
    public class BattleMatchReadyPresenter : ViewPresenter
    {
        public enum MatchReadyState
        {
            None,
            TryReady,
            Ready,
            TryCancel,
        }

        // <!-- Models --!>
        private readonly DungeonModel dungeonModel;

        // <!-- Repositories --!>
        private readonly MultiMazeDataManager multiMazeDataRepo;

        // <!-- Managers --!>
        private readonly BattleManager battleManager;
        private readonly UIManager uiManager;
        private readonly ConnectionManager connectionManager;

        // <!-- Event --!>
        public event System.Action OnStartOtherBattle;
        public event System.Action<MatchReadyState> OnUpdateMatchReadyState;
        public event System.Action<int> OnUpdateMatchingCharacterCount;

        private MultiMazeData multiMazeData;

        public BattleMatchReadyPresenter()
        {
            dungeonModel = Entity.player.Dungeon;
            multiMazeDataRepo = MultiMazeDataManager.Instance;
            battleManager = BattleManager.Instance;
            uiManager = UIManager.Instance;
            connectionManager = ConnectionManager.Instance;
        }

        public override void AddEvent()
        {
            BattleManager.OnStart += OnStartBattle;
            Protocol.RECEIVE_MATCHMULMAZE_ROOM_JOIN.AddEvent(OnReceiveMatchMultiMazeRoomJoin);
            Protocol.RECEIVE_MATMULMAZE_WAITINGUSERNUM.AddEvent(OnReceiveMatchMultiMazeWaitingUserCount);
            connectionManager.OnLostConnect += OnLostConnect;
        }

        public override void RemoveEvent()
        {
            BattleManager.OnStart -= OnStartBattle;
            Protocol.RECEIVE_MATCHMULMAZE_ROOM_JOIN.RemoveEvent(OnReceiveMatchMultiMazeRoomJoin);
            Protocol.RECEIVE_MATMULMAZE_WAITINGUSERNUM.RemoveEvent(OnReceiveMatchMultiMazeWaitingUserCount);
            connectionManager.OnLostConnect -= OnLostConnect;
        }

        public void Initialize(int id)
        {
            multiMazeData = multiMazeDataRepo.Get(id);
        }

        public int GetMaxMatchingUser()
        {
            if (multiMazeData == null)
                return 0;

            return multiMazeData.GetMaxMatchingUser();
        }

        public void RequestMatchMulti()
        {
            if (multiMazeData == null)
            {
                OnUpdateMatchReadyState(MatchReadyState.None);
                return;
            }

            if (multiMazeData.IsEvent())
            {
                switch (multiMazeData.id)
                {
                    case MultiMazeWaitingRoomData.MULTI_MAZE_LOBBY_CHRISTMAS_EVENT:
                        AsyncRequestReadyEventMatchMulti(multiMazeData.id).WrapNetworkErrors();
                        break;

                    case MultiMazeWaitingRoomData.MULTI_MAZE_LOBBY_DARK_MAZE_EVENT:
                        AsyncRequestReadyEventDarkMaze(multiMazeData.id).WrapNetworkErrors();
                        break;
                }
            }
            else
            {
                AsyncRequestReadyMatchMulti(multiMazeData.id).WrapErrors();
            }
        }

        public void RequestCancelMatchMulti(bool isAuto)
        {
            AsyncRequestCancelMatchMulti(multiMazeData.id, isAuto).WrapErrors();
        }

        private async Task AsyncRequestReadyMatchMulti(int id)
        {
            OnUpdateMatchReadyState(MatchReadyState.TryReady); // 매칭 시작
            Response response = await dungeonModel.RequestMultimazeMatchStart(id);

            if (!response.isSuccess)
            {
                OnUpdateMatchReadyState(MatchReadyState.None); // 매칭 실패
                response.ShowResultCode();
                return;
            }

            OnUpdateMatchReadyState(MatchReadyState.Ready); // 매칭 상태
        }

        private async Task AsyncRequestReadyEventMatchMulti(int id)
        {
            OnUpdateMatchReadyState(MatchReadyState.TryReady); // 매칭 시작
            Response response = await dungeonModel.RequestEventMultimazeMatchStart(id);

            if (!response.isSuccess)
            {
                OnUpdateMatchReadyState(MatchReadyState.None); // 매칭 실패
                response.ShowResultCode();
                return;
            }

            OnUpdateMatchReadyState(MatchReadyState.Ready); // 매칭 상태
        }

        private async Task AsyncRequestReadyEventDarkMaze(int id)
        {
            OnUpdateMatchReadyState(MatchReadyState.TryReady); // 매칭 시작
            Response response = await dungeonModel.RequestDarkmazeMatchStart(id);

            if (!response.isSuccess)
            {
                OnUpdateMatchReadyState(MatchReadyState.None); // 매칭 실패
                response.ShowResultCode();
                return;
            }

            OnUpdateMatchReadyState(MatchReadyState.Ready); // 매칭 상태
        }

        private async Task AsyncRequestCancelMatchMulti(int id, bool isAuto)
        {
            OnUpdateMatchReadyState(MatchReadyState.TryCancel); // 취소 시작

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", id); // 가고자 하는 id
            Response response = await Protocol.REQUEST_MULMAZE_MATCH_CANCEL.SendAsync(sfs);
            if (!response.isSuccess)
            {
                OnUpdateMatchReadyState(MatchReadyState.Ready); // 취소 실패
                response.ShowResultCode();
                return;
            }

            if (isAuto)
            {
                string message = LocalizeKey._90233.ToText(); // 매칭 시간이 오래 경과되어 자동으로 매칭이 종료되었습니다.
                UI.ShowToastPopup(message);
            }

            OnUpdateMatchReadyState(MatchReadyState.None); // 매칭 상태
        }

        void OnStartBattle(BattleMode mode)
        {
            OnStartOtherBattle?.Invoke();
        }

        void OnReceiveMatchMultiMazeRoomJoin(Response response)
        {
            uiManager.ShortCut(); // 모든 UI 닫기
            Tutorial.Abort(); // 튜토리얼 중지

            MatchMultiMazePacket packet = response.GetPacket<MatchMultiMazePacket>();
            packet.SetMultiMazeId(multiMazeData.id);

            if (multiMazeData.IsEvent())
            {
                switch (multiMazeData.id)
                {
                    case MultiMazeWaitingRoomData.MULTI_MAZE_LOBBY_CHRISTMAS_EVENT:
                        battleManager.StartBattle(BattleMode.ChristmasMatchMultiMaze, packet);
                        break;

                    case MultiMazeWaitingRoomData.MULTI_MAZE_LOBBY_DARK_MAZE_EVENT:
                        battleManager.StartBattle(BattleMode.DarkMaze, packet);
                        break;
                }
            }
            else
            {
                battleManager.StartBattle(BattleMode.MatchMultiMaze, packet);
            }
        }

        void OnReceiveMatchMultiMazeWaitingUserCount(Response response)
        {
            int count = response.GetInt("1"); // 매칭된 인원수
            OnUpdateMatchingCharacterCount?.Invoke(count);
        }

        void OnLostConnect()
        {
            OnUpdateMatchReadyState(MatchReadyState.None); // 매칭 상태
        }
    }
}