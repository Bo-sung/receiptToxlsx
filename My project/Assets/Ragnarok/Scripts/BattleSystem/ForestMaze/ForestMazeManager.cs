using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    using MonsterState = ForestMazeEntry.MonsterState;
    using PlayerState = ForestMazeEntry.PlayerState;

    public sealed class ForestMazeManager : Singleton<ForestMazeManager>
    {
        private readonly CharacterEntity player;

        private readonly DungeonModel dungeonModel;
        private readonly CharacterModel characterModel;

        private readonly ForestBaseDataManager forestBaseDataRepo;
        private readonly ForestMonDataManager forestMonDataRepo;
        private readonly ForestRewardDataManager forestRewardDataRepo;

        private readonly BotEntityPoolManager botEntityPool;
        private readonly MazeObjectPoolManager<HpPotionEntity> mazeHpPotionPool;
        private readonly MazeObjectPoolManager<EmperiumEntity> mazeEmperiumPool;
        private readonly BetterList<int> selectedRewardIds;
        public int NeedBossBattleEmperiumCount { get; private set; } // 보스도전 필요 엠펠리움 수
        public int MaxEmperiumCount { get; private set; } // 최대 획득 가능한 엠펠리움 수

        public ForestMazeState State { get; private set; } // 상태
        public ForestMazeBattleResult BattleResult { get; private set; }

        public Vector3 PlayerPosition { get; private set; } // 캐릭터 위치
        public System.DateTime EndTime { get; private set; } // 종료 시간
        public int PlayerMaxHp { get; private set; } // 플레이어 MaxHp
        public int PlayerHp { get; private set; } // 플레이어 Hp
        public int EmperiumCount { get; private set; } // 현재 엠펠리움
        public int Floor { get; private set; } // 현재 층
        public int BossMonsterIndex { get; private set; } // 보스 Index

        public int BattlePlayerCid { get; private set; } // 전투중인 플레이어 cid
        public int BattleMonsterIndex { get; private set; } // 전투중인 몬스터 Index

        private ForestBaseData[] currentArrData;
        public ForestBaseData CurrentData { get; private set; }
        public RewardData[] TotalRewards { get; private set; } // 최종보상

        public event System.Action OnUpdateState; // State 업데이트

        public event System.Action OnPlayerMove; // 플레이어 움직임
        public event System.Action OnPlayerDie; // 플레이어 죽음
        public event System.Action<int> OnPlayerHp; // 플레이어 대미지

        public event System.Action<PlayerBotEntity> OnMultiPlayerJoin; // 멀티플레이어 입장
        public event System.Action<PlayerBotEntity> OnMultiPlayerExit; // 멀티플레이어 퇴장
        public event System.Action<PlayerBotEntity> OnMultiPlayerMove; // 멀티플레이어 움직임
        public event System.Action<PlayerBotEntity> OnMultiPlayerWarp; // 멀티플레이어 워프
        public event System.Action<PlayerBotEntity> OnMultiPlayerState; // 멀티플레이어 상태변화
        public event System.Action<PlayerBotEntity, int> OnMultiPlayerHp; // 멀티플레이어 대미지

        public event System.Action<MonsterBotEntity> OnMultiMonsterStatus; // 몬스터 상태변화

        public event System.Action<HpPotionEntity, bool> OnGainedHpPotion; // 체력포션 획득
        public event System.Action<HpPotionEntity> OnRegenHpPotion; // 체력포션 리젠
        public event System.Action<EmperiumEntity, bool> OnGainedEmperium; // 엠펠리움 획득
        public event System.Action<EmperiumEntity> OnRegenEmperium; // 엠펠리움 리젠

        public event System.Action OnUpdateEmperium; // 엠펠리움 업데이트
        public event System.Action OnUpdateFloor; // 층 업데이트
        public event System.Action<int[]> OnSelectReward; // 중간보스 보상선택

        private bool isInitialize;

        public ForestMazeManager()
        {
            player = Entity.player;
            dungeonModel = player.Dungeon;
            characterModel = player.Character;

            forestBaseDataRepo = ForestBaseDataManager.Instance;
            forestMonDataRepo = ForestMonDataManager.Instance;
            forestRewardDataRepo = ForestRewardDataManager.Instance;

            botEntityPool = BotEntityPoolManager.Instance;
            mazeHpPotionPool = new MazeObjectPoolManager<HpPotionEntity>();
            mazeEmperiumPool = new MazeObjectPoolManager<EmperiumEntity>();
            selectedRewardIds = new BetterList<int>();
        }

        protected override void OnTitle()
        {
            Dispose();
        }

        private void AddEvent()
        {
            Protocol.RECEIVE_FOREST_ROOM_JOIN.AddEvent(OnUserJoin);
            Protocol.RECIEVE_FOREST_ROOM_EXIT.AddEvent(OnUserExit);
            Protocol.REQUEST_FOREST_ROOM_TRANSFORM.AddEvent(OnUserTransform);
            Protocol.RECEIVE_MULMAZE_ROOM_MONMOVE.AddEvent(OnMonsterMove);
            Protocol.RECEIVE_MULMAZE_MON_REGEN.AddEvent(OnMonsterRegen);

            Protocol.RECEIVE_FOREST_GET_EMPAL.AddEvent(OnGetEmperium);
            Protocol.RECEIVE_FOREST_GET_ITEM.AddEvent(OnGetHpPotion);
            Protocol.RECEIVE_FOREST_MINIBOSSBATTLE_START.AddEvent(OnBattleMiddleBoss);
            Protocol.RECEIVE_FOREST_BOSSBATTLE_START.AddEvent(OnBattleBoss);
            Protocol.REQUEST_FOREST_MINIBOSSBATTLE_END.AddEvent(OnEndBattleMiddleBoss);
            Protocol.RECEIVE_FOREST_USERDIE.AddEvent(OnUserDie);
            Protocol.RECEIVE_FOREST_NOMALMON_CRASH.AddEvent(OnMonsterCrash);
            Protocol.REQUEST_FOREST_UPSTORY.AddEvent(OnUpFloor);
            Protocol.REQUEST_FOREST_DOWNSTORY.AddEvent(OnDownFloor);
            Protocol.REQUEST_FOREST_ITEMSELECT.AddEvent(OnItemSelect);
            Protocol.RECEIVE_FOREST_EMPAL_REGEN.AddEvent(OnEmperiumRegen);
            Protocol.RECEIVE_FOREST_ITEM_REGEN.AddEvent(OnItemRegen);
            Protocol.RECEIVE_FOREST_MINIBOSSBATTLE_WIN.AddEvent(OnWinBattleMiddleBoss);
        }

        private void RemoveEvent()
        {
            Protocol.RECEIVE_FOREST_ROOM_JOIN.RemoveEvent(OnUserJoin);
            Protocol.RECIEVE_FOREST_ROOM_EXIT.RemoveEvent(OnUserExit);
            Protocol.REQUEST_FOREST_ROOM_TRANSFORM.RemoveEvent(OnUserTransform);
            Protocol.RECEIVE_MULMAZE_ROOM_MONMOVE.RemoveEvent(OnMonsterMove);
            Protocol.RECEIVE_MULMAZE_MON_REGEN.RemoveEvent(OnMonsterRegen);

            Protocol.RECEIVE_FOREST_GET_EMPAL.RemoveEvent(OnGetEmperium);
            Protocol.RECEIVE_FOREST_GET_ITEM.RemoveEvent(OnGetHpPotion);
            Protocol.RECEIVE_FOREST_MINIBOSSBATTLE_START.RemoveEvent(OnBattleMiddleBoss);
            Protocol.RECEIVE_FOREST_BOSSBATTLE_START.RemoveEvent(OnBattleBoss);
            Protocol.REQUEST_FOREST_MINIBOSSBATTLE_END.RemoveEvent(OnEndBattleMiddleBoss);
            Protocol.RECEIVE_FOREST_USERDIE.RemoveEvent(OnUserDie);
            Protocol.RECEIVE_FOREST_NOMALMON_CRASH.RemoveEvent(OnMonsterCrash);
            Protocol.REQUEST_FOREST_UPSTORY.RemoveEvent(OnUpFloor);
            Protocol.REQUEST_FOREST_DOWNSTORY.RemoveEvent(OnDownFloor);
            Protocol.REQUEST_FOREST_ITEMSELECT.RemoveEvent(OnItemSelect);
            Protocol.RECEIVE_FOREST_EMPAL_REGEN.RemoveEvent(OnEmperiumRegen);
            Protocol.RECEIVE_FOREST_ITEM_REGEN.RemoveEvent(OnItemRegen);
            Protocol.RECEIVE_FOREST_MINIBOSSBATTLE_WIN.RemoveEvent(OnWinBattleMiddleBoss);
        }

        public void Initialize()
        {
            if (isInitialize)
                return;

            isInitialize = true;
            AddEvent();

            NeedBossBattleEmperiumCount = BasisForestMazeInfo.NeedEmperiumCount.GetInt();
            MaxEmperiumCount = BasisForestMazeInfo.MaxEmperiumCount.GetInt();
        }

        public void Dispose()
        {
            if (!isInitialize)
                return;

            isInitialize = false;
            RemoveEvent();

            NeedBossBattleEmperiumCount = 0;
            MaxEmperiumCount = 0;

            BattleResult = ForestMazeBattleResult.None;
            PlayerMaxHp = 0;
            SetPlayerHp(0);
            selectedRewardIds.Clear();
            TotalRewards = null;

            UpdateState(ForestMazeState.None);

            botEntityPool.Clear();
            mazeHpPotionPool.Clear();
            mazeEmperiumPool.Clear();
        }

        /// <summary>
        /// 미궁숲 진행 중
        /// </summary>
        public bool IsJoined()
        {
            return State != ForestMazeState.None;
        }

        /// <summary>
        /// 입장 처리
        /// </summary>
        public async Task<bool> Enter(int id)
        {
            if (IsJoined())
                return false;

            ForestBaseData[] arrForestData = forestBaseDataRepo.Get(id);
            if (arrForestData == null || arrForestData.Length == 0)
            {
                Debug.LogError($"해당 id가 존재하지 않습니다: {nameof(id)} = {id}");
                return false;
            }

            Response response = await dungeonModel.RequestForestMazeRoomJoin(id);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return false;
            }

            currentArrData = arrForestData;
            dungeonModel.SetLastEnterMultiMazeLobbyId(MultiMazeWaitingRoomData.MULTI_MAZE_LOBBY_FOREST_MAZE); // 마지막 입장한 Lobby도 변경해 줌

            SetFloor(response, 1); // 층 세팅
            SetEmperiumCount(0); // 엠펠리움 수 초기화
            UpdateState(ForestMazeState.Maze); // Maze 상태로 변경
            return true;
        }

        /// <summary>
        /// 퇴장 처리
        /// </summary>
        public async Task<bool> Exit()
        {
            if (IsJoined())
            {
                Response response = await Protocol.REQUEST_FOREST_ROOM_EXIT.SendAsync();
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
        /// 체력 찾기
        /// </summary>
        public MazeObjectEntity FindHpPotion(int index)
        {
            return mazeHpPotionPool.Find(index);
        }

        /// <summary>
        /// 엠펠리움 찾기
        /// </summary>
        public EmperiumEntity FindEmperium(int index)
        {
            return mazeEmperiumPool.Find(index);
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
        /// 포션 정보 반환
        /// </summary>
        public MazeObjectEntity[] GetHpPotions()
        {
            return mazeHpPotionPool.GetMazeObjects();
        }

        /// <summary>
        /// 엠펠리움 정보 반환
        /// </summary>
        public MazeObjectEntity[] GetEmperiums()
        {
            return mazeEmperiumPool.GetMazeObjects();
        }

        /// <summary>
        /// 선택한 보상 목록
        /// </summary>
        public int[] GetSelectedRewardIds()
        {
            return selectedRewardIds.ToArray();
        }

        /// <summary>
        /// 스킬 존재 여부
        /// </summary>
        public bool HasSkill(ForestMazeSkill input)
        {
            foreach (var item in selectedRewardIds)
            {
                ForestRewardData data = forestRewardDataRepo.Get(item);
                if (data == null)
                    continue;

                ForestMazeSkill skill = data.GetSkill();
                if (skill == null)
                    continue;

                if (skill == input)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 봇 재활용
        /// </summary>
        private void Recycle()
        {
            botEntityPool.Recycle();
            mazeHpPotionPool.Recycle();
            mazeEmperiumPool.Recycle();
        }

        /// <summary>
        /// 미로 오브젝트 회수
        /// </summary>
        private void Despawn()
        {
            mazeHpPotionPool.Despawn();
            mazeEmperiumPool.Despawn();
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
            Protocol.REQUEST_FOREST_ROOM_TRANSFORM.SendAsync(sfs).WrapNetworkErrors();
        }

        void OnUserJoin(Response response)
        {
            IMultiPlayerInput multiPlayer = response.GetPacket<MultiMazePlayerPacket>("1");
            UpdatePlayerJoin(multiPlayer);
        }

        void OnUserExit(Response response)
        {
            const byte USER_EXIT = 1; // 단순 나감 (only 멀티플레이어)
            const byte KILLED_BY_BOSS_CRASH = 2; // 보스충돌로 인해 죽음 (본인 포함)
            const byte KILLED_BY_LOW_HP = 3; // HP부족으로 인해 죽음 (본인 포함)
            const byte LOSE_BOSS_BATTLE = 4; // 보스전투패배로 인해 죽음 (본인이 포함되긴 하지만, MultiMazeEntry에서는 only 멀티플레이어)
            const byte WIN_BOSS_BATTLE = 5; // 보스전투승리로 인해 나감 (본인이 포함되긴 하지만, MultiMazeEntry에서는 only 멀티플레이어)
            const byte PLAYER_OUT_SYSTEM = 6; // 플레이어 죽음: 단순 시스템 띄우기

            int cid = response.GetInt("1");
            byte exitType = response.GetByte("2");
            int? remainHp = null;
            int? monsterIndex = null;

            if (response.ContainsKey("3"))
                remainHp = response.GetInt("3");

            if (response.ContainsKey("4"))
                monsterIndex = response.GetByte("4");

            ForestMazeBattleResult result = ForestMazeBattleResult.None;
            switch (exitType)
            {
                case LOSE_BOSS_BATTLE: // 보스전투패배로 인해 죽음
                    if (monsterIndex.HasValue)
                    {
                        const byte MONSTER_GENERAL_STATE = (byte)MonsterState.General;
                        UpdateMonsterStatus(monsterIndex.Value, MONSTER_GENERAL_STATE, remainHp, null, null, isStartState: false);
                    }
                    result = ForestMazeBattleResult.Fail;
                    break;

                case WIN_BOSS_BATTLE:
                    if (monsterIndex.HasValue)
                    {
                        const byte MONSTER_GENERAL_STATE = (byte)MonsterState.General;
                        UpdateMonsterStatus(monsterIndex.Value, MONSTER_GENERAL_STATE, remainHp, null, null, isStartState: true);
                    }
                    result = ForestMazeBattleResult.Succees;
                    break;
            }

            // 플레이어의 경우에 한 함
            if (IsPlayer(cid))
            {
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    TotalRewards = UI.ConvertRewardData(charUpdateData.rewards);
                }

                SetResult(result);
                return;
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

        void OnUserDie(Response response)
        {
            int cid = response.GetInt("1");
            UpdatePlayerDie(cid);
        }

        void OnGetEmperium(Response response)
        {
            int cid = response.GetInt("1");
            int index = response.GetByte("2");
            UpdateEmperiumGained(cid, index); // 엠펠리움 획득
        }

        void OnGetHpPotion(Response response)
        {
            int cid = response.GetInt("1");
            int index = response.GetByte("2");
            int remainHp = response.GetInt("3");
            UpdateHpPotionGained(cid, index); // 체력포션 획득
            UpdatePlayerHp(cid, remainHp);
        }

        void OnBattleMiddleBoss(Response response)
        {
            int cid = response.GetInt("1"); // 충돌한 유저
            int monsterIndex = response.GetByte("2"); // 충돌한 보스 인덱스
            UpdatePlayerMatchState(ForestMazeState.MiddleBossBattle, cid, monsterIndex); // 플레이어 상태 - 전투
        }

        void OnBattleBoss(Response response)
        {
            int cid = response.GetInt("1"); // 충돌한 유저
            int monsterIndex = response.GetByte("2"); // 충돌한 보스 인덱스
            UpdatePlayerMatchState(ForestMazeState.BossBattle, cid, monsterIndex); // 플레이어 상태 - 전투
        }

        void OnEndBattleMiddleBoss(Response response)
        {
            if (response.ContainsKey("1"))
            {
                int[] rewardIds = response.GetIntArray("1");
                OnSelectReward?.Invoke(rewardIds);
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

        void OnUpFloor(Response response)
        {
            SetFloor(response, Floor + 1);
        }

        void OnDownFloor(Response response)
        {
            SetFloor(response, Floor - 1);
        }

        void OnItemSelect(Response response)
        {
            int selectedId = response.GetInt("1");
            selectedRewardIds.Add(selectedId);
            UpdateState(ForestMazeState.Maze); // 전투 종료
        }

        void OnEmperiumRegen(Response response)
        {
            int index = response.GetByte("1");
            float posX = response.GetFloat("2");
            float posZ = response.GetFloat("3");
            UpdateEmperiumRegen(index, new Vector3(posX, 0f, posZ));
        }

        void OnItemRegen(Response response)
        {
            int index = response.GetByte("1");
            float posX = response.GetFloat("2");
            float posZ = response.GetFloat("3");
            UpdateHpRegen(index, new Vector3(posX, 0f, posZ));
        }

        void OnWinBattleMiddleBoss(Response response)
        {
            int cid = response.GetInt("1");
            UpdatePlayerGeneralState(cid);
        }

        /// <summary>
        /// 상태 변경
        /// </summary>
        private void UpdateState(ForestMazeState state)
        {
            if (State == state)
                return;

            State = state;

            switch (State)
            {
                case ForestMazeState.None:
                    Recycle(); // 전체 봇 Recycle
                    break;

                case ForestMazeState.Maze:
                    // 전투 관련 정보 초기화
                    BattlePlayerCid = -1; // 전투중인 플레이어
                    BattleMonsterIndex = -1; // 전투중인 몬스터
                    break;

                case ForestMazeState.MiddleBossBattle:
                case ForestMazeState.BossBattle:
                    Despawn(); // 미로 오브젝트 회수
                    break;
            }

            OnUpdateState?.Invoke();
        }

        /// <summary>
        /// 결과 세팅
        /// </summary>
        private void SetResult(ForestMazeBattleResult result)
        {
            BattleResult = result;
            UpdateState(ForestMazeState.None); // 이미 퇴장했기 때문에 서버에 Exit를 날리지 않아야 함
        }

        /// <summary>
        /// 플레이어 여부
        /// </summary>
        private bool IsPlayer(int cid)
        {
            return cid == characterModel.Cid;
        }

        /// <summary>
        /// 플레이어 입장
        /// </summary>
        private void UpdatePlayerJoin(IMultiPlayerInput input)
        {
            if (IsPlayer(input.Cid))
            {
                Debug.LogError("방어코드! 플레이어의 입장은 오지 않아야 한다.");
                return;
            }

            PlayerBotEntity find = botEntityPool.FindPlayer(input.Cid);
            if (find != null)
            {
                Debug.LogError("방어코드! 이미 존재하는 유저가 중복으로 들어옴");
                return;
            }

            PlayerBotEntity entity = botEntityPool.Create(input);
            OnMultiPlayerJoin?.Invoke(entity);
        }

        /// <summary>
        /// 플레이어 퇴장
        /// </summary>
        private void UpdatePlayerExit(int cid)
        {
            // 방어코드 (앞에서 처리 함)
            if (IsPlayer(cid))
            {
                UpdateState(ForestMazeState.None); // 이미 퇴장했기 때문에 서버에 Exit를 날리지 않아야 함
                return;
            }

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
        private void UpdatePlayerMatchState(ForestMazeState mazeState, int cid, int monsterIndex)
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
        /// 엠펠리움 획득
        /// </summary>
        private void UpdateEmperiumGained(int index, int mazeCubeIndex)
        {
            bool isPlayer = index == player.Character.Cid;
            EmperiumEntity find = mazeEmperiumPool.Find(mazeCubeIndex);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 엠펠리움 획득: {nameof(index)} = {index}");
#endif
                return;
            }

            if (isPlayer)
            {
                SetEmperiumCount(EmperiumCount + 1); // 엠펠리움 개수 증가
            }

            find.SetState(MazeCubeState.StandByRespawn);
            OnGainedEmperium?.Invoke(find, isPlayer);
        }

        /// <summary>
        /// 체력포션 획득
        /// </summary>
        private void UpdateHpPotionGained(int index, int mazeCubeIndex)
        {
            bool isPlayer = index == player.Character.Cid;
            HpPotionEntity find = mazeHpPotionPool.Find(mazeCubeIndex);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 체력포션 획득: {nameof(index)} = {index}");
#endif
                return;
            }

            find.SetState(MazeCubeState.StandByRespawn);
            OnGainedHpPotion?.Invoke(find, isPlayer);
        }

        /// <summary>
        /// 엠펠리움 리젠
        /// </summary>
        private void UpdateEmperiumRegen(int index, Vector3 pos)
        {
            EmperiumEntity find = mazeEmperiumPool.Find(index);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 체력포션 리젠: {nameof(index)} = {index}");
#endif
                return;
            }

            find.SetState(MazeCubeState.General);
            find.SetPosition(pos);
            OnRegenEmperium?.Invoke(find);
        }

        /// <summary>
        /// 체력포션 리젠
        /// </summary>
        private void UpdateHpRegen(int index, Vector3 pos)
        {
            HpPotionEntity find = mazeHpPotionPool.Find(index);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 체력포션 리젠: {nameof(index)} = {index}");
#endif
                return;
            }

            find.SetState(MazeCubeState.General);
            find.SetPosition(pos);
            OnRegenHpPotion?.Invoke(find);
        }

        /// <summary>
        /// 획득한 엠펠리움 조각 세팅
        /// </summary>
        private void SetEmperiumCount(int count)
        {
            EmperiumCount = Mathf.Clamp(count, 0, MaxEmperiumCount);
            OnUpdateEmperium?.Invoke();
        }

        /// <summary>
        /// 층 세팅
        /// </summary>
        private void SetFloor(Response response, int floor)
        {
            Recycle(); // 기존 데이터 초기화

            MultiMazePlayerPacket[] characterPackets = response.ContainsKey("2") ? response.GetPacketArray<MultiMazePlayerPacket>("2") : System.Array.Empty<MultiMazePlayerPacket>();
            ForestMazeMonsterPacket[] monsterPackets = response.ContainsKey("3") ? response.GetPacketArray<ForestMazeMonsterPacket>("3") : System.Array.Empty<ForestMazeMonsterPacket>();
            int posX = response.GetInt("4");
            int posZ = response.GetInt("5");
            int mapId = response.GetInt("6");
            MazeCubePacket[] arrMazeItemPacket = response.GetPacketArray<MazeCubePacket>("7"); // 아이템 정보
            MazeCubePacket[] arrMazeEmperiumPacket = response.GetPacketArray<MazeCubePacket>("8"); // 엠펠리움 정보
            long currentTime = response.GetLong("9"); // 현재시간
            long endTime = response.GetLong("10");
            int playerHp = response.GetInt("11");

            // 추가 데이터 세팅
            BossMonsterIndex = -1;
            foreach (var item in monsterPackets)
            {
                ForestMonData monsterData = forestMonDataRepo.Get(item.forestMonsterDataId);
                if (monsterData == null)
                {
                    Debug.LogError($"존재하지 않는 미궁숲몬스터 아이디 {nameof(item.forestMonsterDataId)} = {item.forestMonsterDataId}");
                    continue;
                }

                item.SetSpawnInfo(monsterData);
                item.SetMoveSpeed(MathUtils.ToPermyriadValue(monsterData.monster_speed));

                if (monsterData.IsBoss())
                {
                    BossMonsterIndex = item.index;
                }
            }

            botEntityPool.Create(characterPackets);
            botEntityPool.Create(monsterPackets);
            PlayerPosition = new Vector3(posX * 0.001f, 0f, posZ * 0.001f);

            mazeHpPotionPool.Create(arrMazeItemPacket);
            mazeEmperiumPool.Create(arrMazeEmperiumPacket);
            ServerTime.Initialize(currentTime); // 서버 시간 세팅
            EndTime = endTime.ToDateTime(); // 종료 시간 세팅

            // 플레이어의 MaxHp 세팅
            if (PlayerMaxHp == 0)
                PlayerMaxHp = playerHp;

            SetPlayerHp(playerHp);

            // Set FLoor
            Floor = Mathf.Clamp(floor, 1, currentArrData.Length); // 층 세팅

            // Set Data
            int index = Floor - 1;
            CurrentData = currentArrData[index]; // 현제 데이터 세팅

            OnUpdateFloor?.Invoke();
        }

        /// <summary>
        /// 플레이어 Hp 세팅
        /// </summary>
        public void SetPlayerHp(int hp)
        {
            PlayerHp = hp;
        }
    }
}