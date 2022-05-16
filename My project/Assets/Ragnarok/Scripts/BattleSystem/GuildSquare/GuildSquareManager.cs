#define TEST

using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class GuildSquareManager : Singleton<GuildSquareManager>
    {
        public delegate void PlayerHealEvent(int plusHp, int remainHp);
        public delegate void PlayerDamageEvent(int damage, int remainHp);
        public delegate void PlayerDieEvent(int damage);
        public delegate void PlayerAppearEvent();
        public delegate void PlayerChainAttackEvent(MonsterBotEntity target, SkillInfo skill);

        public delegate void MultiPlayerHealEvent(PlayerBotEntity entity, int plusHp, int remainHp);
        public delegate void MultiPlayerAttackMotionEvent(PlayerBotEntity entity, MonsterBotEntity target, SkillInfo skill);
        public delegate void MultiPlayerDamageEvent(PlayerBotEntity entity, int damage, int remainHp);
        public delegate void MultiPlayerDieEvent(PlayerBotEntity entity, int damage);
        public delegate void MultiPlayerAppearEvent(PlayerBotEntity entity);
        public delegate void MultiPlayerBuffEvent(int[] targetCids, SkillInfo skill);

        public delegate void MultiMonsterEvent(MonsterBotEntity entity);
        public delegate void MultiMonsterExit(MonsterBotEntity entity);
        public delegate void MultiMonsterDamageEvent(MonsterBotEntity entity, UnitEntity attacker, int dmgPerHit, int blowCount, bool isCriticalHit, bool isBasicActiveSkill, ElementType elementType, int damage, bool isKnockBack, int remainHp);
        public delegate void MultiMonsterDieEvent(MonsterBotEntity entity, UnitEntity attacker, int dmgPerHit, int blowCount, bool isCriticalHit, bool isBasicActiveSkill, ElementType elementType, int damage);
        public delegate void MultiMonsterDotDamageEvent(MonsterBotEntity entity, int damage, int remainHp);
        public delegate void MultiMonsterCrowdControlEvent(MonsterBotEntity entity, CrowdControlType type);
        public delegate void MultiMonsterAppearEvent(MonsterBotEntity entity);

        /// <summary>
        /// 엠펠리움 위치
        /// </summary>
        public static readonly Vector3 EmperiumPosition = new Vector3(33.0f, 0f, 43.0f);

        private const float CELL_SIZE_X = 3.24f;
        private const float CELL_SIZE_Z = 3.24f;

        private readonly CharacterEntity player;

        private readonly CharacterModel characterModel;

        private readonly SkillDataManager skillDataRepo;

        private readonly BotEntityPoolManager botEntityPool;
        private readonly BattleTrapPool battleTrapPool;

        public GuildSquareState State { get; private set; }
        public GuildBattleResult BattleResult { get; private set; }

        public Vector3 PlayerPosition { get; private set; }
        public byte PlayerState { get; private set; }

        // <!-- 길드 습격 정보 --!>
        public int PlayerMaxHp { get; private set; }
        public int EmperiumHp { get; private set; }
        public int EmperiumMaxHp { get; private set; }
        public System.DateTime GuildAttackStartTime { get; private set; }
        public System.DateTime GuildAttackEndTime { get; private set; }
        public System.DateTime RebirthTime { get; private set; } // 부활시간
        public int RefineEmperium { get; private set; } // 정제된 엠펠리움 수량
        public int PreEmperiumLevel { get; private set; } // 엠펠리움 이전레벨
        public int EmperiumLevel { get; private set; } // 엠펠리움 레벨

        public event System.Action OnPlayerGuildOut; // 내 캐릭터 길드 추방에 의한 퇴장
        public event System.Action OnPlayerMove; // 플레이어 움직임
        public event PlayerHealEvent OnPlayerHeal; // 플레이어 회복
        public event PlayerDamageEvent OnPlayerDamage; // 플레이어 대미지
        public event PlayerDieEvent OnPlayerDie; // 플레이어 죽음
        public event PlayerAppearEvent OnPlayerAppear; // 플레이어 등장
        public event PlayerChainAttackEvent OnPlayerChainAttack; // 플레이어 연계스킬

        public event System.Action<PlayerBotEntity> OnMultiPlayerJoin; // 멀티플레이어 입장
        public event System.Action<PlayerBotEntity> OnMultiPlayerExit; // 멀티플레이어 퇴장
        public event System.Action<PlayerBotEntity> OnMultiPlayerMove; // 멀티플레이어 움직임
        public event MultiPlayerHealEvent OnMultiPlayerHeal; // 멀티플레이어 회복
        public event MultiPlayerAttackMotionEvent OnMultiPlayerAttackMotion; // 멀티플레이어 공격모션
        public event MultiPlayerDamageEvent OnMultiPlayerDamage; // 멀티플레이어 대미지
        public event MultiPlayerDieEvent OnMultiPlayerDie; // 멀티플레이어 죽음
        public event MultiPlayerAppearEvent OnMultiPlayerAppear; // 멀티플레이어 등장
        public event MultiPlayerBuffEvent OnMultiPlayerBuff; // 멀티플레이어 버프

        public event MultiMonsterEvent OnMultiMonsterStatus; // 몬스터 상태변화
        public event MultiMonsterExit OnMultiMonsterExit; // 몬스터 회수
        public event MultiMonsterDamageEvent OnMultiMonsterDamage; // 몬스터 대미지
        public event MultiMonsterDieEvent OnMultiMonsterDie; // 몬스터 죽음
        public event MultiMonsterDotDamageEvent OnMultiMonsterDotDamage; // 몬스터 도트대미지
        public event MultiMonsterCrowdControlEvent OnMultiMonsterCrowdControl; // 몬스터 상태이상

        public event System.Action OnUpdateSquareState;
        public event System.Action<int> OnUpdateEmperiumDamage;

        public event System.Action OnUpdateRefineEmperium; // 엠펠리움 결정 수량 변경
        public event System.Action OnUpdateGuildAttackStartTime; // 길드습격 시간 변경
        public event System.Action OnUpdateCreateEmperium; // 길드습격 엠펠리움 생성
        public event System.Action OnUpdateEmperiumLevel; // 엠펠리움 레벨 업데이트

        public GuildSquareManager()
        {
            player = Entity.player;

            characterModel = player.Character;

            skillDataRepo = SkillDataManager.Instance;

            botEntityPool = BotEntityPoolManager.Instance;
            battleTrapPool = new BattleTrapPool(BattleTrapType.GuildAttack);

            AddEvent();
        }

        ~GuildSquareManager()
        {
            RemoveEvent();
        }

        protected override void OnTitle()
        {
            ResetData();

            botEntityPool.Clear();
            battleTrapPool.Clear();
            RefineEmperium = 0;
            PreEmperiumLevel = 0;
            EmperiumLevel = 0;
        }

        private void AddEvent()
        {
            Protocol.RESPONSE_GUILDUSER_ENTERROOM.AddEvent(OnUserJoin);
            Protocol.RESPONSE_GUILDUSER_EXITROOM.AddEvent(OnUserExit);
            Protocol.REQUEST_GUILDUSER_TRANSFORM.AddEvent(OnUserTransform);

            Protocol.RECEIVE_GA_START.AddEvent(OnStartGuildAttack);
            Protocol.RECEIVE_GA_END.AddEvent(OnEndGuildAttack);
            Protocol.REQUEST_GA_ATTACK_MOT.AddEvent(OnUserAttackMotion);
            Protocol.RECEIVE_GA_MONSTATUS.AddEvent(OnMonsterStatus);
            Protocol.RECEIVE_GA_PLAYER_PLUSHP.AddEvent(OnUserPlusHp);
            Protocol.RECEIVE_GA_MON_GETCROWDCONTROL.AddEvent(OnMonsterCrowdControl);
            Protocol.RECEIVE_GA_MON_DAMAGE.AddEvent(OnMonsterDamage);
            Protocol.RECEIVE_GA_MON_DIE.AddEvent(OnMonsterDie);
            Protocol.RECEIVE_GA_EMPAL_DAMAGE.AddEvent(OnEmperiumDamage);
            Protocol.RECEIVE_GA_EMPAL_DIE.AddEvent(OnEmperiumDie);
            Protocol.RECEIVE_GA_PLAYER_DAMAGE.AddEvent(OnUserDamage);
            Protocol.RECEIVE_GA_PLAYER_DIE.AddEvent(OnUserDie);
            Protocol.RECEIVE_GA_PLAYER_APPEAR.AddEvent(OnUserRebirth); // REQUEST_GA_USER_REVIVE 의 응답
            Protocol.RECEIVE_GA_ROCK_APPEAR_READY.AddEvent(OnTrapReady);
            Protocol.RECEIVE_GA_ROCK_APPEAR.AddEvent(OnTrapAppear);
            Protocol.RECEIVE_GA_ROCK_DISAPPEAR.AddEvent(OnTrapDisappear);
            Protocol.REQUEST_GA_ACTIVEBUFSKILL.AddEvent(OnUserBuff);
            Protocol.RECEIVE_GA_MONDOTDAMAGE.AddEvent(OnMonsterDotDamage);

            Protocol.RECEIVE_GA_EMPERIUM.AddEvent(OnRefineEmperieum);
            Protocol.RECEIVE_GA_EMPERIUM_AND_START_TIME.AddEvent(OnEmperiumAndGuildAttackTime);

            Protocol.RESPONSE_CHAR_UPDATE.AddEvent(OnResponseCharUpdate);
            Protocol.RECEIVE_GA_MAKEEMPAL.AddEvent(OnCreateEmperium);
        }

        private void RemoveEvent()
        {
            Protocol.RESPONSE_GUILDUSER_ENTERROOM.RemoveEvent(OnUserJoin);
            Protocol.RESPONSE_GUILDUSER_EXITROOM.RemoveEvent(OnUserExit);
            Protocol.REQUEST_GUILDUSER_TRANSFORM.RemoveEvent(OnUserTransform);

            Protocol.RECEIVE_GA_START.RemoveEvent(OnStartGuildAttack);
            Protocol.RECEIVE_GA_END.RemoveEvent(OnEndGuildAttack);
            Protocol.REQUEST_GA_ATTACK_MOT.RemoveEvent(OnUserAttackMotion);
            Protocol.RECEIVE_GA_MONSTATUS.RemoveEvent(OnMonsterStatus);
            Protocol.RECEIVE_GA_PLAYER_PLUSHP.RemoveEvent(OnUserPlusHp);
            Protocol.RECEIVE_GA_MON_GETCROWDCONTROL.RemoveEvent(OnMonsterCrowdControl);
            Protocol.RECEIVE_GA_MON_DAMAGE.RemoveEvent(OnMonsterDamage);
            Protocol.RECEIVE_GA_MON_DIE.RemoveEvent(OnMonsterDie);
            Protocol.RECEIVE_GA_EMPAL_DAMAGE.RemoveEvent(OnEmperiumDamage);
            Protocol.RECEIVE_GA_EMPAL_DIE.RemoveEvent(OnEmperiumDie);
            Protocol.RECEIVE_GA_PLAYER_DAMAGE.RemoveEvent(OnUserDamage);
            Protocol.RECEIVE_GA_PLAYER_DIE.RemoveEvent(OnUserDie);
            Protocol.RECEIVE_GA_PLAYER_APPEAR.RemoveEvent(OnUserRebirth); // REQUEST_GA_USER_REVIVE 의 응답
            Protocol.RECEIVE_GA_ROCK_APPEAR_READY.RemoveEvent(OnTrapReady);
            Protocol.RECEIVE_GA_ROCK_APPEAR.RemoveEvent(OnTrapAppear);
            Protocol.RECEIVE_GA_ROCK_DISAPPEAR.RemoveEvent(OnTrapDisappear);
            Protocol.REQUEST_GA_ACTIVEBUFSKILL.RemoveEvent(OnUserBuff);
            Protocol.RECEIVE_GA_MONDOTDAMAGE.RemoveEvent(OnMonsterDotDamage);

            Protocol.RECEIVE_GA_EMPERIUM.RemoveEvent(OnRefineEmperieum);
            Protocol.RECEIVE_GA_EMPERIUM_AND_START_TIME.RemoveEvent(OnEmperiumAndGuildAttackTime);

            Protocol.RESPONSE_CHAR_UPDATE.RemoveEvent(OnResponseCharUpdate);
            Protocol.RECEIVE_GA_MAKEEMPAL.RemoveEvent(OnCreateEmperium);
        }

        public void ResetData()
        {
            BattleResult = GuildBattleResult.None;
            EmperiumHp = 0;
            EmperiumMaxHp = 0;

            UpdateSquareState(GuildSquareState.None);
        }

        /// <summary>
        /// 스퀘어 진행 중
        /// </summary>
        public bool IsJoined()
        {
            return State != GuildSquareState.None;
        }

        /// <summary>
        /// 입장 처리
        /// </summary>
        public async Task<bool> Enter()
        {
            // 중복 입장
            if (IsJoined())
                return false;

            Response response = await Protocol.REQUEST_GUILDUSER_ENTERROOM.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return false;
            }

            Response multiLobbyPlayers = new Response(response.GetSFSObject("2"));
            IMultiPlayerInput[] characterPackets = multiLobbyPlayers.ContainsKey("in") ? multiLobbyPlayers.GetPacketArray<GuildLobbyCharacterPacket>("in") : System.Array.Empty<GuildLobbyCharacterPacket>();
            float playerPosX = response.GetFloat("3");
            float playerPosZ = response.GetFloat("4");
            long guildAttackStartTime = response.GetLong("5"); // 길드습격 시작 시간
            long serverTime = response.GetLong("6"); // 서버 현재 시간
            int refineEmperimum = response.GetInt("7"); // 정제된 엠펠리움 수량
            short emperiumLevel = response.GetShort("8"); // 엠펠리움 레벨

            ServerTime.Initialize(serverTime); // 서버 시간 세팅

            PlayerPosition = new Vector3(playerPosX, 0f, playerPosZ);
            botEntityPool.Create(characterPackets);

            SetGuildAttackTime(guildAttackStartTime);
            SetRefineEmperium(refineEmperimum);
            SetEmperiumLevel(emperiumLevel);

            // 길드 습격 정보
            if (response.ContainsKey("e"))
            {
                int emperiumHp = response.GetInt("e");
                int emperiumMaxHp = response.GetInt("e2");
                GuildAttackMonsterPacket[] monsterPackets = response.ContainsKey("m") ? response.GetPacketArray<GuildAttackMonsterPacket>("m") : null;
                FreeFightTrapPacket[] trpaPackets = response.ContainsKey("r") ? response.GetPacketArray<FreeFightTrapPacket>("r") : System.Array.Empty<FreeFightTrapPacket>();
                long guildAttackEndTime = response.GetLong("et");
                int maxHp = response.GetInt("9");

                PlayerState = response.GetByte("s"); // 플레이어 상태
                if (response.ContainsKey("rst"))
                {
                    RebirthTime = response.GetLong("rst").ToDateTime(); // 플레이어 부활시간 (s가 1일때만 보냄)
                }

                SetGuildAttack(emperiumHp, emperiumMaxHp, monsterPackets, trpaPackets, guildAttackEndTime, maxHp);
            }
            else
            {
                UpdateSquareState(GuildSquareState.Square); // 길드 스퀘어 상태
            }

            return true;
        }

        /// <summary>
        /// 퇴장 처리
        /// </summary>
        public async Task<bool> Exit()
        {
            if (IsJoined())
            {
                Response response = await Protocol.REQUEST_GUILDUSER_EXITROOM.SendAsync();
                if (!response.isSuccess)
                {
                    response.ShowResultCode();
                    return false;
                }

                ResetData(); // 데이터 초기화
            }

            return true;
        }

        public CharacterEntity FindPlayer(int cid)
        {
            return IsPlayer(cid) ? player : botEntityPool.FindPlayer(cid);
        }

        public MonsterBotEntity FindMonster(int index)
        {
            return botEntityPool.FindMonster(index);
        }

        public BattleTrap FindTrap(int id)
        {
            return battleTrapPool.Find(id);
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
        /// 트랩 생성 대기열 정보 반환
        /// </summary>
        public BattleTrap DequeueTrapTuple()
        {
            if (battleTrapPool.HasQueue())
            {
                IBattleTrapInput input = battleTrapPool.Dequeue();
                BattleTrap trap = battleTrapPool.Create(input);
                return trap;
            }

            return null;
        }

        /// <summary>
        /// 봇 재활용
        /// </summary>
        private void Recycle()
        {
            botEntityPool.Recycle();
            battleTrapPool.Recycle();
        }

        /// <summary>
        /// 몬스터 봇 재활용
        /// </summary>
        private void MonsterRecycle()
        {
            botEntityPool.MonsterRecycle();
        }

        /// <summary>
        /// 포인트에 따른 위치 좌표 반환
        /// </summary>
        public Vector3 GetCellPosition(short indexX, short indexZ)
        {
            return new Vector3(indexX * CELL_SIZE_X, 0f, indexZ * CELL_SIZE_Z);
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
            Protocol.REQUEST_GUILDUSER_TRANSFORM.SendAsync(sfs).WrapNetworkErrors();
        }

        void OnUserJoin(Response response)
        {
            Response multiLobbyPlayers = new Response(response.GetSFSObject("1"));
            IMultiPlayerInput[] multiPlayers = multiLobbyPlayers.GetPacketArray<GuildLobbyCharacterPacket>("in");
            foreach (var item in multiPlayers)
            {
                UpdatePlayerJoin(item); // 플레이어 입장
            }
        }

        void OnUserExit(Response response)
        {
            int cid = response.GetInt("1");
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

        void OnStartGuildAttack(Response response)
        {
            int emperiumHp = response.GetInt("e");
            int emperiumMaxHp = response.GetInt("e2");
            GuildAttackMonsterPacket[] monsterPackets = response.ContainsKey("m") ? response.GetPacketArray<GuildAttackMonsterPacket>("m") : null;
            FreeFightTrapPacket[] trpaPackets = response.ContainsKey("r") ? response.GetPacketArray<FreeFightTrapPacket>("r") : null;
            long guildAttackEndTime = response.GetLong("et");
            int maxHp = response.GetInt("9");

            SetGuildAttack(emperiumHp, emperiumMaxHp, monsterPackets, trpaPackets, guildAttackEndTime, maxHp);
        }

        void OnEndGuildAttack(Response response)
        {
            const int SUCCESS = 1;
            int resultFlag = response.GetInt("1");
            int emperiumLevel = response.GetInt("2");
            long guildAttackStartTime = response.GetLong("3");

            SetEmperiumLevel(emperiumLevel);
            SetGuildAttackTime(guildAttackStartTime);
            SetResult(resultFlag == SUCCESS ? GuildBattleResult.Succees : GuildBattleResult.Fail);
        }

        void OnUserAttackMotion(Response response)
        {
            int cid = response.GetInt("1");
            int targetMonsterIndex = response.GetInt("2");
            int skillId = response.GetInt("3");
            UpdatePlayerSkillMotion(cid, skillId, targetMonsterIndex);
        }

        void OnMonsterStatus(Response response)
        {
            int monsterIndex = response.GetByte("1");
            byte monsterState = response.GetByte("2");
            float prePosX = response.GetFloat("3");
            float prePosZ = response.GetFloat("4");
            float targetPosX = response.GetFloat("5");
            float targetPosZ = response.GetFloat("6");
            int remainHp = response.GetInt("7");

            bool isStartState = monsterState == (byte)GuildAttackEntry.MonsterState.Start;
            if (isStartState)
            {
                int id = response.GetInt("8");
                int scale = response.GetInt("9");
                int level = response.GetInt("10");

                float monsterScale = MathUtils.ToPercentValue(scale);
                UpdateMonsterAppear(monsterIndex, id, level, monsterScale); // 몬스터 상태변화 하기 전에 미리 세팅
            }

            Vector3 prePos = new Vector3(prePosX, 0f, prePosZ);
            Vector3 targetPos = new Vector3(targetPosX, 0f, targetPosZ);

            UpdateMonsterStatus(monsterIndex, monsterState, remainHp, prePos, targetPos, isStartState); // 몬스터 상태변화
        }

        void OnUserPlusHp(Response response)
        {
            int cid = response.GetInt("1");
            int plusHp = response.GetInt("2");
            int remainHp = response.GetInt("3");

            UpdatePlayerHeal(cid, plusHp, remainHp); // 플레이어 회복
        }

        void OnUserDamage(Response response)
        {
            int cid = response.GetInt("1");
            int damage = response.GetInt("2"); // 0: 미스
            int remainHp = response.GetInt("3");

            UpdatePlayerDamage(cid, damage, remainHp);
        }

        void OnUserDie(Response response)
        {
            int cid = response.GetInt("1");
            int damage = response.GetInt("2");

            UpdatePlayerDie(cid, damage);
        }

        void OnUserRebirth(Response response)
        {
            int cid = response.GetInt("1");
            int[] arrayPosValue = response.GetIntArray("2");
            int maxHp = response.GetInt("3");

            // 방어코드
            Vector3 position = arrayPosValue == null ? Vector3.zero : new Vector3(arrayPosValue[0] * 0.001f, 0f, arrayPosValue[1] * 0.001f);
            UpdatePlayerRebirth(cid, position, maxHp);
        }

        void OnUserBuff(Response response)
        {
            int cid = response.GetInt("1");
            int skillId = response.GetInt("2");
            int[] targetCids = response.GetIntArray("3");
            long slotId = response.GetLong("4"); // 사용하지 않음
            UpdatePlayerApplyBuff(cid, skillId, targetCids, slotId);
        }

        void OnTrapReady(Response response)
        {
            int trapId = response.GetShort("1");
            short indexX = response.GetShort("2");
            short indexZ = response.GetShort("3");
            UpdateTrapReady(trapId, indexX, indexZ);
        }

        void OnTrapAppear(Response response)
        {
            int trapId = response.GetShort("1");
            UpdateTrapAppear(trapId);
        }

        void OnTrapDisappear(Response response)
        {
            int trapId = response.GetShort("1");
            UpdateTrapDisappear(trapId);
        }

        void OnMonsterCrowdControl(Response response)
        {
            int monsterIndex = response.GetInt("1");
            byte crowdControlType = response.GetByte("2");

            if (response.ContainsKey("3"))
            {
                float posX = response.GetFloat("3");
                float posZ = response.GetFloat("4");
                Vector3 position = new Vector3(posX, 0f, posZ);
                UpdateMonsterFreeze(monsterIndex, position);
            }

            UpdateMonsterCrowdControl(monsterIndex, crowdControlType);
        }

        void OnMonsterDamage(Response response)
        {
            int monsterIndex = response.GetByte("1");
            int damage = response.GetInt("2"); // 0: 미스
            bool isCritical = response.GetByte("3") == 1; // 0:일반, 1:크리
            int remainHp = response.GetInt("4");
            int attackerCid = response.GetInt("5");
            int attackerSkillId = response.GetInt("6");

            UpdateMonsterDamage(monsterIndex, damage, isCritical, remainHp, attackerCid, attackerSkillId);
        }

        void OnMonsterDie(Response response)
        {
            int monsterIndex = response.GetByte("1");
            int damage = response.GetInt("2"); // 0: 미스
            bool isCritical = response.GetByte("3") == 1; // 0:일반, 1:크리
            int attackerCid = response.GetInt("4");
            int attackerSkillId = response.GetInt("5");

            UpdateMonsterDie(monsterIndex, damage, isCritical, attackerCid, attackerSkillId);
        }

        void OnMonsterDotDamage(Response response)
        {
            int monsterIndex = response.GetInt("1");
            int damage = response.GetInt("2"); // 0: 미스
            int remainHp = response.GetInt("3");

            UpdateMonsterDotDamage(monsterIndex, damage, remainHp);
        }

        void OnEmperiumDamage(Response response)
        {
            int damage = response.GetInt("1");
            int remainHp = response.GetInt("2");
            UpdateEmperiumDamage(damage, remainHp);
        }

        void OnEmperiumDie(Response response)
        {
            int damage = response.GetInt("1");
            UpdateEmperiumDamage(damage, 0);
        }

        /// <summary>
        /// 길드 습격 세팅
        /// </summary>
        private void SetGuildAttack(int emperiumHp, int emperiumMaxHp, GuildAttackMonsterPacket[] monsterPackets, FreeFightTrapPacket[] trpaPackets, long endTime, int maxHp)
        {
            PlayerMaxHp = maxHp;
            EmperiumHp = emperiumHp;
            EmperiumMaxHp = emperiumMaxHp;
            botEntityPool.Create(monsterPackets);
            battleTrapPool.EnqueueRange(trpaPackets);
            GuildAttackEndTime = endTime.ToDateTime();

            UpdateSquareState(GuildSquareState.GuildAttack); // 길드 전투 상태

            Quest.QuestProgress(QuestType.GUILD_ATTACK_ENTER_COUNT); // 길드 습격 도전 횟수
        }

        /// <summary>
        /// Square 상태 변경
        /// </summary>
        private void UpdateSquareState(GuildSquareState state)
        {
            if (State == state)
                return;

            State = state;

            switch (State)
            {
                case GuildSquareState.None:
                    Recycle(); // 전체 봇 Recycle
                    break;

                case GuildSquareState.Square:
                    MonsterRecycle(); // 몬스터 봇 Recycle
                    break;
            }

            OnUpdateSquareState?.Invoke();
        }

        /// <summary>
        /// 결과 세팅
        /// </summary>
        private void SetResult(GuildBattleResult result)
        {
            BattleResult = result;
            UpdateSquareState(GuildSquareState.Square); // 전투 종료
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
            if (IsPlayer(cid))
            {
                UpdateSquareState(GuildSquareState.None); // 이미 퇴장했기 때문에 서버에 Exit를 날리지 않아야 함

                string message = LocalizeKey._90252.ToText(); // 길드에서 탈퇴되었습니다. 필드로 이동합니다.
                UI.ConfirmPopup(message, OnConfirmGuildOut);
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
        /// 길드 탈퇴 메시지 확인
        /// </summary>
        private void OnConfirmGuildOut()
        {
            OnPlayerGuildOut?.Invoke();
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
        /// 플레이어 회복
        /// </summary>
        private void UpdatePlayerHeal(int cid, int plusHp, int remainHp)
        {
            if (IsPlayer(cid))
            {
                OnPlayerHeal?.Invoke(plusHp, remainHp);
                return;
            }

            PlayerBotEntity find = botEntityPool.FindPlayer(cid);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 플레이어가 회복: {nameof(cid)} = {cid}");
#endif
                return;
            }

            find.SetBotCurHp(remainHp);
            OnMultiPlayerHeal?.Invoke(find, plusHp, remainHp);
        }

        /// <summary>
        /// 플레이어 스킬 모션
        /// </summary>
        private void UpdatePlayerSkillMotion(int cid, int skillId, int monsterIndex)
        {
            if (IsPlayer(cid))
            {
                Debug.LogError("방어코드! 본인의 스킬 모션 진행"); // 플레이어의 모션은 오지 않지만, 혹시 모르니 막는다.
                return;
            }

            PlayerBotEntity find = botEntityPool.FindPlayer(cid);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 플레이어가 스킬모션 진행: {nameof(cid)} = {cid}");
#endif
                return;
            }

            MonsterBotEntity target = FindMonster(monsterIndex);
            if (target == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 몬스터에게 공격함: {nameof(monsterIndex)} = {monsterIndex}");
#endif
                return;
            }

            SkillInfo skill = FindSkill(find, skillId);
            if (skill == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 스킬: {nameof(skillId)} = {skillId}");
#endif
                return;
            }

            OnMultiPlayerAttackMotion?.Invoke(find, target, skill);
        }

        /// <summary>
        /// 플레이어 대미지
        /// </summary>
        private void UpdatePlayerDamage(int cid, int damage, int remainHp)
        {
            if (IsPlayer(cid))
            {
                OnPlayerDamage?.Invoke(damage, remainHp);
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
            OnMultiPlayerDamage?.Invoke(find, damage, remainHp);
        }

        /// <summary>
        /// 플레이어 죽음
        /// </summary>
        private void UpdatePlayerDie(int cid, int damage)
        {
            if (IsPlayer(cid))
            {
                OnPlayerDie?.Invoke(damage);
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

            byte state;
            if (State == GuildSquareState.GuildAttack)
            {
                state = (byte)GuildAttackEntry.PlayerState.Die;
            }
            else
            {
                state = (byte)PlayerBotState.Dead;
            }

            find.SetBotState(state);
            find.SetBotCurHp(0);
            OnMultiPlayerDie?.Invoke(find, damage);
        }

        /// <summary>
        /// 플레이어 부활
        /// </summary>
        private void UpdatePlayerRebirth(int cid, Vector3 position, int maxHp)
        {
            byte state;
            if (State == GuildSquareState.GuildAttack)
            {
                state = (byte)GuildAttackEntry.PlayerState.Idle;
            }
            else
            {
                state = (byte)PlayerBotState.General;
            }

            if (IsPlayer(cid))
            {
                PlayerState = state;
                PlayerPosition = position;
                PlayerMaxHp = maxHp;
                OnPlayerAppear?.Invoke();
                return;
            }

            PlayerBotEntity find = botEntityPool.FindPlayer(cid);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 플레이어가 부활: {nameof(cid)} = {cid}");
#endif
                return;
            }

            find.SetBotState(state);
            find.SetBotPosition(position);
            find.SetBotMaxHp(maxHp);
            find.SetBotCurHp(maxHp);

            OnMultiPlayerAppear?.Invoke(find);
        }

        /// <summary>
        /// 플레이어 버프 적용
        /// </summary>
        private void UpdatePlayerApplyBuff(int cid, int skillId, int[] targetCids, long slotId)
        {
            if (targetCids == null)
                return;

            UnitEntity find = FindPlayer(cid);
            SkillInfo skill = FindSkill(find, skillId);
            if (skill == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 스킬 사용: {nameof(cid)} = {cid}, {nameof(skillId)} = {skillId}");
#endif
                return;
            }

            OnMultiPlayerBuff?.Invoke(targetCids, skill);
        }

        /// <summary>
        /// 트랩 준비
        /// </summary>
        private void UpdateTrapReady(int trapId, short indexX, short indexZ)
        {
            BattleTrap find = FindTrap(trapId);
            if (find == null)
            {
                const byte READY_STATE = (byte)GuildAttackEntry.TrapState.Ready;

                // 생성 도중
                if (battleTrapPool.HasQueue() && battleTrapPool.UpdateQueueState(trapId, READY_STATE) && battleTrapPool.UpdateQueueIndex(trapId, indexX, indexZ))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 트랩 등장: {nameof(trapId)} = {trapId}");
#endif
                return;
            }

            Vector3 pos = GetCellPosition(indexX, indexZ);
            find.SetPosition(pos);
            find.ShowReady(); // 트랩 등장
        }

        /// <summary>
        /// 트랩 나타남
        /// </summary>
        private void UpdateTrapAppear(int trapId)
        {
            BattleTrap find = FindTrap(trapId);
            if (find == null)
            {
                const byte APPEAR_STATE = (byte)GuildAttackEntry.TrapState.Appear;

                // 생성 도중이라면 생성큐 업데이트
                if (battleTrapPool.HasQueue() && battleTrapPool.UpdateQueueState(trapId, APPEAR_STATE))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 트랩 출현: {nameof(trapId)} = {trapId}");
#endif
                return;
            }

            find.Appear(); // 트랩 출현
        }

        /// <summary>
        /// 트랩 사라짐
        /// </summary>
        private void UpdateTrapDisappear(int trapId)
        {
            BattleTrap find = FindTrap(trapId);
            if (find == null)
            {
                const byte NONE_STATE = (byte)GuildAttackEntry.TrapState.None;

                // 생성 도중이라면 생성큐 업데이트
                if (battleTrapPool.HasQueue() && battleTrapPool.UpdateQueueState(trapId, NONE_STATE))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 트랩 사라짐: {nameof(trapId)} = {trapId}");
#endif
                return;
            }

            find.Disappear(); // 트랩 사라짐
        }

        /// <summary>
        /// 엠펠리움 대미지
        /// </summary>
        private void UpdateEmperiumDamage(int damage, int hp)
        {
            if (EmperiumHp == hp)
                return;

            EmperiumHp = hp;
            OnUpdateEmperiumDamage?.Invoke(damage);
        }

        /// <summary>
        /// 몬스터 등장
        /// </summary>
        private void UpdateMonsterAppear(int monsterIndex, int id, int level, float monsterScale)
        {
            MonsterBotEntity find = FindMonster(monsterIndex);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 몬스터가 등장: {nameof(monsterIndex)} = {monsterIndex}");
#endif
                return;
            }

            find.SetBotMonster(id, level, monsterScale);
            OnMultiMonsterExit?.Invoke(find); // Dispose 처리해주기 위함
        }

        /// <summary>
        /// 몬스터 움직임
        /// </summary>
        private void UpdateMonsterStatus(int monsterIndex, byte monsterState, int remainHp, Vector3 prePos, Vector3 targetPos, bool isStartState)
        {
            MonsterBotEntity find = FindMonster(monsterIndex);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 몬스터가 움직임: {nameof(monsterIndex)} = {monsterIndex}");
#endif
                return;
            }

            find.SetBotState(monsterState);

            if (isStartState)
            {
                find.SetBotMaxHp(remainHp);
            }

            find.SetBotCurHp(remainHp);
            find.SetBotPosition(prePos, targetPos);
            OnMultiMonsterStatus?.Invoke(find);
        }

        /// <summary>
        /// 몬스터 상태이상 변경
        /// </summary>
        private void UpdateMonsterFreeze(int monsterIndex, Vector3 position)
        {
            MonsterBotEntity find = FindMonster(monsterIndex);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 몬스터가 상태변경: {nameof(monsterIndex)} = {monsterIndex}");
#endif
                return;
            }

            byte state;
            if (State == GuildSquareState.GuildAttack)
            {
                state = (byte)GuildAttackEntry.MonsterState.Freeze;
            }
            else
            {
                state = (byte)MonsterBotState.Freeze;
            }

            find.SetBotState(state);
            find.SetBotPosition(position);
            OnMultiMonsterStatus?.Invoke(find);
        }

        /// <summary>
        /// 몬스터 상태이상 변경
        /// </summary>
        private void UpdateMonsterCrowdControl(int monsterIndex, byte type)
        {
            MonsterBotEntity find = FindMonster(monsterIndex);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 몬스터가 상태변경: {nameof(monsterIndex)} = {monsterIndex}");
#endif
                return;
            }

            CrowdControlType crowdControlType = type.ToEnum<CrowdControlType>();
            OnMultiMonsterCrowdControl?.Invoke(find, crowdControlType);
        }

        /// <summary>
        /// 몬스터 대미지
        /// </summary>
        private void UpdateMonsterDamage(int monsterIndex, int damage, bool isCritical, int remainHp, int attackerCid, int attackerSkillId)
        {
            MonsterBotEntity find = FindMonster(monsterIndex);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 몬스터가 대미지: {nameof(monsterIndex)} = {monsterIndex}");
#endif
                return;
            }

            UnitEntity attacker = FindPlayer(attackerCid);
            if (attacker == null)
            {
                // 생성 도중에는 공격자가 존재하지 않을 수도 있음
            }

            SkillInfo skill = FindSkill(attacker, attackerSkillId);

            int dmgPerHit;
            int blowCount;
            bool isCriticalHit;
            bool isBasicActiveSkill;
            ElementType elementType;
            bool isKnockBack;
            if (skill == null)
            {
                dmgPerHit = damage;
                blowCount = 1;
                isCriticalHit = isCritical;
                isBasicActiveSkill = false;
                elementType = ElementType.Neutral;
                isKnockBack = false;
            }
            else
            {
                dmgPerHit = damage == 0 ? 0 : Mathf.Max(1, damage / skill.BlowCount);
                blowCount = skill.BlowCount;
                isCriticalHit = isCritical;
                isBasicActiveSkill = skill.IsBasicActiveSkill;
                elementType = skill.ElementType;
                isKnockBack = skill.IsRush;
            }

            find.SetBotCurHp(remainHp);
            OnMultiMonsterDamage?.Invoke(find, attacker, dmgPerHit, blowCount, isCriticalHit, isBasicActiveSkill, elementType, damage, isKnockBack, remainHp);

            // 공격자가 플레이어일 경우에 연계 공격 처리
            if (IsPlayer(attackerCid))
            {
                OnPlayerChainAttack?.Invoke(find, skill);
            }
        }

        /// <summary>
        /// 몬스터 죽음
        /// </summary>
        private void UpdateMonsterDie(int monsterIndex, int damage, bool isCritical, int attackerCid, int attackerSkillId)
        {
            MonsterBotEntity find = FindMonster(monsterIndex);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 몬스터가 죽음: {nameof(monsterIndex)} = {monsterIndex}");
#endif
                return;
            }

            UnitEntity attacker = FindPlayer(attackerCid);
            if (attacker == null)
            {
                // 생성 도중에는 공격자가 존재하지 않을 수도 있음
            }

            SkillInfo skillInfo = FindSkill(attacker, attackerSkillId);

            int dmgPerHit;
            int blowCount;
            bool isCriticalHit;
            bool isBasicActiveSkill;
            ElementType elementType;
            if (skillInfo == null)
            {
                dmgPerHit = damage;
                blowCount = 1;
                isCriticalHit = isCritical;
                isBasicActiveSkill = false;
                elementType = ElementType.Neutral;
            }
            else
            {
                dmgPerHit = damage == 0 ? 0 : Mathf.Max(1, damage / skillInfo.BlowCount);
                blowCount = skillInfo.BlowCount;
                isCriticalHit = isCritical;
                isBasicActiveSkill = skillInfo.IsBasicActiveSkill;
                elementType = skillInfo.ElementType;
            }

            byte state;
            if (State == GuildSquareState.GuildAttack)
            {
                state = (byte)GuildAttackEntry.MonsterState.Die;
            }
            else
            {
                state = (byte)MonsterBotState.Die;
            }

            find.SetBotState(state);
            find.SetBotCurHp(0);
            OnMultiMonsterDie?.Invoke(find, attacker, dmgPerHit, blowCount, isCriticalHit, isBasicActiveSkill, elementType, damage);
        }

        /// <summary>
        /// 몬스터 도트대미지
        /// </summary>
        private void UpdateMonsterDotDamage(int monsterIndex, int damage, int remainHp)
        {
            MonsterBotEntity find = FindMonster(monsterIndex);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 몬스터가 죽음: {nameof(monsterIndex)} = {monsterIndex}");
#endif
                return;
            }

            find.SetBotCurHp(remainHp);
            OnMultiMonsterDotDamage?.Invoke(find, damage, remainHp);
        }

        /// <summary>
        /// 스킬 반환
        /// </summary>
        private SkillInfo FindSkill(UnitEntity entity, int skillId)
        {
            SkillData skillData = skillDataRepo.Get(skillId, level: 1);
            if (skillData == null)
                return null;

            if (entity != null)
            {
                // 평타 검색
                foreach (var item in entity.battleSkillInfo.GetExtraBasicActiveSkills())
                {
                    if (item.SkillId == skillId)
                        return item;
                }

                // 일반 스킬 검색
                foreach (var item in entity.battleSkillInfo.GetActiveSkills())
                {
                    if (item.SkillId == skillId)
                        return item;
                }

                // 연계 스킬 검색
                ElementType skillElementType = skillData.element_type.ToEnum<ElementType>();
                foreach (var item in entity.battleSkillInfo.GetBlowActiveSkills(skillElementType))
                {
                    if (item.SkillId == skillId)
                        return item;
                }
            }

            SkillInfo skillInfo = new ActiveSkill();
            skillInfo.SetData(skillData);
            return skillInfo;
        }

        /// <summary>
        /// 정제된 엠펠리움 수량 업데이트
        /// </summary>
        private void OnRefineEmperieum(Response response)
        {
            int refineEmperium = response.GetInt("1");
            SetRefineEmperium(refineEmperium);
        }

        /// <summary>
        /// 길드습격 정제된 엠펠리움 수량 & 시간 변경 업데이트
        /// </summary>
        private void OnEmperiumAndGuildAttackTime(Response response)
        {
            int refineEmperium = response.GetInt("1");
            long guildAttackStartTime = response.GetLong("2");
            SetRefineEmperium(refineEmperium);
            SetGuildAttackTime(guildAttackStartTime);
        }

        void OnResponseCharUpdate(Response response)
        {
            int cid = response.GetInt("1");
            string costumeIds = response.GetUtfString("2");
            int weaponItemId = response.GetInt("3");

            PlayerBotEntity find = botEntityPool.FindPlayer(cid);
            if (find == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"방어코드! 존재하지 않는 플레이어가 무기,코스튬 변경: {nameof(cid)} = {cid}");
#endif
                return;
            }

            string[] results = costumeIds.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            int[] equipCostumeIds = new int[results.Length];
            for (int i = 0; i < results.Length; i++)
            {
                equipCostumeIds[i] = int.Parse(results[i]);
            }

            find.Inventory.UpdateCostumeWithWeapon(weaponItemId, equipCostumeIds);
        }

        /// <summary>
        /// 엠펠리움 생성
        /// </summary>
        private void OnCreateEmperium(Response response)
        {
            int refineEmperium = response.GetInt("1");
            long guildAttackStartTime = response.GetLong("2");
            int emperiumLevel = response.GetInt("3");

            SetRefineEmperium(refineEmperium);
            SetGuildAttackTime(guildAttackStartTime);
            SetEmperiumLevel(emperiumLevel);
            OnUpdateCreateEmperium?.Invoke();
        }

        /// <summary>
        /// 정제된 엠펠리움 세팅
        /// </summary>
        private void SetRefineEmperium(int value)
        {
            if (RefineEmperium == value)
                return;

            RefineEmperium = value;
            OnUpdateRefineEmperium?.Invoke();
        }

        /// <summary>
        /// 길드 습격 시작 시간 세팅
        /// </summary>
        private void SetGuildAttackTime(long guildAttackTime)
        {
            GuildAttackStartTime = guildAttackTime.ToDateTime();
            OnUpdateGuildAttackStartTime?.Invoke();
        }

        /// <summary>
        /// 엠펠리움 레벨 세팅
        /// </summary>
        private void SetEmperiumLevel(int level)
        {
            if (EmperiumLevel == level)
                return;

            PreEmperiumLevel = EmperiumLevel;
            EmperiumLevel = level;
            OnUpdateEmperiumLevel?.Invoke();
        }
    }
}