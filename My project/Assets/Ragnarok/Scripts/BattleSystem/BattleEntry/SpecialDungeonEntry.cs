using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace Ragnarok
{
    public class SpecialDungeonEntry : BattleEntry, IEqualityComparer<UnitEntity>
    {
        private const string SCENE_NAME = "TempSpecialMaze";
        private const string BGM = "Comodo_field_01";

        public const float SPEED = 10f;

        private const int TOWER_MAX_HP = 100;
        private const int TOWER_COUNT = 2;
        private const long TOWER_DAMAGE_TICK = 2000; // 타워 대미지 간격 (ms)

        private const float SEND_POSITION_INTERVAL_DISTANCE = 1f;
        private const int SYSTEM_CID = -1;
        private const int DEFAULT_SKILL_LEVEL = 1;
        private const int GUILD_MAZE_DEATH_COOL_TIME = 5;


        /******************** Repositories ********************/
        private readonly MultiBattlePlayers multiBattlePlayers;
        private readonly GuildMazeNexusList nexusInfoList;
        private readonly GuildMazeRockList rockInfoList;
        private readonly GuildMazeItemList itemInfoList;

        /******************** UIs ********************/
        private UIMainTop uiMainTop;
        private UIBattleMenu uiBattleMenu;
        private UIBattleInfo uiBattleInfo;
        private UIController uiController;
        private UIEnemyFocus uiEnemyFocus;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIBattleFail uiBattleFail;
        private UICamSelect uiCamSelect;
        private UIBattleMazeSkillList uiBattleMazeSkillList;
        private UIBattleSkillList uiBattleSkillList;

        /******************** Temp Data ********************/
        private Map map;
        private float savedDefaultSpeed;
        private UnitMovement.Mode savedMove;
        private IMultiPlayerInput[] multiPlayerInputs;
        private Dictionary<UnitEntity, IMultiPlayerInput> multiPlayerInputDic;
        private Vector3 playerHome; // 플레이어 생성 위치
        private Vector3 curLastPos; // 플레이어 마지막 위치
        private int playerTeamIdx; // 플레이어의 팀 인덱스
        private bool isInvisibleState; // 은신상태 여부
        private bool isSleepState; // 수면상태 여부
        private Dictionary<CharacterEntity, (Vector3 pos, int hp, IMultiPlayerInput input)> invisiblePlayerDic;

        public SpecialDungeonEntry() : base(BattleMode.SpecialDungeon)
        {
            player.SetState(UnitEntity.UnitState.GVG);

            multiBattlePlayers = new MultiBattlePlayers();
            nexusInfoList = new GuildMazeNexusList();
            rockInfoList = new GuildMazeRockList();
            itemInfoList = new GuildMazeItemList();
            multiPlayerInputDic = new Dictionary<UnitEntity, IMultiPlayerInput>();
            invisiblePlayerDic = new Dictionary<CharacterEntity, (Vector3 pos, int hp, IMultiPlayerInput)>();
        }

        public override IEnumerator<float> YieldExitBattle()
        {
            Task<Response> task = Protocol.REQUEST_GVG_ROOM_EXIT.SendAsync();
            yield return Timing.WaitUntilTrue(task.IsComplete);
            Response response = task.Result;
            IsSuccessExit = response.isSuccess;

            if (!IsSuccessExit)
                response.ShowResultCode();
        }

        protected override void Dispose()
        {
            base.Dispose();

            map = null;
            ClearMultiPlayers();
            multiPlayerInputs = null;
            multiPlayerInputDic.Clear();
            invisiblePlayerDic.Clear();

            nexusInfoList.Release();
            rockInfoList.ReleaseAll();
            itemInfoList.ReleaseAll();

            cameraController.SetCameraGrayScale(false);
            cameraController.SetCameraBlur(false);

            // 플레이어 스킬 쿨타임 초기화
            ResetSkillCooldownTime();
        }

        protected override void AddEvent()
        {
            base.AddEvent();

            // 플레이어, 전투 관련
            Protocol.REQUEST_GVG_ROOM_TRANSFORM.AddEvent(OnMazePlayerMove);
            Protocol.RECEIVE_GVG_ROOM_JOIN.AddEvent(OnMazePlayerJoin);
            Protocol.RECEIVE_GVG_ROOM_EXIT.AddEvent(OnMazePlayerExit);
            Protocol.RECEIVE_GVG_DAMAGE.AddEvent(OnMazePlayerDamage);
            Protocol.REQUEST_GVG_ATTACK_MOT.AddEvent(OnMazePlayerAttackMotion);
            Protocol.RECEIVE_GVG_DIE.AddEvent(OnMazePlayerDie);
            Protocol.RECEIVE_GVG_RESPAWN.AddEvent(OnMazePlayerRespawn);
            Protocol.REQUEST_GVG_ACTIVEBUFSKILL.AddEvent(OnMazePlayerSpellBuffSkill);
            Protocol.RECEIVE_GVG_GETCROWDCONTROL.AddEvent(OnMazePlayerGetCC);
            Protocol.RECEIVE_GVG_DOTDAMAGE.AddEvent(OnMazePlayerDotDamage);
            Protocol.RECEIVE_GVG_PLUSHP.AddEvent(OnMazePlayerHeal);

            // 타워 관련
            Protocol.RECEIVE_GVG_ATTACKTOWER_START.AddEvent(OnTowerAttackStart);
            Protocol.RECEIVE_GVG_ATTACKTOWER_ING.AddEvent(OnTowerAttackDamage);
            Protocol.RECEIVE_GVG_ATTACKTOWER_END.AddEvent(OnTowerAttackEnd);
            Protocol.RECEIVE_GVG_ATTACKTOWER_DESTROY.AddEvent(OnTowerDestroy);

            // 아이템/상태이상 관련
            Protocol.RECEIVE_GVG_ITEM_APPEAR.AddEvent(OnItemAppear);
            Protocol.RECEIVE_GVG_ITEM_DISAPPEAR.AddEvent(OnItemDisappear);
            Protocol.RECEIVE_GVG_SLEEP_START.AddEvent(OnSleepStart);
            Protocol.RECEIVE_GVG_SLEEP_END.AddEvent(OnSleepEnd);
            Protocol.RECEIVE_GVG_INVISIBLE_START.AddEvent(OnInvisibleStart);
            Protocol.RECEIVE_GVG_INVISIBLE_END.AddEvent(OnInvisibleEnd);
            Protocol.RECEIVE_GVG_ITEM_ATTACK.AddEvent(OnTeamDamaged);

            // 거석 관련
            Protocol.RECEIVE_GVG_ROCK_APPEAR_READY.AddEvent(OnRockReady);
            Protocol.RECEIVE_GVG_ROCK_APPEAR.AddEvent(OnRockAppear);
            Protocol.RECEIVE_GVG_ROCK_DISAPPEAR.AddEvent(OnRockDisappear);


            player.OnUseSkill += OnUseSkill;
            player.OnApplySkill += OnApplySkill;
        }

        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            // 플레이어, 전투 관련
            Protocol.REQUEST_GVG_ROOM_TRANSFORM.RemoveEvent(OnMazePlayerMove);
            Protocol.RECEIVE_GVG_ROOM_JOIN.RemoveEvent(OnMazePlayerJoin);
            Protocol.RECEIVE_GVG_ROOM_EXIT.RemoveEvent(OnMazePlayerExit);
            Protocol.RECEIVE_GVG_DAMAGE.RemoveEvent(OnMazePlayerDamage);
            Protocol.REQUEST_GVG_ATTACK_MOT.RemoveEvent(OnMazePlayerAttackMotion);
            Protocol.RECEIVE_GVG_DIE.RemoveEvent(OnMazePlayerDie);
            Protocol.RECEIVE_GVG_RESPAWN.RemoveEvent(OnMazePlayerRespawn);
            Protocol.REQUEST_GVG_ACTIVEBUFSKILL.RemoveEvent(OnMazePlayerSpellBuffSkill);
            Protocol.RECEIVE_GVG_GETCROWDCONTROL.RemoveEvent(OnMazePlayerGetCC);
            Protocol.RECEIVE_GVG_DOTDAMAGE.RemoveEvent(OnMazePlayerDotDamage);
            Protocol.RECEIVE_GVG_PLUSHP.RemoveEvent(OnMazePlayerHeal);

            // 타워 관련
            Protocol.RECEIVE_GVG_ATTACKTOWER_START.RemoveEvent(OnTowerAttackStart);
            Protocol.RECEIVE_GVG_ATTACKTOWER_ING.RemoveEvent(OnTowerAttackDamage);
            Protocol.RECEIVE_GVG_ATTACKTOWER_END.RemoveEvent(OnTowerAttackEnd);
            Protocol.RECEIVE_GVG_ATTACKTOWER_DESTROY.RemoveEvent(OnTowerDestroy);

            // 아이템/상태이상 관련
            Protocol.RECEIVE_GVG_ITEM_APPEAR.RemoveEvent(OnItemAppear);
            Protocol.RECEIVE_GVG_ITEM_DISAPPEAR.RemoveEvent(OnItemDisappear);
            Protocol.RECEIVE_GVG_SLEEP_START.RemoveEvent(OnSleepStart);
            Protocol.RECEIVE_GVG_SLEEP_END.RemoveEvent(OnSleepEnd);
            Protocol.RECEIVE_GVG_INVISIBLE_START.RemoveEvent(OnInvisibleStart);
            Protocol.RECEIVE_GVG_INVISIBLE_END.RemoveEvent(OnInvisibleEnd);
            Protocol.RECEIVE_GVG_ITEM_ATTACK.RemoveEvent(OnTeamDamaged);

            // 거석 관련
            Protocol.RECEIVE_GVG_ROCK_APPEAR_READY.RemoveEvent(OnRockReady);
            Protocol.RECEIVE_GVG_ROCK_APPEAR.RemoveEvent(OnRockAppear);
            Protocol.RECEIVE_GVG_ROCK_DISAPPEAR.RemoveEvent(OnRockDisappear);

            player.OnUseSkill -= OnUseSkill;
            player.OnApplySkill -= OnApplySkill;
        }

        protected override void InitCanvas()
        {
            uiMainTop = UI.Show<UIMainTop>();
            uiBattleMenu = UI.Show<UIBattleMenu>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiController = UI.Show<UIController>();
            uiEnemyFocus = UI.Show<UIEnemyFocus>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiBattleFail = UI.Show<UIBattleFail>();
            uiCamSelect = UI.Show<UICamSelect>();
            uiBattleMazeSkillList = UI.Show<UIBattleMazeSkillList>();
            uiBattleSkillList = UI.Show<UIBattleSkillList>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiBattleMenu.OnExit += ExitEntry;
            uiBattleFail.OnConfirm += OnBattleFailConfirm;
            uiBattleMazeSkillList.OnSelect += OnSelectSkill;
            uiBattleMazeSkillList.OnToggleSkill += OnToggleSkill;
            uiBattleSkillList.OnSelect += OnSelectSkill;
            uiBattleSkillList.OnToggleSkill += OnToggleSkill;

            // Initialize
            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);
            uiBattleFail.Hide();
            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit); // 나가기
            uiBattleInfo.SetMode(UIBattleInfo.Mode.TYPE_1);
            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
            uiBattleMazeSkillList.Hide();

            cameraController.SetView(CameraController.View.Quater50_10);
            cameraController.AddMask(Layer.MAZE_OTHER_PLAYER, Layer.MAZE_ENEMY);
        }

        protected override void DisposeCanvas()
        {
            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
            uiBattleMenu.OnExit -= ExitEntry;
            uiBattleFail.OnConfirm -= OnBattleFailConfirm;
            uiBattleMazeSkillList.OnSelect -= OnSelectSkill;
            uiBattleMazeSkillList.OnToggleSkill -= OnToggleSkill;
            uiBattleSkillList.OnSelect -= OnSelectSkill;
            uiBattleSkillList.OnToggleSkill -= OnToggleSkill;

            UI.Close<UIMainTop>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIController>();
            UI.Close<UIEnemyFocus>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIBattleFail>();
            UI.Close<UICamSelect>();
            UI.Close<UIBattleMazeSkillList>();
            UI.Close<UIBattleSkillList>();

            uiMainTop = null;
            uiBattleMenu = null;
            uiBattleInfo = null;
            uiController = null;
            uiEnemyFocus = null;
            uiBattlePlayerStatus = null;
            uiBattleFail = null;
            uiCamSelect = null;
            uiBattleMazeSkillList = null;
            uiBattleSkillList = null;
        }

        protected override void OnLocalize()
        {
            uiBattleInfo.Set(LocalizeKey._7036.ToText()); // 길드 미로
        }

        void OnStartController()
        {
            if (player.IsDie)
                return;

            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            CameraUtils.InvokePlayerTrackingEffect();

            actor.Movement.Stop();
            actor.AI.SetInputMove(isControl: true);
        }

        void OnDragController(Vector2 position)
        {
            if (position == Vector2.zero)
                return;

            Camera mainCamera = Camera.main;

            if (mainCamera == null)
                return;

            if (player.IsDie)
                return;

            if (player.battleCrowdControlInfo.GetCannotMove())
                return;

            if (isSleepState)
                return;

            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            Vector3 motion = mainCamera.transform.TransformDirection(position);
            motion.y = 0;
            motion.Normalize();
            actor.Movement.Move(motion);

            //Vector3 des = actor.LastPosition + (motion * (actor.Movement.GetDefaultSpeed() * player.MoveSpeedRate) * Time.deltaTime);
            //int[] posArray = { (int)(des.x * 1000), (int)(des.y * 1000), (int)(des.z * 1000) };
            //var sfs = Protocol.NewInstance();
            //sfs.PutIntArray("2", posArray);
            //Protocol.REQUEST_GVG_ROOM_TRANSFORM.SendAsync(sfs).WrapNetworkErrors();
        }

        void OnResetController()
        {
            CameraUtils.Zoom(CameraZoomType.None);

            if (player.IsDie)
                return;

            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            actor.AI.SetInputMove(isControl: false);
            actor.Movement.Stop();

            // 이동이 끝난 시점에 현재 위치 업로드
            SendCurrentPosition(player.LastPosition);
        }

        public override IEnumerator<float> YieldEnterBattle()
        {
            yield return Timing.WaitUntilDone(YieldEnterBattle(id: 0));
        }

        public override IEnumerator<float> YieldEnterBattle(int id)
        {
            Task<Response> task = Protocol.REQUEST_GVG_ROOM_JOIN.SendAsync();

            yield return Timing.WaitUntilTrue(task.IsComplete);
            Response response = task.Result;
            IsSuccessEnter = response.isSuccess;

            if (!IsSuccessEnter)
            {
                response.ShowResultCode();
                yield break;
            }

            // 미로맵 다른 플레이어 정보
            if (response.ContainsKey("2"))
                multiPlayerInputs = response.GetPacketArray<SpecialDungeonCharacterPacket>("2");

            // 플레이어 위치
            int[] playerPos = { response.GetInt("3"), response.GetInt("4"), response.GetInt("5") };
            playerHome = IntArrayToVector3(playerPos);

            // 플레이어 팀 식별
            playerTeamIdx = response.GetByte("6");

            // 타워 상태
            int hp;
            byte towerState;
            short xIndex, zIndex;

            // 0번 타워
            hp = response.GetInt("7"); // 현재 HP (최대 100)
            towerState = response.GetByte("9"); // 상태 (0: 생존, 1:파괴)
            xIndex = response.GetShort("27");
            zIndex = response.GetShort("28");
            nexusInfoList.Add(0, hp, towerState, xIndex, zIndex);

            // 1번 타워
            hp = response.GetInt("8"); // 현재 HP (최대 100)
            towerState = response.GetByte("10"); // 상태 (0: 생존, 1:파괴)
            xIndex = response.GetShort("29");
            zIndex = response.GetShort("30");
            nexusInfoList.Add(1, hp, towerState, xIndex, zIndex);

            // 거석 상태
            // 상태, 아이디, 좌표(x, z) *2 11~18
            byte rockState;
            int rockId;

            // 거석 1
            rockState = response.GetByte("11");
            if (rockState != GuildMazeRockInfo.RockState.None.ToByteValue())
            {
                rockId = response.GetInt("12");
                xIndex = response.GetShort("13");
                zIndex = response.GetShort("14");
                rockInfoList.Add(rockId, xIndex, zIndex, rockState);
            }

            // 거석 2
            rockState = response.GetByte("15");
            if (rockState != GuildMazeRockInfo.RockState.None.ToByteValue())
            {
                rockId = response.GetInt("16");
                xIndex = response.GetShort("17");
                zIndex = response.GetShort("18");
                rockInfoList.Add(rockId, xIndex, zIndex, rockState);
            }

            // 아이템 상태
            // 상태, 아이디, 좌표(x, z) 19~26
            byte itemState;
            int itemId;

            // 아이템 1
            itemState = response.GetByte("19");
            if (itemState != GuildMazeItemInfo.ItemState.None.ToByteValue())
            {
                itemId = response.GetInt("20");
                xIndex = response.GetShort("21");
                zIndex = response.GetShort("22");
                itemInfoList.Add(itemId, xIndex, zIndex, itemState);
            }

            // 아이템 2
            itemState = response.GetByte("23");
            if (itemState != GuildMazeItemInfo.ItemState.None.ToByteValue())
            {
                itemId = response.GetInt("24");
                xIndex = response.GetShort("25");
                zIndex = response.GetShort("26");
                itemInfoList.Add(itemId, xIndex, zIndex, itemState);
            }
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            string sceneName = SCENE_NAME;
            string bgmName = BGM;
            bool isChangeScene = true;
            SceneLoader.SceneType sceneType = SceneLoader.SceneType.Dungeon;

            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비
        }

        protected override void OnSceneLoaded(GameObject[] roots)
        {
            foreach (var item in roots)
            {
                map = item.GetComponent<Map>();

                if (map)
                    break;
            }
        }

        protected override void OnReady()
        {
            base.OnReady();
            uiBattlePlayerStatus.SetPlayer(player);
            uiBattleMazeSkillList.SetCharacter(player);
            uiBattleSkillList.SetCharacter(player);
        }

        protected override IEnumerator<float> YieldComposeScene()
        {
            yield return Timing.WaitUntilDone(YieldSpawnCharacter(player), TAG);

            for (int i = 0; i < multiPlayerInputs.Length; i++)
            {
                // 멀티 플레이어는 씬 구성 기다림 제외
                /*yield return */
                Timing.WaitUntilDone(YieldSpawnMultiPlayer(multiPlayerInputs[i]), TAG);
            }

            yield return Timing.WaitUntilDone(YieldSpawnNexus(), TAG);

            // 넥서스 현재 체력 설정
            foreach (var nexusInfo in nexusInfoList)
            {
                if (nexusInfo.IsInvalid)
                {
                    Debug.LogError($"[OnAllReady] {nexusInfo.TeamIndex}번 Nexus is Invalid. (Entity가 할당되지 않음)");
                    continue;
                }

                nexusInfo.Entity.SetCurrentHp(nexusInfo.SavedHp);
            }

            // 거석 리포지션
            foreach (var rockInfo in rockInfoList)
            {
                if (rockInfo.IsInvalid)
                    continue;

                rockInfo.Refresh();
            }

            // 아이템 리포지션
            foreach (var itemInfo in itemInfoList)
            {
                if (itemInfo.IsInvalid)
                    continue;

                itemInfo.Refresh();
            }

            // 플레이어 스킬 쿨타임 초기화
            ResetSkillCooldownTime();
        }

        private IEnumerator<float> YieldSpawnCharacter(CharacterEntity character)
        {
            if (impl.Add(character, isEnemy: false))
            {
                character.OnSpawnActor += OnCharacterSpawn;
                character.OnDespawnActor += OnCharacterDespawn;

                UnitActor characterActor = character.SpawnActor();
                cameraController.SetPlayer(characterActor.CachedTransform); // 카메라 타겟 세팅

                characterActor.AI.SetHomePosition(playerHome, isWarp: false); // 위치 세팅
                characterActor.Movement.ForceWarp(playerHome);
                curLastPos = characterActor.AI.HomePos;
                yield return Timing.WaitForOneFrame;
            }
        }

        private IEnumerator<float> YieldSpawnMultiPlayer(IMultiPlayerInput multiPlayer)
        {
            CharacterEntity character = multiBattlePlayers.Add(multiPlayer, UnitEntity.UnitState.GVGMultiPlayer);

            bool isEnemy = (multiPlayer.TeamIndex != playerTeamIdx);
            if (impl.Add(character, isEnemy))
            {
                multiPlayerInputDic[character] = multiPlayer; // Entity마다 멀티캐릭터 정보 저장
                GVGPlayerState state = multiPlayer.State.ToEnum<GVGPlayerState>();
                if (state == GVGPlayerState.Idle) // 죽었으면 Actor Spawn 안 함.
                {
                    Vector3 pos = new Vector3(multiPlayer.PosX, multiPlayer.PosY, multiPlayer.PosZ);
                    SpawnMultiPlayerActor(character, pos, multiPlayer.CurHp, multiPlayer);

                    yield return Timing.WaitForOneFrame;
                }
            }
        }

        private IEnumerator<float> YieldSpawnNexus()
        {
            for (int i = 0; i < TOWER_COUNT; ++i)
            {
                NexusEntity entity = MonsterEntity.Factory.CreateNexus(teamIndex: i, maxHp: TOWER_MAX_HP);
                GuildMazeNexusInfo nexusInfo = nexusInfoList.GetNexusInfo(i);
                nexusInfo.SetEntity(entity);

                bool isEnemy = (playerTeamIdx != i);
                if (impl.Add(entity, isEnemy))
                {
                    NexusActor actor = entity.SpawnActor() as NexusActor;
                    actor.Movement.SetMode(UnitMovement.Mode.NavMesh);
                    actor.AI.SetHomePosition(nexusInfo.SavedPosition, isWarp: false);
                    actor.Movement.ForceWarp(nexusInfo.SavedPosition);

                    yield return Timing.WaitForOneFrame;

                    if (!actor.Movement.Agent.isOnNavMesh)
                    {
                        WarpToNavMesh(actor, nexusInfo.SavedPosition);
                        if (!actor.Movement.Agent.isOnNavMesh)
                        {
                            Debug.LogError("Nexus isOnNavMesh False.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Y축에서 NavMesh찾아서 이동
        /// </summary>
        private void WarpToNavMesh(NexusActor nexusActor, Vector3 pos)
        {
            bool isHit = NavMesh.SamplePosition(pos, out var hit, Constants.Map.GuildMaze.NAVMESH_SAMPLE_POSITION_RANGE, NavMesh.AllAreas);
            if (!isHit)
            {
                Debug.LogError("Nexus NavMesh 발견되지 않음");
                return;
            }

            nexusActor.Movement.Agent.Warp(hit.position);
        }

        /// <summary>
        /// 멀티 플레이어 Actor 생성
        /// </summary>
        private void SpawnMultiPlayerActor(CharacterEntity character, Vector3 pos, int curHp, IMultiPlayerInput multiPlayerInput)
        {
            UnitActor characterActor = character.SpawnActor(); // 유닛 소환
            characterActor.AI.SetHomePosition(pos, isWarp: false);
            characterActor.Movement.ForceWarp(pos);
            characterActor.Movement.SetMode(UnitMovement.Mode.NavMesh);
            characterActor.Movement.SetDefaultSpeed(SPEED);
            characterActor.AI.ReadyToBattle();

            characterActor.AI.StopHpRegen();
            character.SetCurrentHp(curHp);

            characterActor.EffectPlayer.ShowName();
        }

        protected override void OnAllReady()
        {
            base.OnAllReady();

            player.SetCurrentHp(player.MaxHP);
            player.SetCurrentMp(player.MaxMp);

            UnitActor actor = player.GetActor();
            if (actor)
            {
                actor.EffectPlayer.ShowUnitCircle();

                savedMove = actor.Movement.GetMode();
                savedDefaultSpeed = actor.Movement.GetDefaultSpeed();

                actor.Movement.SetMode(UnitMovement.Mode.NavMesh);
                actor.Movement.SetDefaultSpeed(SPEED);

                CharacterEffectPlayer characterEffectPlayer = actor.EffectPlayer as CharacterEffectPlayer;
                characterEffectPlayer.ShowControllerAssist(uiController);
            }

            // 체력 리젠 중지
            foreach (var unit in unitList)
            {
                unit.GetActor()?.AI.StopHpRegen();
            }
        }

        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
            if (impl.IsPlayerDead()) // 플레이어가 죽었는지 확인
            {
                uiBattleMazeSkillList.SetCharacter(player);
                uiBattleSkillList.SetCharacter(player);
            }
        }

        void OnCharacterSpawn(UnitActor actor)
        {
            actor.Movement.OnMove += OnMove;
            actor.Movement.OnRushEnd += OnRushEnd;
            actor.Movement.OnKnockBackEnd += OnKnockBackEnd;
        }

        void OnCharacterDespawn(UnitActor actor)
        {
            actor.Movement.SetMode(savedMove);
            actor.Movement.SetDefaultSpeed(savedDefaultSpeed);

            actor.Movement.OnMove -= OnMove;
            actor.Movement.OnRushEnd -= OnRushEnd;
            actor.Movement.OnKnockBackEnd -= OnKnockBackEnd;
            actor.Entity.OnSpawnActor -= OnCharacterSpawn;
            actor.Entity.OnDespawnActor -= OnCharacterDespawn;
        }

        /// <summary>
        /// 미로맵 다른 모든 플레이어 제거
        /// </summary>
        private void ClearMultiPlayers()
        {
            foreach (var item in multiBattlePlayers)
            {
                if (item == null)
                    continue;

                item.DespawnActor();
                impl.Remove(item);
            }

            multiBattlePlayers.Recycle();
        }

        #region 캐릭터 상호작용
        /// <summary>
        /// 다른 유저 미로맵 입장
        /// </summary>
        /// <param name="response"></param>
        void OnMazePlayerJoin(Response response)
        {
            Timing.RunCoroutine(YieldSpawnMultiPlayer(response.GetPacket<SpecialDungeonCharacterPacket>("1")), TAG);
        }

        /// <summary>
        /// 다른 유저 미로맵 퇴장
        /// </summary>
        /// <param name="response"></param>
        void OnMazePlayerExit(Response response)
        {
            int cid = response.GetInt("1");

            CharacterEntity entity = multiBattlePlayers.Find(cid);
            if (entity == null)
                return;

            if (invisiblePlayerDic.ContainsKey(entity))
            {
                invisiblePlayerDic.Remove(entity);
            }

            entity.DespawnActor();
            impl.Remove(entity);
            multiBattlePlayers.Remove(entity);
        }

        /// <summary>
        /// 미로맵 플레이어 이동
        /// </summary>
        /// <param name="response"></param>
        private void OnMazePlayerMove(Response response)
        {
            if (!response.ContainsKey("1") || !response.ContainsKey("2"))
            {
                Debug.LogError($"[REQUEST_GVG_ROOM_TRANSFORM] Responded Fail.");
                return;
            }
            int cid = response.GetInt("1");
            int[] pos = response.GetIntArray("2");

            // 방어코드
            if (pos == null)
                return;

            Vector3 des = IntArrayToVector3(pos);

            bool isMe = IsMe(cid);
            if (isMe)
            {
                if (des != player.LastPosition)
                {
                    player.GetActor().Movement.ForceWarp(des);
                }
                return;
            }

            var entity = FindUnitEntity(cid, hasActor: false);
            var actor = entity?.GetActor();
            if (actor is null)
            {
                // 은신 유닛 예외 처리
                if (entity != null && invisiblePlayerDic.ContainsKey(entity))
                {
                    var data = invisiblePlayerDic[entity];
                    data.pos = des;
                    invisiblePlayerDic[entity] = data;
                    return;
                }

                Debug.LogError($"[OnMazePlayerMove] 해당 유저를 찾을 수 없음. {cid} (hasEntity : {entity != null})");
                return;
            }

            actor.Movement.ForceSetDestination(des);
        }

        /// <summary>
        /// 내 캐릭터 이동
        /// </summary>
        /// <param name="position"></param>
        private void OnMove(Vector3 position)
        {
            if (player.IsDie)
            {
                Debug.LogError("플레이어가 죽어있는데 무브 신호가 보내졌다.");
                return;
            }

            if (Vector3.Distance(curLastPos, position) > SEND_POSITION_INTERVAL_DISTANCE)
            {
                SendCurrentPosition(position);
            }
        }

        /// <summary>
        /// 현재 좌표 전송
        /// </summary>
        private void SendCurrentPosition(Vector3 position)
        {
            curLastPos = position;
            int[] posArray = Vector3ToIntArray(curLastPos);
            var sfs = Protocol.NewInstance();
            sfs.PutIntArray("2", posArray);
            Protocol.REQUEST_GVG_ROOM_TRANSFORM.SendAsync(sfs).WrapNetworkErrors();
        }

        /// <summary>
        /// 내 캐릭터 돌진 끝났을 때
        /// </summary>
        private void OnRushEnd()
        {
            // 내 위치 업로드
            int[] pos = Vector3ToIntArray(player.LastPosition);
            var param = Protocol.NewInstance();
            param.PutIntArray("2", pos);

            Protocol.REQUEST_GVG_ROOM_TRANSFORMEX.SendAsync(param).WrapNetworkErrors();
        }

        /// <summary>
        /// 내 캐릭터 넉백 끝났을 때
        /// </summary>
        private void OnKnockBackEnd()
        {
            // 내 위치 업로드
            int[] pos = Vector3ToIntArray(player.LastPosition);
            var param = Protocol.NewInstance();
            param.PutIntArray("2", pos);

            Protocol.REQUEST_GVG_ROOM_TRANSFORMEX.SendAsync(param).WrapNetworkErrors();
        }

        /// <summary>
        /// 내가 적을 공격했을 때 (공격 시작)
        /// </summary>
        private void OnUseSkill(UnitEntity target, SkillInfo skillInfo)
        {
            CharacterEntity targetCharacterEntity = target as CharacterEntity;

            var param = Protocol.NewInstance();
            param.PutInt("1", player.Character.Cid); // 공격자 아이디
            param.PutInt("2", targetCharacterEntity.Character.Cid); // 피격자 아이디
            param.PutInt("3", skillInfo.SkillId); // 스킬 아이디

            Protocol.REQUEST_GVG_ATTACK_MOT.SendAsync(param).WrapNetworkErrors();
        }

        /// <summary>
        /// 내가 적을 공격했을 때 (대미지 적용) + 내가 버프 스킬을 시전했을 때
        /// </summary>
        private void OnApplySkill(UnitEntity[] targets, SkillInfo skillInfo)
        {
            var param = Protocol.NewInstance();

            // 타겟 CID 목록 구하기
            var targetCIDs = (from e in targets
                              let ch = e as CharacterEntity
                              where ch != null
                              select ch.Character.Cid).ToArray();
            param.PutIntArray("3", targetCIDs);

            Debug.Log($"CIDs : [{string.Join(", ", targetCIDs)}]"); // 로그

            // 사용한 스킬의 슬롯No 구하기
            int skillSlotCount = player.Skill.SkillSlotCount;
            long skillSlotNo = 0L;
            for (int i = 0; i < skillSlotCount; ++i)
            {
                var slotValue = player.Skill.GetSlotInfo(i);
                var slotSkill = player.Skill.GetSkill(slotValue.SkillNo, isBattleSkill: true);

                if (slotSkill != null)
                {
                    if (slotSkill.SkillId == skillInfo.SkillId)
                        skillSlotNo = slotValue.SlotNo;
                }
            }
            param.PutLong("4", skillSlotNo);

            // 공격형 스킬
            if (skillInfo.ActiveSkillType == ActiveSkill.Type.Attack)
            {
                param.PutLong("2", skillInfo.SkillId);

                Protocol.REQUEST_GVG_ATTACK.SendAsync(param).WrapNetworkErrors();
                return;
            }

            // 회복형, 버프형 스킬
            if (skillSlotNo == 0L)
            {
                Debug.LogError("[OnApplySkill] 버프스킬 SlotNo 찾지 못함.");
                return;
            }

            Protocol.REQUEST_GVG_ACTIVEBUFSKILL.SendAsync(param).WrapNetworkErrors();
        }

        /// <summary>
        /// 나를 포함, 특정 유저가 대미지를 받았을 때 (모든 유저 수신)
        /// </summary>
        private void OnMazePlayerDamage(Response response)
        {
            int cid = response.GetInt("1"); // 대미지 받은 유저 CID
            int dmg = response.GetInt("2"); // 0: 미스
            bool isCritical = (response.GetByte("3") == 1); // 0:일반, 1:크리
            int curHp = response.GetInt("4"); // 남은 HP
            int attackerCID = response.GetInt("5"); // 공격자 CID
            int skillId = response.GetInt("6"); // 공격스킬아이디

            bool isSystemDmg = (attackerCID == SYSTEM_CID); // -1 : 시스템에 의한 대미지 (아이템 등)

            var target = FindUnitEntity(cid, hasActor: false);
            if (target is null)
            {
                Debug.LogError($"[OnMazePlayerDamage] CID({cid}) 피격 대상 없음.");
                return;
            }

            // 은신유닛에 대한 처리 (HP 변경 처리)
            if (invisiblePlayerDic.ContainsKey(target))
            {
                var data = invisiblePlayerDic[target];
                data.hp = curHp;
                invisiblePlayerDic[target] = data;
                return;
            }

            CharacterActor targetActor = target.GetActor() as CharacterActor;
            if (targetActor is null)
            {
                Debug.LogError($"[OnMazePlayerDamage] CID({cid}) 피격 대상 Actor 없음.");
                return;
            }


            var attacker = FindUnitEntity(attackerCID);
            if (attacker is null)
            {
                if (!isSystemDmg) // 시스템에 의한 대미지 (아이템 등)
                {
                    Debug.LogError($"[OnMazePlayerDamage] CID({attackerCID}) 공격자 없음.");
                    return;
                }
            }

            // 대미지 스펙 처리
            int dmgPerHit = dmg;
            int blowCount = 1;
            bool isBasicActiveSkill = false;
            ElementType elementType = ElementType.Neutral;
            isCritical = isSystemDmg ? true : isCritical;
            bool isRush = false;
            if (!isSystemDmg)
            {
                SkillData skillData = SkillDataManager.Instance.Get(skillId, DEFAULT_SKILL_LEVEL);
                SkillInfo skillInfo = new ActiveSkill();
                skillInfo.SetData(skillData);

                dmgPerHit = Mathf.Max(1, dmg / skillInfo.BlowCount);
                blowCount = skillInfo.BlowCount;
                isBasicActiveSkill = skillInfo.IsBasicActiveSkill;
                elementType = skillInfo.ElementType;
                isRush = skillInfo.IsRush;
            }

            target.ApplyDamage(attacker, dmgPerHit, blowCount, isCritical, isBasicActiveSkill, elementType, totalDamage: dmg);

            // 넉백
            if (isRush)
            {
                Vector3 dir = (target.LastPosition - attacker.LastPosition).normalized;
                targetActor.Movement.KnockBack(dir, Constants.Battle.RushKnockBackPower);
            }

            if (target.CurHP != curHp)
            {
                Debug.LogError($"[OnMazePlayerDamage] CID({cid}) 체력 재조정. {target.CurHP} -> {curHp}");
                target.SetCurrentHp(curHp);
            }
        }

        /// <summary>
        /// 다른 유저의 공격 모션 신호 수신 (공격자 제외 모두가 수신)
        /// </summary>
        private void OnMazePlayerAttackMotion(Response response)
        {
            int attackerCID = response.GetInt("1");
            int targetCID = response.GetInt("2");
            int skillId = response.GetInt("3");

            // 공격자 찾기
            var attackerActor = FindUnitEntity(attackerCID)?.GetActor();
            if (attackerActor is null)
            {
                Debug.LogError($"[OnMazePlayerAttack] 공격자({attackerCID}) 없음.");
                return;
            }

            // 피격자 찾기
            var targetActor = FindUnitEntity(targetCID)?.GetActor();
            if (targetActor is null)
            {
                Debug.LogError($"[OnMazePlayerAttack] 피격자({targetCID}) 없음.");
                return;
            }

            // 스킬 찾기
            SkillData skillData = SkillDataManager.Instance.Get(skillId, DEFAULT_SKILL_LEVEL);
            if (skillData is null)
            {
                Debug.LogError($"[OnMazePlayerAttack] 해당 스킬({skillId}) 없음.");
                return;
            }
            SkillInfo skillInfo = new ActiveSkill();
            skillInfo.SetData(skillData);

            // 스킬 사용
            attackerActor.UseSkill(targetActor, skillInfo, isChainableSkill: false, queueIdleMotion: true);
        }

        /// <summary>
        /// 다른 유저의 사망 신호 수신 (모든 유저 수신)
        /// </summary>
        private void OnMazePlayerDie(Response response)
        {
            int dieUnitCID = response.GetInt("1");
            int dmg = response.GetInt("2"); // 0: 미스
            bool isCriticalDmg = (response.GetByte("3") == 1); // 0:일반, 1:크리
            int attackerCID = response.GetInt("4");
            int skillId = response.GetInt("5");

            bool isSystemKill = (attackerCID == SYSTEM_CID);

            var dieUnitEntity = FindUnitEntity(dieUnitCID, hasActor: false);
            var dieUnitActor = dieUnitEntity?.GetActor();
            if (dieUnitActor is null)
            {
                // 은신 유닛 예외 처리
                if (dieUnitEntity != null && invisiblePlayerDic.ContainsKey(dieUnitEntity))
                {
                    Debug.Log("[OnMazePlayerDie] 은신 유닛 사망.");
                    var data = invisiblePlayerDic[dieUnitEntity];
                    data.hp = 0;
                    invisiblePlayerDic[dieUnitEntity] = data;
                    return;
                }

                Debug.LogError($"[OnMazePlayerDie] 사망 대상({dieUnitCID}) 없음.");
                return;
            }

            UnitEntity attacker = null;
            if (!isSystemKill) // -1 : 시스템에 의해 사망
            {
                attacker = FindUnitEntity(attackerCID);
                if (attacker is null)
                {
                    Debug.LogError($"[OnMazePlayerDie] CID({attackerCID}) 공격자 없음.");
                    return;
                }

                // 대미지 적용
                SkillData skillData = SkillDataManager.Instance.Get(skillId, 1);
                SkillInfo skillInfo = new ActiveSkill();
                skillInfo.SetData(skillData);

                int dmgPerHit = Mathf.Max(1, dmg / skillInfo.BlowCount);
                dieUnitEntity.ApplyDamage(attacker, dmgPerHit, skillInfo.BlowCount, isCriticalDmg, skillInfo.IsBasicActiveSkill, skillInfo.ElementType, totalDamage: dmg);
            }

            // 죽지 않았으면 강제로 사망처리
            if (!dieUnitEntity.IsDie)
            {
                if (isSystemKill)
                {
                    Debug.Log("시스템에 의해서 사망.");
                }
                else
                {
                    Debug.LogError($"죽지않아서 강제로 사망처리 attacker : {attackerCID}");
                }
                dieUnitEntity.Die(attacker);
            }

            // 내가 죽은거면 20초 카운트 팝업 
            bool isMe = IsMe(dieUnitCID);
            if (isMe)
            {
                uiBattleFail.Show(UIBattleFail.ConfirmType.Exit, GUILD_MAZE_DEATH_COOL_TIME, isShowButton: false);
            }
        }

        /// <summary>
        /// 특정 유저의 리스폰 신호 수신 (모든 유저 수신)
        /// </summary>
        private void OnMazePlayerRespawn(Response response)
        {
            int respawnUnitCID = response.GetInt("1");
            int[] respawnPos = response.GetIntArray("2");
            int hp = response.GetInt("3");

            // 방어코드
            if (respawnPos == null)
                return;

            Vector3 pos = IntArrayToVector3(respawnPos);

            var respawnUnitEntity = FindUnitEntity(respawnUnitCID, hasActor: false);
            if (respawnUnitEntity is null)
            {
                Debug.LogError($"[OnMazePlayerRespawn] CID({respawnUnitCID}) 대상 없음.");
                return;
            }

            // 내 플레이어가 리스폰한 경우
            bool isMe = IsMe(respawnUnitCID);
            if (isMe)
            {
                var actor = respawnUnitEntity.GetActor();
                actor.AI.SetHomePosition(pos, isWarp: false);
                actor.Movement.ForceWarp(pos);
                actor.AI.ReadyToBattle();

                actor.AI.StopHpRegen();
                respawnUnitEntity.SetCurrentHp(hp);

                uiBattleFail.Hide();

                PlayerSleepEnd(); // 수면 상태이상 해제
                PlayerInvisibleEnd(); // 은신 해제
                return;
            }

            // 다른 플레이어가 리스폰한 경우
            if (!multiPlayerInputDic.ContainsKey(respawnUnitEntity))
            {
                Debug.LogError($"[OnMazePlayerRespawn] 해당 캐릭터의 MultiPlayerInput을 찾을 수 없음.");
                return;
            }

            SpawnMultiPlayerActor(respawnUnitEntity, pos, hp, multiPlayerInputDic[respawnUnitEntity]);
            invisiblePlayerDic.Remove(respawnUnitEntity);
        }

        /// <summary>
        /// 스킬 선택 사용
        /// </summary>
        private void OnSelectSkill(SkillInfo info, UIBattleNormalSkillSlot.SlotType slotType)
        {
            switch (slotType)
            {
                case UIBattleNormalSkillSlot.SlotType.Skill:
                    if (player.IsDie)
                        return;

                    UnitActor unitActor = player.GetActor();
                    if (unitActor == null)
                        return;

                    int mpCost = player.CurMp - info.MpCost;
                    if (mpCost < 0)
                    {
                        UI.ShowToastPopup(LocalizeKey._90142.ToText()); // 스킬 포인트가 부족합니다.
                        return;
                    }

                    unitActor.AI.SetInputSkill(info);
                    break;
            }
        }

        private void OnToggleSkill()
        {
            if (uiBattleSkillList.IsVisible)
            {
                uiBattleSkillList.Hide();
                uiBattleMazeSkillList.Show();
            }
            else
            {
                uiBattleSkillList.Show();
                uiBattleMazeSkillList.Hide();
            }
        }

        /// <summary>
        /// 특정 유저의 버프 스킬 시전 신호 수신 (모든 유저 수신)
        /// </summary>
        private void OnMazePlayerSpellBuffSkill(Response response)
        {
            int bufferCID = response.GetInt("1");
            int skillId = response.GetInt("2");
            int[] buffTargetCIDs = response.GetIntArray("3");
            long skillSlotId = response.GetLong("4"); // 사용하지 않음

            //            (int)1: 1203
            //(long) 2: 6749001
            //(int_array) 3: [System.Int32[]]
            //	(long) 4: 2651001

            bool isMyBuff = IsMe(bufferCID);

            // 버퍼와 타겟 찾기
            var buffer = FindUnitEntity(bufferCID);
            if (buffer is null)
                return;
            var targets = FindUnitEntities(buffTargetCIDs);
            if (targets is null)
                return;

            SkillData skillData = SkillDataManager.Instance.Get(skillId, DEFAULT_SKILL_LEVEL);
            if (skillData is null)
            {
                Debug.LogError($"[OnMazePlayerSpellBuffSkill] 해당 스킬({skillId}) 없음.");
                return;
            }
            SkillInfo skillInfo = new ActiveSkill();
            skillInfo.SetData(skillData);

            // 대상들에게 버프 적용
            foreach (var target in targets)
            {
                target.AddBattleBuff(skillInfo);
            }
        }

        /// <summary>
        /// 특정 유저 상태이상 시작(모든 유저 수신)
        /// </summary>
        private void OnMazePlayerGetCC(Response response)
        {
            int targetCID = response.GetInt("1");
            byte ccIndex = response.GetByte("2");
            CrowdControlType ccType = ccIndex.ToEnum<CrowdControlType>();

            // 타겟 찾기
            var target = FindUnitEntity(targetCID);
            if (target is null)
                return;

            // 상태이상 적용
            target.battleCrowdControlInfo.Apply(ccType);
        }

        /// <summary>
        /// 특정 유저 도트딜 (모든 유저 수신)
        /// </summary>
        private void OnMazePlayerDotDamage(Response response)
        {
            int targetCID = response.GetInt("1");
            int dmg = response.GetInt("2");
            int leftHp = response.GetInt("3");

            // 타겟 찾기
            var target = FindUnitEntity(targetCID);
            if (target is null)
                return;

            // 감소된 체력 적용
            target.SetCurrentHp(leftHp);

            // 대미지 스펙 처리
            int dmgPerHit = dmg;
            int blowCount = 1;
            bool isBasicActiveSkill = false;
            ElementType elementType = ElementType.Neutral;
            bool isCritical = false;
            bool isRush = false;
            target.ApplyDamage(null, dmgPerHit, blowCount, isCritical, isBasicActiveSkill, elementType, totalDamage: dmg);

            if (target.CurHP != leftHp)
            {
                Debug.LogError($"[OnMazePlayerDamage] CID({targetCID}) 체력 재조정. {target.CurHP} -> {leftHp}");
                target.SetCurrentHp(leftHp);
            }
        }

        /// <summary>
        /// 특정 유저 체력 회복 (모든 유저 수신)
        /// </summary>
        private void OnMazePlayerHeal(Response response)
        {
            int targetCID = response.GetInt("1");
            int healValue = response.GetInt("2");
            int leftHp = response.GetInt("3");

            // 타겟 찾기
            var target = FindUnitEntity(targetCID);
            if (target is null)
                return;

            // 회복된 체력 적용
            target.SetCurrentHp(leftHp);
        }
        #endregion


        #region 넥서스(타워) 상호작용

        /// <summary>
        /// 타워 점령 시작 (양팀 모두 수신)
        /// </summary>
        private void OnTowerAttackStart(Response response)
        {
            byte towerTeamIndex = response.GetByte("1");
            long nextDamageTime = response.GetLong("2");
            long curTime = response.GetLong("3");

            NexusEntity nexusEntity = GetNexusEntity(towerTeamIndex);
            NexusActor nexusActor = nexusEntity?.GetActor() as NexusActor;
            if (nexusActor is null)
            {
                Debug.LogError($"[OnTowerAttackStart] {towerTeamIndex}팀의 타워가 없다. ({nexusEntity})");
                return;
            }

            // 타이머 출력
            long startDamageTime = nextDamageTime - TOWER_DAMAGE_TICK;
            long elapsedTick = curTime - startDamageTime;
            NexusEffectPlayer nexusEffectPlayer = nexusActor.EffectPlayer as NexusEffectPlayer;
            nexusEffectPlayer.ShowCircleTimer(TOWER_DAMAGE_TICK, elapsedTick);
        }

        /// <summary>
        /// 타워 점령 대미지 (양팀 모두 수신)
        /// </summary>
        /// <param name="response"></param>
        private void OnTowerAttackDamage(Response response)
        {
            byte towerTeamIndex = response.GetByte("1");
            long nextDamageTime = response.GetLong("2");
            int leftHp = response.GetInt("3");
            long curTime = response.GetLong("4");

            NexusEntity nexusEntity = GetNexusEntity(towerTeamIndex);
            NexusActor nexusActor = nexusEntity?.GetActor() as NexusActor;
            if (nexusActor is null)
            {
                Debug.LogError($"[OnTowerAttackDamage] {towerTeamIndex}팀의 타워가 없다. ({nexusEntity})");
                return;
            }

            // 타워 체력 적용
            nexusEntity.SetCurrentHp(leftHp);

            // 타이머 출력
            long startDamageTime = nextDamageTime - TOWER_DAMAGE_TICK;
            long elapsedTick = curTime - startDamageTime;
            NexusEffectPlayer nexusEffectPlayer = nexusActor.EffectPlayer as NexusEffectPlayer;
            nexusEffectPlayer.ShowCircleTimer(TOWER_DAMAGE_TICK, elapsedTick);
        }

        /// <summary>
        /// 타워 점령 중지 (양팀 모두 수신)
        /// </summary>
        /// <param name="response"></param>
        private void OnTowerAttackEnd(Response response)
        {
            byte towerTeamIndex = response.GetByte("1");

            NexusEntity nexusEntity = GetNexusEntity(towerTeamIndex);
            NexusActor nexusActor = nexusEntity?.GetActor() as NexusActor;
            if (nexusActor is null)
            {
                Debug.LogError($"[OnTowerAttackEnd] {towerTeamIndex}팀의 타워가 없다. ({nexusEntity})");
                return;
            }

            // 타이머 제거
            NexusEffectPlayer nexusEffectPlayer = nexusActor.EffectPlayer as NexusEffectPlayer;
            nexusEffectPlayer.HideCircleTimer();
        }

        /// <summary>
        /// 타워 파괴 (양팀 모두 수신)
        /// </summary>
        /// <param name="response"></param>
        private void OnTowerDestroy(Response response)
        {
            byte towerTeamIndex = response.GetByte("1");

            bool isMyTeam = (playerTeamIdx == towerTeamIndex);

            NexusEntity nexusEntity = GetNexusEntity(towerTeamIndex);
            NexusActor nexusActor = nexusEntity?.GetActor() as NexusActor;
            if (nexusActor is null)
            {
                Debug.LogError($"[OnTowerDestroy] {towerTeamIndex}팀의 타워가 없다. ({nexusEntity})");
                return;
            }

            nexusEntity.Die(null);

            // 타이머 제거
            NexusEffectPlayer nexusEffectPlayer = nexusActor.EffectPlayer as NexusEffectPlayer;
            nexusEffectPlayer.HideCircleTimer();

            string message = isMyTeam
                ? LocalizeKey._90149.ToText() // 우리팀의 수정이 파괴되었습니다.
                : LocalizeKey._90152.ToText(); // 상대팀의 수정이 파괴되었습니다.

            UI.ShowToastPopup(message);
        }

        #endregion

        #region 거석 상호작용

        /// <summary>
        /// 거석 생성 (준비 상태)
        /// </summary>
        void OnRockReady(Response response)
        {
            int rockIndex = response.GetInt("1");
            short xIndex = (short)response.GetInt("2");
            short zIndex = (short)response.GetInt("3");

            // 거석 스폰
            rockInfoList.Add(rockIndex, xIndex, zIndex, GuildMazeRockInfo.RockState.Ready);
        }

        /// <summary>
        /// 거석 낙하
        /// </summary>
        void OnRockAppear(Response response)
        {
            int rockIndex = response.GetInt("1");

            // 거석 상태 변경
            var rockInfo = rockInfoList.Get(rockIndex);
            rockInfo.SetState(GuildMazeRockInfo.RockState.Blocked);
        }

        /// <summary>
        /// 거석 제거
        /// </summary>
        void OnRockDisappear(Response response)
        {
            int rockIndex = response.GetInt("1");

            // 거석 제거
            rockInfoList.Release(rockIndex);
        }

        #endregion


        #region 아이템 상호작용

        /// <summary>
        /// 아이템 생성
        /// </summary>
        private void OnItemAppear(Response response)
        {
            int itemId = response.GetInt("1");
            short xIndex = (short)response.GetInt("2");
            short zIndex = (short)response.GetInt("3");

            itemInfoList.Add(itemId, xIndex, zIndex, GuildMazeItemInfo.ItemState.Idle);
        }

        /// <summary>
        /// 아이템 제거
        /// </summary>
        private void OnItemDisappear(Response response)
        {
            int itemId = response.GetInt("1");

            itemInfoList.Release(itemId);
        }

        /// <summary>
        /// 상태이상-수면 시작
        /// </summary>
        private void OnSleepStart(Response response)
        {
            int teamIndex = response.GetByte("1");

            bool isMyTeam = (playerTeamIdx == teamIndex);
            string message = isMyTeam
                ? LocalizeKey._90150.ToText() // 우리팀이 아이템에 의해 수면에 빠집니다.
                : LocalizeKey._90153.ToText(); // 상대팀이 아이템에 의해 수면에 빠집니다.
            UI.ShowToastPopup(message);

            // 해당하는 팀 전체 수면 (플레이어는 따로 처리)
            foreach (var multiPlayer in multiBattlePlayers)
            {
                if (!multiPlayerInputDic.ContainsKey(multiPlayer))
                {
                    Debug.LogError($"Input 정보가 없는 멀티플레이어가 있다. CID : {multiPlayer.Character.Cid}");
                    continue;
                }

                if (multiPlayerInputDic[multiPlayer].TeamIndex != teamIndex)
                    continue;

                if (multiPlayer.IsDie)
                    continue;

                var multiActor = multiPlayer.GetActor();
                multiActor.Animator.PlayDie(); // 사망 애니메이션 재생
            }

            // 플레이어 처리
            if (!isMyTeam || player.IsDie)
                return;

            var actor = player.GetActor();
            actor.AI.ChangeState(AI.Transition.Hold); // 수면 AI로 ChangeState
            actor.Animator.PlayDie(); // 수면 애니메이션

            // 수면상태임을 알려주는 UI Show
            cameraController.FlashCamera();
            cameraController.SetCameraBlur(true);

            isSleepState = true;
        }

        /// <summary>
        /// 상태이상-수면 종료
        /// </summary>
        private void OnSleepEnd(Response response)
        {
            int teamIndex = response.GetByte("1");

            // 해당하는 팀 전체 수면해제 (플레이어는 따로 처리)
            foreach (var multiPlayer in multiBattlePlayers)
            {
                if (!multiPlayerInputDic.ContainsKey(multiPlayer))
                {
                    Debug.LogError($"Input 정보가 없는 멀티플레이어가 있다. CID : {multiPlayer.Character.Cid}");
                    continue;
                }

                if (multiPlayerInputDic[multiPlayer].TeamIndex != teamIndex)
                    continue;

                if (multiPlayer.IsDie)
                    continue;

                var multiActor = multiPlayer.GetActor();
                multiActor.Animator.PlayIdle(); // Idle 애니메이션 재생
            }

            // 플레이어 처리
            bool isMyTeam = (playerTeamIdx == teamIndex);
            if (!isMyTeam || player.IsDie)
                return;

            PlayerSleepEnd();
        }

        /// <summary>
        /// 은신 시작
        /// </summary>
        private void OnInvisibleStart(Response response)
        {
            int CID = response.GetInt("1");
            bool isMe = IsMe(CID);

            // 내가 은신한 경우
            if (isMe)
            {
                // 은신 효과 Show
                cameraController.FlashCamera();
                cameraController.SetCameraGrayScale(true);

                // 공격불가 AI로 ChangeState
                var actor = player.GetActor();
                actor.AI.ChangeState(AI.Transition.Hold);

                isInvisibleState = true;
                return;
            }

            // 다른 유저가 은신한 경우
            var entity = FindUnitEntity(CID);
            if (entity is null)
                return;

            // 은신 이펙트 Show?

            // 해당 유저의 정보를 invisiblePlayerDic에 저장.
            if (!multiPlayerInputDic.ContainsKey(entity))
            {
                Debug.LogError($"{CID}의 멀티플레이어데이터가 없다.");
                return;
            }
            var input = multiPlayerInputDic[entity];
            invisiblePlayerDic[entity] = (entity.LastPosition, entity.CurHP, input);

            // 해당 유저 Die처리하고 Actor 즉시 제거
            entity.Die(null);
            entity.DespawnActor();
        }

        /// <summary>
        /// 은신 종료
        /// </summary>
        private void OnInvisibleEnd(Response response)
        {
            int CID = response.GetInt("1");
            bool isMe = IsMe(CID);

            // 내가 은신한 경우
            if (isMe)
            {
                PlayerInvisibleEnd();
                return;
            }

            // 다른 유저가 은신한 경우
            var entity = FindUnitEntity(CID, hasActor: false);
            if (entity is null)
                return;

            // invisiblePlayerDic에서 해당 유저 정보 꺼내와서 해당 Entity 즉시 부활
            if (!invisiblePlayerDic.ContainsKey(entity))
            {
                Debug.LogError($"은신한 적 없는 유저가 은신해제했다. {CID}");
                return;
            }

            Vector3 savedPos = invisiblePlayerDic[entity].pos;
            int savedHp = invisiblePlayerDic[entity].hp;
            IMultiPlayerInput savedInput = invisiblePlayerDic[entity].input;

            SpawnMultiPlayerActor(entity, savedPos, savedHp, savedInput);
            invisiblePlayerDic.Remove(entity);
        }

        /// <summary>
        /// 팀 전체 대미지 아이템 알림
        /// </summary>
        private void OnTeamDamaged(Response response)
        {
            int teamIndex = response.GetByte("1");

            bool isMyTeam = (playerTeamIdx == teamIndex);
            string message = isMyTeam
                ? LocalizeKey._90151.ToText() // 우리팀이 아이템에 의해 대미지를 입었습니다.
                : LocalizeKey._90154.ToText(); // 상대팀이 아이템에 의해 대미지를 입었습니다.
            UI.ShowToastPopup(message);
        }


        #endregion


        /// <summary>
        /// 플레이어의 수면 종료
        /// </summary>
        private void PlayerSleepEnd()
        {
            var actor = player.GetActor();
            actor.AI.ChangeState(AI.Transition.Finished); // 일반 AI로 ChangeState
            actor.Animator.PlayIdle(); // 일반 애니메이션

            // 수면상태임을 알려주는 UI Hide
            cameraController.FlashCamera();
            cameraController.SetCameraBlur(false);

            isSleepState = false;
        }

        /// <summary>
        /// 플레이어의 은신 종료
        /// </summary>
        private void PlayerInvisibleEnd()
        {
            // 은신 효과 Hide
            cameraController.FlashCamera();
            cameraController.SetCameraGrayScale(false);

            // 공격가능 AI로 ChangeState
            var actor = player.GetActor();
            actor.AI.ChangeState(AI.Transition.Finished);

            isInvisibleState = false;
        }

        /// <summary>
        /// 사망 팝업 확인버튼 이벤트
        /// </summary>
        private void OnBattleFailConfirm()
        {
            // Do Nothing
        }

        /// <summary>
        /// 모든 스킬 쿨타임 초기화
        /// </summary>
        private void ResetSkillCooldownTime()
        {
            player.ResetSkillCooldown();
        }

        /////////////////// Util /////////////////////
        private CharacterEntity FindUnitEntity(int CID, bool hasActor = true)
        {
            if (CID == SYSTEM_CID)
                return null;

            var entity = unitList.Find(e =>
            {
                CharacterEntity charEntity = e as CharacterEntity;
                return (charEntity != null && charEntity.Character.Cid == CID) && (!hasActor || charEntity.GetActor() != null);
            }) as CharacterEntity;

            if (entity is null)
            {
                var CIDList = from e in unitList.ToArray().OrEmptyIfNull()
                              let ch = e as CharacterEntity
                              where ch != null
                              select ch.Character.Cid;
                Debug.LogError($"CID({CID}) 유저를 찾지 못함. [{string.Join(", ", CIDList)}]");
                return null;
            }

            return entity;
        }

        private UnitEntity[] FindUnitEntities(int[] CIDs, bool hasActor = true)
        {
            if (CIDs == null)
                return null;

            var entities = unitList.FindAll(e =>
            {
                CharacterEntity entity = e as CharacterEntity;
                return entity != null && CIDs.Contains(entity.Character.Cid) && (!hasActor || entity.GetActor() != null);
            }).ToArray();

            if (entities.Length != CIDs.Length)
            {
                var CIDList = from e in unitList.ToArray()
                              let ch = e as CharacterEntity
                              where ch != null
                              select ch.Character.Cid;
                var findList = from e in entities
                               let ch = e as CharacterEntity
                               where ch != null
                               select ch.Character.Cid;

                var msg = StringBuilderPool.Get()
                    .AppendLine($"CID({string.Join(", ", CIDs)}) 유저를 전부 찾지 못함. [{string.Join(", ", CIDList)}]")
                    .AppendLine($"못 찾은 유저 : {string.Join(", ", CIDs.Except(findList))}")
                    .AppendLine($"찾은 유저 : {string.Join(", ", CIDs.Intersect(findList))}");

                Debug.LogError(msg.Release());
                return null;
            }

            return entities;
        }

        private Vector3 IntArrayToVector3(int[] pos)
        {
            return new Vector3(pos[0] / 1000f, pos[1] / 1000f, pos[2] / 1000f);
        }

        private int[] Vector3ToIntArray(Vector3 pos)
        {
            int[] posArray = { (int)(curLastPos.x * 1000), (int)(curLastPos.y * 1000), (int)(curLastPos.z * 1000) };
            return posArray;
        }

        private bool IsMe(int CID)
        {
            return (player.Character.Cid == CID);
        }

        /// <summary>
        /// 해당 팀의 넥서스 반환
        /// </summary>
        private NexusEntity GetNexusEntity(int teamIndex)
        {
            return unitList.Find(e => e is NexusEntity nexusEntity && nexusEntity.TeamIndex == teamIndex) as NexusEntity;
        }

        bool IEqualityComparer<UnitEntity>.Equals(UnitEntity x, UnitEntity y)
        {
            return ReferenceEquals(x, y);
        }

        int IEqualityComparer<UnitEntity>.GetHashCode(UnitEntity obj)
        {
            return obj.GetHashCode();
        }
    }
}