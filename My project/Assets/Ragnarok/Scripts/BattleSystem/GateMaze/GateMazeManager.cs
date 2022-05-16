using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    using MonsterState = GateMazeEntry.MonsterState;
    using PlayerState = GateMazeEntry.PlayerState;

    /// <summary>
    /// <see cref="GateMazeEntry"/>
    /// <see cref="GateBossEntry"/>
    /// <see cref="GateWorldBossEntry"/>
    /// </summary>
    public sealed class GateMazeManager : Singleton<GateMazeManager>
    {
        public delegate void WorldBossHpEvent(int cid, int damage, int curHp, int maxHp);

        private readonly CharacterEntity player;

        private readonly DungeonModel dungeonModel;
        private readonly CharacterModel characterModel;

        private readonly GateDataManager gateDataRepo;
        private readonly ForestMonDataManager forestMonDataRepo;

        private readonly BotEntityPoolManager botEntityPool;
        private readonly BetterList<IMultiPlayerInput> multiPlayerList;

        public GateMazeState State { get; private set; } // 상태
        public GateMazeBattleResult BattleResult { get; private set; }

        public Vector3 PlayerPosition { get; private set; } // 캐릭터 위치
        public System.DateTime EndTime { get; private set; } // 종료 시간
        public int PlayerMaxHp { get; private set; } // 플레이어 MaxHp
        public int PlayerHp { get; private set; } // 플레이어 Hp
        public int MonsterKillMaxCount { get; private set; } // 몬스터 최대 처치 수
        public int MonsterKillCount { get; private set; } // 몬스터 처치 수

        public int BattlePlayerCid { get; private set; } // 전투중인 플레이어 cid
        public int BattleMonsterIndex { get; private set; } // 전투중인 몬스터 Index

        public long WorldBossMaxHp { get; private set; } // 월드보스 MaxHp
        public long WorldBossHp { get; private set; } // 월드보스 Hp

        public GateData CurrentData { get; private set; }
        public RewardData[] TotalRewards { get; private set; } // 최종보상

        public event System.Action OnUpdateState; // State 업데이트

        public event System.Action OnPlayerMove; // 플레이어 움직임
        public event System.Action OnPlayerDie; // 플레이어 죽음
        public event System.Action<int> OnPlayerHp; // 플레이어 대미지

        public event System.Action<PlayerBotEntity> OnMultiPlayerExit; // 멀티플레이어 퇴장
        public event System.Action<PlayerBotEntity> OnMultiPlayerMove; // 멀티플레이어 움직임
        public event System.Action<PlayerBotEntity> OnMultiPlayerWarp; // 멀티플레이어 워프
        public event System.Action<PlayerBotEntity> OnMultiPlayerState; // 멀티플레이어 상태변화
        public event System.Action<PlayerBotEntity, int> OnMultiPlayerHp; // 멀티플레이어 대미지

        public event System.Action<MonsterBotEntity> OnMultiMonsterStatus; // 몬스터 상태변화

        public event System.Action OnMonsterKillCount;
        public event System.Action OnWorldBossHp;

        private bool isInitialize;

        public GateMazeManager()
        {
            player = Entity.player;
            dungeonModel = player.Dungeon;
            characterModel = player.Character;

            gateDataRepo = GateDataManager.Instance;
            forestMonDataRepo = ForestMonDataManager.Instance;

            botEntityPool = BotEntityPoolManager.Instance;
            multiPlayerList = new BetterList<IMultiPlayerInput>();
        }

        protected override void OnTitle()
        {
            Dispose();
        }

        private void AddEvent()
        {
            Protocol.REQUEST_GATE_ROOM_TRANSFORM.AddEvent(OnUserTransform);
            Protocol.RECIEVE_GATE_ROOM_EXIT.AddEvent(OnUserExit);
            Protocol.RECEIVE_MULMAZE_ROOM_MONMOVE.AddEvent(OnMonsterMove);
            Protocol.RECEIVE_GATE_MINIBOSSBATTLE_START.AddEvent(OnBattleMiddleBoss);
            Protocol.REQUEST_GATE_MINIBOSSBATTLE_END.AddEvent(OnEndBattleMiddleBoss);
            Protocol.RECEIVE_GATE_MINIBOSSBATTLE_WIN.AddEvent(OnWinBattleMiddleBoss);
            Protocol.RECEIVE_GATE_MINIBOSSBATTLE_LOSE.AddEvent(OnLoseBattleMiddleBoss);
            Protocol.RECEIVE_GATE_BOSSBATTLE_START.AddEvent(OnBattleBoss);
            Protocol.RECEIVE_GATE_BOSSBATTLE_ATTACK.AddEvent(OnAttackBoss);
            Protocol.RECEIVE_GATE_USERDIE.AddEvent(OnUserDie);
            Protocol.RECEIVE_MULMAZE_MON_REGEN.AddEvent(OnMonsterRegen);
        }

        private void RemoveEvent()
        {
            Protocol.REQUEST_GATE_ROOM_TRANSFORM.RemoveEvent(OnUserTransform);
            Protocol.RECIEVE_GATE_ROOM_EXIT.RemoveEvent(OnUserExit);
            Protocol.RECEIVE_MULMAZE_ROOM_MONMOVE.RemoveEvent(OnMonsterMove);
            Protocol.RECEIVE_GATE_MINIBOSSBATTLE_START.RemoveEvent(OnBattleMiddleBoss);
            Protocol.REQUEST_GATE_MINIBOSSBATTLE_END.RemoveEvent(OnEndBattleMiddleBoss);
            Protocol.RECEIVE_GATE_MINIBOSSBATTLE_WIN.RemoveEvent(OnWinBattleMiddleBoss);
            Protocol.RECEIVE_GATE_MINIBOSSBATTLE_LOSE.RemoveEvent(OnLoseBattleMiddleBoss);
            Protocol.RECEIVE_GATE_BOSSBATTLE_START.RemoveEvent(OnBattleBoss);
            Protocol.RECEIVE_GATE_BOSSBATTLE_ATTACK.RemoveEvent(OnAttackBoss);
            Protocol.RECEIVE_GATE_USERDIE.RemoveEvent(OnUserDie);
            Protocol.RECEIVE_MULMAZE_MON_REGEN.RemoveEvent(OnMonsterRegen);
        }

        public void Initialize()
        {
            if (isInitialize)
                return;

            isInitialize = true;
            AddEvent();
        }

        public void Dispose()
        {
            if (!isInitialize)
                return;

            isInitialize = false;
            RemoveEvent();

            BattleResult = GateMazeBattleResult.None;
            PlayerMaxHp = 0;
            SetPlayerHp(0);
            MonsterKillCount = 0;
            MonsterKillMaxCount = 0;
            WorldBossMaxHp = 0;
            SetWorldBossHp(0L);

            CurrentData = null;
            TotalRewards = null;

            UpdateState(GateMazeState.None);

            botEntityPool.Clear();
            multiPlayerList.Clear();
        }

        /// <summary>
        /// 진행 중
        /// </summary>
        public bool IsJoined()
        {
            return State != GateMazeState.None;
        }

        /// <summary>
        /// 입장 처리
        /// </summary>
        public bool Enter(GateMultiMazePacket packet)
        {
            if (IsJoined())
                return false;

            int id = packet.gateId;
            CurrentData = gateDataRepo.Get(id);
            if (CurrentData == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"해당 id가 존재하지 않습니다: {nameof(id)} = {id}");
#endif
                return false;
            }

            MonsterKillMaxCount = CurrentData.boss_battle_condition;

            Quest.QuestProgress(QuestType.DUNGEON_TYPE_COUNT, DungeonType.Gate.ToIntValue()); // 특정던전 입장 횟수
            Quest.QuestProgress(QuestType.GATE_ENTER_COUNT, CurrentData.id); // 특정 게이트 도전 횟수
            dungeonModel.SetLastEnterMultiMazeLobbyId(CurrentData.GetMultiMazeDataId()); // 마지막 입장한 Lobby도 변경해 줌

            BattleMazeCharacterPacket[] characterPackets = packet.arrMultiMazePlayerPacket;
            ForestMazeMonsterPacket[] monsterPackets = packet.arrMazeMonsterPacket;

            foreach (var item in characterPackets)
            {
                // 플레이어가 아닐 경우에만 Bot 으로 처리
                if (!IsPlayer(item.Cid))
                {
                    multiPlayerList.Add(item);
                    continue;
                }

                // 플레이어 기본 정보 저장
                PlayerPosition = new Vector3(item.PosX, item.PosY, item.PosZ);
                PlayerMaxHp = item.MaxHp;
                SetPlayerHp(PlayerMaxHp);
            }

            // 추가 데이터 세팅
            foreach (var item in monsterPackets)
            {
                ForestMonData monsterData = forestMonDataRepo.Get(item.forestMonsterDataId);
                if (monsterData == null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"존재하지 않는 미궁숲몬스터 아이디 {nameof(item.forestMonsterDataId)} = {item.forestMonsterDataId}");
#endif
                    continue;
                }

                item.SetSpawnInfo(monsterData);
                item.SetMoveSpeed(MathUtils.ToPermyriadValue(monsterData.monster_speed));
            }

            WorldBossMaxHp = packet.worldBossHp;
            SetWorldBossHp(WorldBossMaxHp);

            botEntityPool.Create(multiPlayerList.ToArray());
            botEntityPool.Create(monsterPackets);

            ServerTime.Initialize(packet.currentTime); // 서버 시간 세팅
            EndTime = packet.endTime.ToDateTime(); // 종료 시간 세팅

            player.ResetSkillCooldown(); // 쿨타임 초기화
            SetMonsterKillCount(0); // 몬스터 처치 수 초기화
            UpdateState(GateMazeState.Maze); // Maze 상태로 변경
            return true;
        }

        /// <summary>
        /// 퇴장 처리
        /// </summary>
        public async Task<bool> Exit()
        {
            if (IsJoined())
            {
                Response response = await Protocol.REQUEST_GATE_ROOM_EXIT.SendAsync();
                if (!response.isSuccess)
                {
                    response.ShowResultCode();
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 플레이어 찾기
        /// </summary>
        public CharacterEntity FindPlayer(int cid)
        {
            return IsPlayer(cid) ? player : botEntityPool.FindPlayer(cid);
        }

        /// <summary>
        /// 몬스터 찾기
        /// </summary>
        public MonsterBotEntity FindMonster(int index)
        {
            return botEntityPool.FindMonster(index);
        }

        /// <summary>
        /// 플레이어 정보 반환
        /// </summary>
        public PlayerBotEntity[] GetPlayers()
        {
            return botEntityPool.GetPlayers();
        }

        /// <summary>
        /// 몬스터 정보 반환
        /// </summary>
        public MonsterBotEntity[] GetMonsters()
        {
            return botEntityPool.GetMonsters();
        }

        /// <summary>
        /// 멀티플레이어 정보 반환
        /// </summary>
        public IMultiPlayerInput[] GetMultiPlayers()
        {
            return multiPlayerList.ToArray();
        }

        /// <summary>
        /// 봇 재활용
        /// </summary>
        private void Recycle()
        {
            botEntityPool.Recycle();
        }

        /// <summary>
        /// 미로 오브젝트 회수
        /// </summary>
        private void Despawn()
        {
        }

        /// <summary>
        /// 플레이어 움직임
        /// </summary>
        public void RequestPlayerMove(Vector3 pos)
        {
            PlayerPosition = pos;

            int[] posArray = { (int)pos.x * 1000, (int)pos.y * 1000, (int)pos.z * 1000 };
            var sfs = Protocol.NewInstance();
            sfs.PutIntArray("2", posArray);
            Protocol.REQUEST_GATE_ROOM_TRANSFORM.SendAsync(sfs).WrapNetworkErrors();
        }

        void OnUserExit(Response response)
        {
            int cid = response.GetInt("1");
            bool isAllDied = cid == -1;
            bool isVictory = response.ContainsKey("V");

            // 플레이어의 경우에 한 함
            if (IsPlayer(cid) || isAllDied || isVictory)
            {
                UI.HideIndicator(); // 895:REQUEST_GATE_ROOM_EXIT 로 인한 Indicator 제거

                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    TotalRewards = UI.ConvertRewardData(charUpdateData.rewards);
                }

                SetResult(isVictory ? GateMazeBattleResult.Succees : GateMazeBattleResult.Fail);
            }

            if (isVictory)
            {
                Quest.QuestProgress(QuestType.DUNGEON_TYPE_CLEAR_COUNT, DungeonType.Gate.ToIntValue()); // 특정 타입 던전 클리어 횟수
            }

            UpdatePlayerExit(cid); // 플레이어 퇴장
        }

        void OnUserTransform(Response response)
        {
            int cid = response.GetInt("1");
            int[] arrayPosValue = response.GetIntArray("2");

            // 방어코드: 죽었을 때 움직이려 했는지는 모르겠지만 간혹 서버에서 "2" 파라미터가 안 올 때가 존재함
            if (arrayPosValue == null)
                return;

            Vector3 position = new Vector3(arrayPosValue[0] * 0.001f, arrayPosValue[1] * 0.001f, arrayPosValue[2] * 0.001f);
            UpdatePlayerMove(cid, position); // 플레이어 움직임
        }

        void OnMonsterMove(Response response)
        {
            int monsterIndex = response.GetByte("1");
            float prePosX = response.GetFloat("2");
            float prePosZ = response.GetFloat("3");
            float targetPosX = response.GetFloat("4");
            float targetPosZ = response.GetFloat("5");
            Vector3 prePos = new Vector3(prePosX, 0f, prePosZ);
            Vector3 targetPos = new Vector3(targetPosX, 0f, targetPosZ);
            const byte MONSTER_PATROL_STATE = (byte)MonsterState.Patrol;
            UpdateMonsterStatus(monsterIndex, MONSTER_PATROL_STATE, null, prePos, targetPos, isStartState: false); // 몬스터 상태 - 일반
        }

        void OnMonsterRegen(Response response)
        {
            int monsterIndex = response.GetByte("1");
            int? bossHp = null;
            float posX = response.GetFloat("3");
            float posZ = response.GetFloat("4");

            if (response.ContainsKey("2"))
                bossHp = response.GetInt("2");

            Vector3 targetPos = new Vector3(posX, 0f, posZ);
            const byte MONSTER_GENERAL_STATE = (byte)MonsterState.General;
            UpdateMonsterStatus(monsterIndex, MONSTER_GENERAL_STATE, bossHp, null, targetPos, isStartState: true); // 몬스터 상태 - 일반
        }

        void OnBattleMiddleBoss(Response response)
        {
            int cid = response.GetInt("1"); // 충돌한 유저
            int monsterIndex = response.GetByte("2"); // 충돌한 보스 인덱스
            UpdatePlayerMatchState(GateMazeState.MiddleBossBattle, cid, monsterIndex); // 플레이어 상태 - 전투
        }

        void OnBattleBoss(Response response)
        {
            UpdateState(GateMazeState.WorldBossBattle); // 상태변경: 최종보스 전투
        }

        void OnAttackBoss(Response response)
        {
            int cid = response.GetInt("1");
            int damage = response.GetInt("2");
            long remainBossHp = response.GetLong("3");
            SetWorldBossHp(remainBossHp);
        }

        void OnUserDie(Response response)
        {
            int cid = response.GetInt("1");
            UpdatePlayerDie(cid);
        }

        void OnEndBattleMiddleBoss(Response response)
        {
            // 중간보스 처치 시 호출된다 (방어코드)
            if (PlayerHp > 0)
            {
                SetMonsterKillCount(MonsterKillCount + 1); // 몬스터 처치 수 초기화
            }

            // 몬스터 수가 못 미쳤을 경우에 클라가 강제로 미로로 상태 변경
            if (MonsterKillCount < MonsterKillMaxCount)
            {
                UpdateState(GateMazeState.Maze); // 상태변경: 미로
            }
            else
            {
                // OnBattleBoss 올 때까지 대기
            }
        }

        void OnMonsterCrash(Response response)
        {
            int cid = response.GetInt("1");
            int? remainHp = null;
            int monsterIndex = response.GetByte("4");
            Vector3? warpPos = null;

            if (response.ContainsKey("3"))
            {
                remainHp = response.GetInt("3");
            }

            if (response.ContainsKey("5") && response.ContainsKey("6"))
            {
                float posX = response.GetFloat("5");
                float posZ = response.GetFloat("6");
                warpPos = new Vector3(posX, 0f, posZ);
            }

            if (remainHp.HasValue)
            {
                UpdatePlayerHp(cid, remainHp.Value);
            }

            // 플레이어의 움직임 존재
            if (warpPos.HasValue)
            {
                UpdatePlayerWarp(cid, warpPos.Value); // 플레이어 워프
            }

            byte MONSTER_DIE_STATE = (byte)MonsterState.Die;
            UpdateMonsterStatus(monsterIndex, MONSTER_DIE_STATE, 0, null, null, isStartState: false); // 몬스터 상태변화: 죽음
        }

        void OnWinBattleMiddleBoss(Response response)
        {
            int cid = response.GetInt("1");
            SetMonsterKillCount(MonsterKillCount + 1);
            UpdatePlayerGeneralState(cid);
        }

        void OnLoseBattleMiddleBoss(Response response)
        {
            int cid = response.GetInt("1");
            UpdatePlayerDie(cid);

            if (IsPlayer(cid))
            {
                UpdateState(GateMazeState.Maze); // 상태변경: 미로
            }
        }

        /// <summary>
        /// 상태 변경
        /// </summary>
        private void UpdateState(GateMazeState state)
        {
            if (State == state)
                return;

            State = state;

            switch (State)
            {
                case GateMazeState.None:
                    Recycle(); // 전체 봇 Recycle
                    break;

                case GateMazeState.Maze:
                    // 전투 관련 정보 초기화
                    BattlePlayerCid = -1; // 전투중인 플레이어
                    BattleMonsterIndex = -1; // 전투중인 몬스터
                    break;

                case GateMazeState.MiddleBossBattle:
                case GateMazeState.WorldBossBattle:
                    Despawn(); // 미로 오브젝트 회수
                    break;
            }

            OnUpdateState?.Invoke();
        }

        /// <summary>
        /// 결과 세팅
        /// </summary>
        private void SetResult(GateMazeBattleResult result)
        {
            BattleResult = result;
            UpdateState(GateMazeState.None); // 이미 퇴장했기 때문에 서버에 Exit를 날리지 않아야 함
        }

        /// <summary>
        /// 플레이어 여부
        /// </summary>
        private bool IsPlayer(int cid)
        {
            return cid == characterModel.Cid;
        }

        /// <summary>
        /// 플레이어 퇴장
        /// </summary>
        private void UpdatePlayerExit(int cid)
        {
            // 방어코드 (앞에서 처리 함)
            if (IsPlayer(cid))
            {
                UpdateState(GateMazeState.None); // 이미 퇴장했기 때문에 서버에 Exit를 날리지 않아야 함
                return;
            }

            RemoveMultiPlayer(cid);

            PlayerBotEntity find = botEntityPool.FindPlayer(cid);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 플레이어가 나감: {nameof(cid)} = {cid}");
#endif
                return;
            }

            OnMultiPlayerExit?.Invoke(find); // 반드시 회수 전에 호출할 것! (회수 후에는 Default 값으로 초기화된다.)

            botEntityPool.Recycle(find); // 회수
        }

        private void RemoveMultiPlayer(int cid)
        {
            int index = FindIndexMultiPlayer(cid);
            if (index == -1)
                return;

            multiPlayerList.RemoveAt(index);
        }

        private int FindIndexMultiPlayer(int cid)
        {
            for (int i = 0; i < multiPlayerList.size; i++)
            {
                if (multiPlayerList[i].Cid == cid)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// 플레이어 움직임
        /// </summary>
        private void UpdatePlayerMove(int cid, Vector3 position)
        {
            if (IsPlayer(cid))
            {
                PlayerPosition = position;
                OnPlayerMove?.Invoke();
                return;
            }

            PlayerBotEntity find = botEntityPool.FindPlayer(cid);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 플레이어가 움직임: {nameof(cid)} = {cid}");
#endif
                return;
            }

            find.SetBotPosition(position);
            OnMultiPlayerMove?.Invoke(find);
        }

        /// <summary>
        /// 플레이어 죽음
        /// </summary>
        private void UpdatePlayerDie(int cid)
        {
            if (IsPlayer(cid))
            {
                UI.HideIndicator(); // REQUEST_GATE_MINIBOSSBATTLE_END 의 응답이 여기로 온다.

                SetPlayerHp(0);
                OnPlayerDie?.Invoke();
                return;
            }

            PlayerBotEntity find = botEntityPool.FindPlayer(cid);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 플레이어가 죽음: {nameof(cid)} = {cid}");
#endif
                return;
            }

            byte state = (byte)PlayerState.Dead;

            find.SetBotState(state);
            find.SetBotCurHp(0);
            OnMultiPlayerState?.Invoke(find);
        }

        /// <summary>
        /// 전투 상태 변화
        /// </summary>
        private void UpdatePlayerMatchState(GateMazeState mazeState, int cid, int monsterIndex)
        {
            BattlePlayerCid = cid;
            BattleMonsterIndex = monsterIndex;

            if (IsPlayer(cid))
            {
                UpdateState(mazeState); // 상태변경: 최종보스 전투
                return;
            }

            PlayerBotEntity find = botEntityPool.FindPlayer(cid);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 플레이어가 전투: {nameof(cid)} = {cid}");
#endif
            }
            else
            {
                const byte BATTLE_BOSS_STATE = (byte)PlayerState.BattleBoss;
                find.SetBotState(BATTLE_BOSS_STATE);
                OnMultiPlayerState?.Invoke(find);
            }

            const byte MAZE_BATTLE_STATE = (byte)MonsterState.MazeBattle;
            UpdateMonsterStatus(monsterIndex, MAZE_BATTLE_STATE, null, null, null, isStartState: false); // 몬스터 상태 - 전투
        }

        /// <summary>
        /// 전투 상태 변화
        /// </summary>
        private void UpdatePlayerGeneralState(int cid)
        {
            // 방어코드
            if (IsPlayer(cid))
                return;

            PlayerBotEntity find = botEntityPool.FindPlayer(cid);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 플레이어가 전투: {nameof(cid)} = {cid}");
#endif
                return;
            }

            const byte GENERAL_STATE = (byte)PlayerState.General;
            find.SetBotState(GENERAL_STATE);
            OnMultiPlayerState?.Invoke(find);
        }

        /// <summary>
        /// 플레이어 Hp
        /// </summary>
        private void UpdatePlayerHp(int cid, int remainHp)
        {
            if (IsPlayer(cid))
            {
                SetPlayerHp(remainHp);
                OnPlayerHp?.Invoke(remainHp);
                return;
            }

            PlayerBotEntity find = botEntityPool.FindPlayer(cid);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 플레이어가 대미지: {nameof(cid)} = {cid}");
#endif
                return;
            }

            find.SetBotCurHp(remainHp);
            OnMultiPlayerHp?.Invoke(find, remainHp);
        }

        /// <summary>
        /// 플레이어 워프
        /// </summary>
        private void UpdatePlayerWarp(int cid, Vector3 position)
        {
            if (IsPlayer(cid))
            {
                PlayerPosition = position;
                OnPlayerMove?.Invoke();
                return;
            }

            PlayerBotEntity find = botEntityPool.FindPlayer(cid);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 플레이어가 워프: {nameof(cid)} = {cid}");
#endif
                return;
            }

            find.SetBotPosition(position);
            OnMultiPlayerWarp?.Invoke(find);
        }

        /// <summary>
        /// 몬스터 상태변화
        /// </summary>
        private void UpdateMonsterStatus(int monsterIndex, byte monsterState, int? remainHp, Vector3? prePos, Vector3? targetPos, bool isStartState)
        {
            MonsterBotEntity find = FindMonster(monsterIndex);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 몬스터가 상태변화: {nameof(monsterIndex)} = {monsterIndex}");
#endif
                return;
            }

            find.SetBotState(monsterState);

            if (isStartState)
            {
                if (remainHp.HasValue)
                {
                    find.SetBotCurHp(remainHp);
                }
                else
                {
                    find.SetBotCurHp(find.MaxHP);
                }
            }

            if (prePos.HasValue && targetPos.HasValue)
            {
                find.SetBotPosition(prePos.Value, targetPos.Value);
            }
            else if (targetPos.HasValue)
            {
                find.SetBotPosition(targetPos.Value);
            }

            OnMultiMonsterStatus?.Invoke(find);
        }

        /// <summary>
        /// 플레이어 Hp 세팅
        /// </summary>
        public void SetPlayerHp(int hp)
        {
            PlayerHp = hp;
        }

        /// <summary>
        /// 월드보스 Hp 세팅
        /// </summary>
        private void SetWorldBossHp(long hp)
        {
            WorldBossHp = hp;
            OnWorldBossHp?.Invoke();
        }

        /// <summary>
        /// 몬스터 킬 세팅
        /// </summary>
        private void SetMonsterKillCount(int count)
        {
            MonsterKillCount = Mathf.Clamp(count, 0, MonsterKillMaxCount);
            OnMonsterKillCount?.Invoke();
        }
    }
}