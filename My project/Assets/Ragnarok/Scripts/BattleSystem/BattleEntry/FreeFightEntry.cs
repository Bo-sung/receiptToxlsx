using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class FreeFightEntry : BattleEntry
    {
        private const string SCENE_NAME = "TempSpecialMaze";
        private const string BGM = "Comodo_field_01";

        private const float SPEED = 10f;
        private const int SYSTEM_CID = -1; // 시스템 Cid

        /// <summary>
        /// 위치가 1 이상 멀어졌을 때 서버로 통신
        /// </summary>
        private const float SEND_POSITION_MAGNITUDE = 1f;
        private const float SEND_POSITION_SQR_MAGNITUDE = SEND_POSITION_MAGNITUDE * SEND_POSITION_MAGNITUDE;

        private const float CELL_SIZE_X = 1.17f;
        private const float CELL_SIZE_Z = 1.17f;

        /// <summary>
        /// 10초 후 자동 참여
        /// </summary>
        private const long AUTO_JOIN_TIME = 10000L;

        /// <summary>
        /// 7초 뒤 부활
        /// </summary>
        private const int DEATH_COOL_TIME = 7;

        /// <summary>
        /// 시스템에 의한 대미지 표기
        /// </summary>
        private const int DAMAGE_BY_SYSTEM = 999999;

        /// <summary>
        /// 기본 속도 (이속, 공속)
        /// </summary>
        private const int DEFAULT_SPEED = 10000;

        private const CameraController.View DEFAULT_VIEW = CameraController.View.Quater50_10;

        private enum PlayerState : byte
        {
            Idle = 0,
            Attack = 1,
            Die = 2,
            PowerUp = 3,
            NoDamage = 4,
            Ready = 5, // 관전 상태
        }

        private enum TrapState : byte
        {
            None = 0,
            Ready = 1,
            Appear = 2,
            Appeared = 3,
        }

        private enum ItemState : byte
        {
            None = 0,
            Ready = 1,
            Appear = 2,
            Appeared = 3,
        }

        private enum RoundState : byte
        {
            Wait = 1, // 30초 쉬기
            Ready = 2, // 3초 대기
            Start = 3, // 10분 전투
            End = 4, // 3초 대기
        }

        // <!-- Models --!>
        private readonly SharingModel sharingModel;

        // <!-- Repositories --!>
        private readonly SkillDataManager skillDataRepo;
        private readonly int totalRound;
        private readonly FreeFightConfig config;

        // <!-- Scene Compositions --!>
        private readonly PlayerBotEntityPool playerBotPool;
        private readonly BattleTrapPool battleTrapPool;
        private readonly BattleItemPool battleItemPool;
        private readonly Buffer<int> intBuffer;

        // <!-- UIs --!>
        private UIMainTop uiMainTop;
        private UIBattleMenu uiBattleMenu;
        private UIController uiController;
        private UIBattlePlayerStatus uiBattlePlayerStatus;
        private UIBattleFail uiBattleFail;
        private UIBattleInfo uiBattleInfo;
        private UIBattleMazeSkillList uiBattleMazeSkillList;
        private UIBattleSkillList uiBattleSkillList;
        private UIFreeFightReady uiFreeFightReady;
        private UIBattleFreeFight uiBattleFreeFight;
        private UIKillCount uiKillCount;
        private UIFreeFightResult uiFreeFightResult;
        private UIBattleCountdown uiBattleCountdown;
        private UIBattleReady uiBattleReady;

        // <!-- Temp Data --!>
        private Map map;
        private Vector3 savedPlayerPosition;
        private Navigator navigator;
        private int round;
        private RoundState roundState;
        private long nextActionTime;
        private int killCount;
        private RemainTime remainJoinTime;

        public FreeFightEntry() : base(BattleMode.FreeFight)
        {
            player.SetState(UnitEntity.UnitState.GVG);

            sharingModel = player.Sharing;

            skillDataRepo = SkillDataManager.Instance;
            totalRound = BasisType.FF_ROUNT_COUNT.GetInt();
            config = FreeFightConfig.NORMAL;

            playerBotPool = new PlayerBotEntityPool();
            battleTrapPool = new BattleTrapPool(BattleTrapType.FreeFight);
            battleItemPool = new BattleItemPool();
            intBuffer = new Buffer<int>();
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            map = null;

            // 플레이어 봇 폐기
            foreach (var item in playerBotPool)
            {
                Dispose(item);
            }

            battleTrapPool.Recycle();

            playerBotPool.Clear();
            battleTrapPool.Clear();
            battleItemPool.Clear();

            NGUITools.Destroy(navigator); // 네비게이터 제거

            SetNextSingleAction(ShowFreeFightUI);
        }

        private void ShowFreeFightUI()
        {
            UI.Show<UIFreeFight>();
        }

        protected override void AddProtocalEvent()
        {
            base.AddProtocalEvent();
            Protocol.RECEIVE_FF_ROOM_JOIN.AddEvent(OnUserJoin);
            Protocol.RECEIVE_FF_ROOM_EXIT.AddEvent(OnUserExit);
            Protocol.REQUEST_FF_ROOM_TRANSFORM.AddEvent(OnUserTransform);
            Protocol.RECEIVE_FF_DAMAGE.AddEvent(OnUserDamage);
            Protocol.REQUEST_FF_ATTACK_MOT.AddEvent(OnUserAttackMotion);
            Protocol.RECEIVE_FF_DIE.AddEvent(OnUserDie);
            Protocol.RECEIVE_FF_USER_APPEAR.AddEvent(OnUserAppear);
            Protocol.RECEIVE_FF_NODAMAGE_START.AddEvent(OnUserNoDamageStart);
            Protocol.RECEIVE_FF_NODAMAGE_END.AddEvent(OnUserNoDamageEnd);
            Protocol.RECEIVE_FF_POWERUP_START.AddEvent(OnUserPowerUpStart);
            Protocol.RECEIVE_FF_POWERUP_END.AddEvent(OnUserPowerUpEnd);
            Protocol.RECEIVE_FF_ROCK_APPEAR_READY.AddEvent(OnTrapReady);
            Protocol.RECEIVE_FF_ROCK_APPEAR.AddEvent(OnTrapAppear);
            Protocol.RECEIVE_FF_ROCK_DISAPPEAR.AddEvent(OnTrapDisappear);
            Protocol.RECEIVE_FF_ITEM_APPEAR_READY.AddEvent(OnItemReady);
            Protocol.RECEIVE_FF_ITEM_APPEAR.AddEvent(OnItemAppear);
            Protocol.RECEIVE_FF_ITEM_DISAPPEAR.AddEvent(OnItemDisappear);
            Protocol.REQUEST_FF_ACTIVEBUFSKILL.AddEvent(OnUserBuff);
            Protocol.RECEIVE_FF_GETCROWDCONTROL.AddEvent(OnUserCrowdControl);
            Protocol.RECEIVE_FF_DOTDAMAGE.AddEvent(OnUserDotDamage);
            Protocol.RECEIVE_FF_PLUSHP.AddEvent(OnUserPlusHp);
            Protocol.RECEIVE_FF_NOTICEROUNDINFO.AddEvent(OnRoundNotice);
            Protocol.REQUEST_FF_FREEFIGHTCHAT.AddEvent(OnChatMessage);
        }

        protected override void RemoveProtocolEvent()
        {
            base.RemoveProtocolEvent();
            Protocol.RECEIVE_FF_ROOM_JOIN.RemoveEvent(OnUserJoin);
            Protocol.RECEIVE_FF_ROOM_EXIT.RemoveEvent(OnUserExit);
            Protocol.REQUEST_FF_ROOM_TRANSFORM.RemoveEvent(OnUserTransform);
            Protocol.RECEIVE_FF_DAMAGE.RemoveEvent(OnUserDamage);
            Protocol.REQUEST_FF_ATTACK_MOT.RemoveEvent(OnUserAttackMotion);
            Protocol.RECEIVE_FF_DIE.RemoveEvent(OnUserDie);
            Protocol.RECEIVE_FF_USER_APPEAR.RemoveEvent(OnUserAppear);
            Protocol.RECEIVE_FF_NODAMAGE_START.RemoveEvent(OnUserNoDamageStart);
            Protocol.RECEIVE_FF_NODAMAGE_END.RemoveEvent(OnUserNoDamageEnd);
            Protocol.RECEIVE_FF_POWERUP_START.RemoveEvent(OnUserPowerUpStart);
            Protocol.RECEIVE_FF_POWERUP_END.RemoveEvent(OnUserPowerUpEnd);
            Protocol.RECEIVE_FF_ROCK_APPEAR_READY.RemoveEvent(OnTrapReady);
            Protocol.RECEIVE_FF_ROCK_APPEAR.RemoveEvent(OnTrapAppear);
            Protocol.RECEIVE_FF_ROCK_DISAPPEAR.RemoveEvent(OnTrapDisappear);
            Protocol.RECEIVE_FF_ITEM_APPEAR_READY.RemoveEvent(OnItemReady);
            Protocol.RECEIVE_FF_ITEM_APPEAR.RemoveEvent(OnItemAppear);
            Protocol.RECEIVE_FF_ITEM_DISAPPEAR.RemoveEvent(OnItemDisappear);
            Protocol.REQUEST_FF_ACTIVEBUFSKILL.RemoveEvent(OnUserBuff);
            Protocol.RECEIVE_FF_GETCROWDCONTROL.RemoveEvent(OnUserCrowdControl);
            Protocol.RECEIVE_FF_DOTDAMAGE.RemoveEvent(OnUserDotDamage);
            Protocol.RECEIVE_FF_PLUSHP.RemoveEvent(OnUserPlusHp);
            Protocol.RECEIVE_FF_NOTICEROUNDINFO.RemoveEvent(OnRoundNotice);
            Protocol.REQUEST_FF_FREEFIGHTCHAT.RemoveEvent(OnChatMessage);
        }

        /// <summary>
        /// 이벤트 추가
        /// </summary>
        protected override void AddEvent()
        {
            base.AddEvent();

            player.OnUseSkill += RequestSkillMotion;
            player.OnApplySkill += RequestSkill;
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            player.OnUseSkill -= RequestSkillMotion;
            player.OnApplySkill -= RequestSkill;            
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
            uiMainTop = UI.Show<UIMainTop>();
            uiBattleMenu = UI.Show<UIBattleMenu>();
            uiController = UI.Show<UIController>();
            uiBattlePlayerStatus = UI.Show<UIBattlePlayerStatus>();
            uiBattleFail = UI.Show<UIBattleFail>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiBattleMazeSkillList = UI.Show<UIBattleMazeSkillList>();
            uiBattleSkillList = UI.Show<UIBattleSkillList>();
            uiFreeFightReady = UI.Show<UIFreeFightReady>();
            uiBattleFreeFight = UI.Show<UIBattleFreeFight>();
            uiKillCount = UI.Show<UIKillCount>();
            uiFreeFightResult = UI.Show<UIFreeFightResult>();
            uiBattleCountdown = UI.Show<UIBattleCountdown>();
            uiBattleReady = UI.Show<UIBattleReady>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiFreeFightReady.OnMove += MoveNavigation;
            uiFreeFightReady.OnEnter += RequestStart;
            uiBattleMenu.OnExit += Finish;
            uiBattleMazeSkillList.OnSelect += OnSelectSkill;
            uiBattleMazeSkillList.OnToggleSkill += OnToggleSkill;
            uiBattleSkillList.OnSelect += OnSelectSkill;
            uiBattleSkillList.OnToggleSkill += OnToggleSkill;
            uiFreeFightResult.OnAutoHide += OnClosedResult;

            // Initialize
            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);
            uiBattleFail.Hide();
            uiBattleInfo.SetMode(UIBattleInfo.Mode.TYPE_1);
            uiBattlePlayerStatus.SetPosition(UIBattlePlayerStatus.Position.TopLeft_Big);
            uiBattleMazeSkillList.Hide();
            uiBattleFreeFight.SetFreeFightEventType(FreeFightEventType.Normal);
            uiBattleFreeFight.Hide();
            uiKillCount.Hide();
            uiFreeFightResult.Hide();
            uiBattleCountdown.Hide();
            uiBattleReady.Hide();

            cameraController.HideCustomView();
            cameraController.SetView(DEFAULT_VIEW);
            cameraController.AddMask(Layer.MAZE_OTHER_PLAYER, Layer.MAZE_ENEMY);
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
            uiFreeFightReady.OnMove -= MoveNavigation;
            uiFreeFightReady.OnEnter -= RequestStart;
            uiBattleMenu.OnExit -= Finish;
            uiBattleMazeSkillList.OnSelect -= OnSelectSkill;
            uiBattleMazeSkillList.OnToggleSkill -= OnToggleSkill;
            uiBattleSkillList.OnSelect -= OnSelectSkill;
            uiBattleSkillList.OnToggleSkill -= OnToggleSkill;
            uiFreeFightResult.OnAutoHide -= OnClosedResult;

            UI.Close<UIMainTop>();
            UI.Close<UIBattleMenu>();
            UI.Close<UIController>();
            UI.Close<UIBattlePlayerStatus>();
            UI.Close<UIBattleFail>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIBattleMazeSkillList>();
            UI.Close<UIBattleSkillList>();
            UI.Close<UIFreeFightReady>();
            UI.Close<UIBattleFreeFight>();
            UI.Close<UIKillCount>();
            UI.Close<UIFreeFightResult>();
            UI.Close<UIBattleCountdown>();
            UI.Close<UIBattleReady>();

            uiMainTop = null;
            uiBattleMenu = null;
            uiController = null;
            uiBattlePlayerStatus = null;
            uiBattleFail = null;
            uiBattleInfo = null;
            uiBattleMazeSkillList = null;
            uiBattleSkillList = null;
            uiBattleFreeFight = null;
            uiKillCount = null;
            uiFreeFightResult = null;
            uiBattleCountdown = null;
            uiBattleReady = null;
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
            uiBattleInfo.Set(config.NameId.ToText());
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        public override void EndBattle()
        {
            base.EndBattle();

            player.ResetExtraOption(ExtraBattleOptionType.MaxHp); // MaxHp 옵션 제거
            player.ResetExtraOption(ExtraBattleOptionType.AttackSpeed);
            player.ResetExtraOption(ExtraBattleOptionType.MoveSpeed);
            player.SetIgnoreTarget(false); // 무시타겟 설정 제거

            cameraController.SetConfine(CameraController.View.NaviQuater50, null); // 제한 해제
        }

        /// <summary>
        /// 나가기 요청
        /// </summary>
        public override IEnumerator<float> YieldExitBattle()
        {
            // 난전에서는 서버 재접속이 일어나므로 굳이 셰어캐릭터 공유 재연결을 하지 않는다.
            //sharingModel.ReuseShareCharacters(); // 셰어캐릭터 공유 재연결

            Task<Response> task = Protocol.REQUEST_FF_ROOM_EXIT.SendAsync();
            yield return Timing.WaitUntilTrue(task.IsComplete);
            Response response = task.Result;
            IsSuccessExit = response.isSuccess;

            if (!IsSuccessExit)
                response.ShowResultCode();
        }

        /// <summary>
        /// 진입 요청
        /// </summary>
        public override IEnumerator<float> YieldEnterBattle()
        {
            return YieldEnterBattle(0);
        }

        /// <summary>
        /// 특정 id로 진입 요청
        /// </summary>
        public override IEnumerator<float> YieldEnterBattle(int id)
        {
            Task<Response> task = Protocol.REQUEST_FF_ROOM_JOIN.SendAsync();
            yield return Timing.WaitUntilTrue(task.IsComplete);

            Response response = task.Result;
            IsSuccessEnter = response.isSuccess;

            if (!IsSuccessEnter)
            {
                response.ShowResultCode();
                yield break;
            }

            Quest.QuestProgress(QuestType.FREE_FIGHT_ENTER_COUNT, FreeFightEventType.Normal.ToIntValue()); // 특정 난전 도전 횟수

            Task pauseShareTask = sharingModel.RequestPauseShareCharacter(); // 셰어캐릭터 공유 일시정지
            yield return Timing.WaitUntilTrue(pauseShareTask.IsComplete);

            int joinChannel = response.GetInt("1");
            IMultiPlayerInput[] characterPackets = response.ContainsKey("2") ? response.GetPacketArray<SpecialDungeonCharacterPacket>("2") : System.Array.Empty<SpecialDungeonCharacterPacket>();
            FreeFightTrapPacket[] trpaPackets = response.ContainsKey("3") ? response.GetPacketArray<FreeFightTrapPacket>("3") : System.Array.Empty<FreeFightTrapPacket>();
            FreeFightItemPacket[] itemPackets = response.ContainsKey("4") ? response.GetPacketArray<FreeFightItemPacket>("4") : System.Array.Empty<FreeFightItemPacket>();
            long currentTime = response.GetLong("5");
            byte round = response.GetByte("6");
            byte roundState = response.GetByte("7");
            long nextActionTime = response.GetLong("8");
            int killCount = response.GetInt("9");

            playerBotPool.EnqueueRange(characterPackets);
            battleTrapPool.EnqueueRange(trpaPackets);
            battleItemPool.EnqueueRange(itemPackets);
            ServerTime.Initialize(currentTime); // 서버 시간 세팅
            remainJoinTime = AUTO_JOIN_TIME;
            SetRound(round, roundState, nextActionTime);
            SetKillCount(killCount);
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            base.Ready();

            string sceneName = SCENE_NAME; // 씬 이름
            string bgmName = BGM; // 배경음 이름
            bool isChangeScene = true; // 씬 변경 여부
            SceneLoader.SceneType sceneType = SceneLoader.SceneType.Dungeon;

            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비
        }

        /// <summary>
        /// 씬 로드 완료 후 호출
        /// </summary>
        protected override void OnSceneLoaded(GameObject[] roots)
        {
            foreach (var item in roots)
            {
                map = item.GetComponent<Map>();

                if (map)
                    break;
            }
        }

        /// <summary>
        /// 전투 준비 완료
        /// </summary>
        protected override void OnReady()
        {
            base.OnReady();

            uiBattlePlayerStatus.SetPlayer(player);
            uiBattleMazeSkillList.SetCharacter(player);
            uiBattleSkillList.SetCharacter(player);
            player.ResetSkillCooldown();

            RefreshRound();
            RefreshKillCount();
        }

        /// <summary>
        /// 씬 구성
        /// </summary>
        protected override IEnumerator<float> YieldComposeScene()
        {
            CreateNavigator();
            SetReadyMode();
            yield break;
        }

        /// <summary>
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            // 체력 리젠 중지
            foreach (var unit in unitList)
            {
                UnitActor actor = unit.GetActor();
                if (actor == null)
                    continue;

                actor.AI.StopHpRegen();
            }

            Timing.RunCoroutine(YieldSpawnPlayerBots(), TAG);
            Timing.RunCoroutine(YieldSpawnTrap(), TAG);
            Timing.RunCoroutine(YieldSpawnItem(), TAG);
        }

        /// <summary>
        /// 유닛 죽음 처리
        /// </summary>
        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
            if (impl.IsPlayerDead()) // 플레이어가 죽었는지 확인
            {
                uiBattleMazeSkillList.SetCharacter(player);
                uiBattleSkillList.SetCharacter(player);
            }
        }

        /// <summary>
        /// 스킬 모션 서버 요청
        /// </summary>
        private void RequestSkillMotion(UnitEntity target, SkillInfo skillInfo)
        {
            if (target is CharacterEntity characterEntity)
            {
                var sfs = Protocol.NewInstance();
                sfs.PutInt("1", player.Character.Cid); // 공격자 아이디
                sfs.PutInt("2", characterEntity.Character.Cid); // 피격자 아이디
                sfs.PutInt("3", skillInfo.SkillId); // 스킬 아이디
                Protocol.REQUEST_FF_ATTACK_MOT.SendAsync(sfs).WrapNetworkErrors();
            }
        }

        /// <summary>
        /// 스킬 서버 요청
        /// </summary>
        private void RequestSkill(UnitEntity[] targets, SkillInfo skillInfo)
        {
            foreach (var item in targets)
            {
                if (item is CharacterEntity characterEntity)
                {
                    intBuffer.Add(characterEntity.Character.Cid);
                }
            }

            int[] targetCids = intBuffer.GetBuffer(isAutoRelease: true);
            if (targetCids.Length == 0)
                return;

            var sfs = Protocol.NewInstance();
            switch (skillInfo.ActiveSkillType)
            {
                case ActiveSkill.Type.Attack:
                    sfs.PutLong("2", skillInfo.SkillId);
                    sfs.PutIntArray("3", targetCids);
                    sfs.PutLong("4", skillInfo.SlotNo);

                    if (skillInfo.SlotNo > 0)
                    {
                        sfs.PutInt("5", 0); // 정상적인 스킬
                    }
                    else
                    {
                        sfs.PutInt("5", (int)skillInfo.RefBattleOption); // 참조되어 나간 스킬
                    }

                    Protocol.REQUEST_FF_ATTACK.SendAsync(sfs).WrapNetworkErrors();
                    break;

                case ActiveSkill.Type.RecoveryHp:
                case ActiveSkill.Type.Buff:
                    sfs.PutIntArray("3", targetCids);
                    sfs.PutLong("4", skillInfo.SlotNo);
                    Protocol.REQUEST_FF_ACTIVEBUFSKILL.SendAsync(sfs).WrapNetworkErrors();
                    break;

                default:
                    throw new System.InvalidOperationException($"유효하지 않은 처리: {nameof(ActiveSkill.Type)} = {skillInfo.ActiveSkillType}");
            }
        }

        /// <summary>
        /// 전투 시작 요청
        /// </summary>
        private void RequestStart()
        {
            Protocol.REQUEST_FF_USER_APPEAR.SendAsync().WrapNetworkErrors();
        }

        /// <summary>
        /// 현재 위치 서버 요청 (돌격 및 넉백에 한함)
        /// </summary>
        private void RequestTransformWithRushOrKnockBack()
        {
            RequestTransform(isRushOrKnockBack: true);
        }

        /// <summary>
        /// 현재 위치 서버 요청
        /// </summary>
        private void RequestTransform(bool isRushOrKnockBack)
        {
            savedPlayerPosition = player.LastPosition;

            int[] posArray = { (int)savedPlayerPosition.x * 1000, (int)savedPlayerPosition.y * 1000, (int)savedPlayerPosition.z * 1000 };
            var sfs = Protocol.NewInstance();
            sfs.PutIntArray("2", posArray);

            if (isRushOrKnockBack)
            {
                Protocol.REQUEST_FF_ROOM_TRANSFORMEX.SendAsync(sfs).WrapNetworkErrors();
            }
            else
            {
                Protocol.REQUEST_FF_ROOM_TRANSFORM.SendAsync(sfs).WrapNetworkErrors();
            }
        }

        /// <summary>
        /// 채팅 메시지 서버 요청
        /// </summary>
        private void RequestChatMessage(string message)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutUtfString("1", message);
            Protocol.REQUEST_FF_FREEFIGHTCHAT.SendAsync(sfs).WrapNetworkErrors();
        }

        /// <summary>
        /// 게임 종료
        /// </summary>
        private void Finish()
        {
            impl.ExitBattle();
        }

        /// <summary>
        /// 네비게이션 움직이기
        /// </summary>
        private void MoveNavigation(Vector2 offset)
        {
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
                return;

            Vector3 motion = mainCamera.transform.TransformDirection(offset);
            motion.y = 0;

            navigator.Warp(motion);
        }

        private void CreateNavigator()
        {
            if (navigator == null)
            {
                navigator = new GameObject("Navigator").AddComponent<Navigator>();
                navigator.SetScale(Vector3.one * 0.05f);
                navigator.SetHome(map.GetPlayerPosition());
            }
        }

        private void SetReadyMode()
        {
            if (navigator == null)
            {
                Debug.LogError("navigator is Null");
                return;
            }

            uiFreeFightReady.Show();
            uiFreeFightReady.SetAutoEnter(remainJoinTime); // 자동 참여 시간 세팅
            uiController.Hide();
            uiBattleMenu.Hide();

            UnitActor actor = player.GetActor();
            if (actor)
            {
                //actor.AI.ChangeState(AI.Transition.Hold); // 은신 상태로 변경
                actor.Movement.Stop(); // 움직임 중지
                actor.Hide(); // Hide 상태
            }

            cameraController.SetView(CameraController.View.NaviQuater50);
            cameraController.SetPlayer(navigator.Target); // 카메라 타겟 세팅
        }

        private void SetPlayMode()
        {
            UnitActor actor = player.GetActor();
            if (actor == null)
            {
                Debug.LogError("actor is Null");
                return;
            }

            uiBattleFail.Hide();

            uiFreeFightReady.Hide();
            uiController.Show();
            uiBattleMenu.Show();
            uiBattleMenu.SetMode(UIBattleMenu.MenuContent.Exit); // 나가기

            actor.AI.ChangeState(AI.Transition.Finished); // 은신 상태 종료
            actor.Show(); // Show 상태

            cameraController.HideCustomView();
            cameraController.SetView(DEFAULT_VIEW);
            cameraController.SetPlayer(actor.CachedTransform); // 카메라 타겟 세팅
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private bool SpawnPlayer(Vector3 pos, int maxHp)
        {
            uiBattleMazeSkillList.SetCharacter(player);
            uiBattleSkillList.SetCharacter(player);

            UnitActor unitActor;
            bool isSpawnActor = impl.Add(player, isEnemy: false);
            if (isSpawnActor)
            {
                player.SetExtraOption(ExtraBattleOptionType.MaxHp, maxHp); // MaxHp 맞추기
                player.SetExtraOption(ExtraBattleOptionType.AttackSpeed, DEFAULT_SPEED);
                player.SetExtraOption(ExtraBattleOptionType.MoveSpeed, DEFAULT_SPEED);

                player.OnSpawnActor += OnCharacterSpawn;
                player.OnDespawnActor += OnCharacterDespawn;

                unitActor = player.SpawnActor(); // 유닛 소환
                unitActor.Movement.SetDefaultSpeed(SPEED);
                unitActor.EffectPlayer.ShowUnitCircle();

                CharacterEffectPlayer characterEffectPlayer = unitActor.EffectPlayer as CharacterEffectPlayer;
                characterEffectPlayer.ShowControllerAssist(uiController);

                cameraController.SetPlayer(unitActor.CachedTransform); // 카메라 타겟 세팅
            }
            else
            {
                unitActor = player.GetActor();
            }

            player.SetIgnoreTarget(IsIgnoreTargetSettings()); // 타겟 무시 여부
            savedPlayerPosition = FindClosestEdge(pos); // 씬 로드 후에 네비 위치로 변경
            unitActor.AI.SetHomePosition(savedPlayerPosition, isWarp: false); // Home 세팅
            unitActor.Movement.ForceWarp(savedPlayerPosition); // Home으로 강제 워프
            unitActor.AI.ReadyToBattle(); // 플레이어 봇은 나중에 생성하므로 생성과 동시에 ReadyToBattle 호출
            unitActor.AI.StopHpRegen(); // 체력 리젠 중지

            player.SetCurrentHp(player.MaxHP);
            player.SetCurrentMp(player.MaxMp);

            if (isSpawnActor)
            {
                // Do Nothing
            }
            else
            {
                uiBattleMazeSkillList.SetCharacter(player);
                uiBattleSkillList.SetCharacter(player);
                player.ResetSkillCooldown();
            }

            return isSpawnActor;
        }

        /// <summary>
        /// 플레이어 봇 생성
        /// </summary>
        private bool CreatePlayerBot(IMultiPlayerInput input)
        {
            PlayerBotEntity entity = playerBotPool.Create(input); // 일단 생성
            entity.SetState(UnitEntity.UnitState.GVG);
            PlayerState state = input.State.ToEnum<PlayerState>();
            if (state == PlayerState.Ready)
                return false;

            Vector3 pos = new Vector3(input.PosX, 0f, input.PosZ);
            int maxHp = input.MaxHp;
            bool hasCurHp = input.HasCurHp;
            int curHp = input.CurHp;
            return SpawnPlayerBot(entity, pos, maxHp, hasCurHp, curHp, state);
        }

        /// <summary>
        /// 플레이어 봇 생성
        /// </summary>
        private bool SpawnPlayerBot(PlayerBotEntity entity, Vector3 pos, int maxHp, bool hasCurHp, int curHp, PlayerState state)
        {
            UnitActor unitActor;
            bool isSpawnActor = impl.Add(entity, isEnemy: true);
            if (isSpawnActor)
            {
                entity.SetExtraOption(ExtraBattleOptionType.MaxHp, maxHp);
                entity.SetExtraOption(ExtraBattleOptionType.AttackSpeed, DEFAULT_SPEED);
                entity.SetExtraOption(ExtraBattleOptionType.MoveSpeed, DEFAULT_SPEED);

                unitActor = entity.SpawnActor(); // 유닛 소환
                unitActor.Movement.SetDefaultSpeed(SPEED);
                unitActor.AI.ReadyToBattle(); // 플레이어 봇은 나중에 생성하므로 생성과 동시에 ReadyToBattle 호출
                unitActor.AI.StopHpRegen(); // 체력 리젠 중지
            }
            else
            {
                unitActor = entity.GetActor();
                unitActor.AI.ChangeState(AI.Transition.Finished); // 기존 상태(Die) 종료
            }

            entity.SetIgnoreTarget(IsIgnoreTargetSettings()); // 타겟 무시 여부
            Vector3 position = FindClosestEdge(pos);
            unitActor.AI.SetHomePosition(position, isWarp: false); // Home 세팅
            unitActor.Movement.ForceWarp(position); // Home으로 강제 워프

            // 현재 Hp 세팅
            if (hasCurHp)
                entity.SetCurrentHp(curHp);

            return isSpawnActor;
        }

        /// <summary>
        /// 트랩 생성
        /// </summary>
        private bool CreateTrap(IBattleTrapInput input)
        {
            BattleTrap trap = battleTrapPool.Create(input); // 일단 생성
            TrapState state = input.State.ToEnum<TrapState>();
            if (state == TrapState.None)
                return false;

            return SpawnTrap(trap, GetPosition(input.IndexX, input.IndexZ), state);
        }

        /// <summary>
        /// 트랩 생성
        /// </summary>
        private bool SpawnTrap(BattleTrap trap, Vector3 pos, TrapState state)
        {
            Vector3 position = FindClosestEdge(pos); // 씬 로드 후에 네비 위치로 변경
            trap.SetPosition(position);

            switch (state)
            {
                case TrapState.Ready:
                    trap.ShowReady();
                    break;

                case TrapState.Appear:
                case TrapState.Appeared:
                    trap.Appear();
                    break;
            }

            return true;
        }

        /// <summary>
        /// 아이템 생성
        /// </summary>
        private bool CreateItem(IBattleItemInput input)
        {
            BattleItem item = battleItemPool.Create(input); // 일단 생성
            ItemState state = input.State.ToEnum<ItemState>();
            if (state == ItemState.None)
                return false;

            return SpawnItem(item, GetPosition(input.IndexX, input.IndexZ), state);
        }

        /// <summary>
        /// 아이템 생성
        /// </summary>
        private bool SpawnItem(BattleItem item, Vector3 pos, ItemState state)
        {
            Vector3 position = FindClosestEdge(pos); // 씬 로드 후에 네비 위치로 변경
            item.SetPosition(position);

            switch (state)
            {
                case ItemState.Ready:
                    item.ShowReady();
                    break;

                case ItemState.Appear:
                case ItemState.Appeared:
                    item.Appear();
                    break;
            }

            return true;
        }

        /// <summary>
        /// 위치 세팅
        /// </summary>
        private Vector3 GetPosition(short indexX, short indexZ)
        {
            return new Vector3(indexX * CELL_SIZE_X, 0f, indexZ * CELL_SIZE_Z);
        }

        /// <summary>
        /// 라운드 정보
        /// </summary>
        private void SetRound(byte round, byte roundState, long nextActionTime)
        {
            this.round = round;
            this.roundState = roundState.ToEnum<RoundState>();
            this.nextActionTime = nextActionTime;
        }

        /// <summary>
        /// 라운드 정보 업데이트
        /// </summary>
        private void RefreshRound()
        {
            UpdateUnitListIgnoreSettings(); // 적군 세팅 새로고침

            // 종료 State 의 경우에 체력 최대
            if (roundState == RoundState.End)
                UpdateMaxHpSettings();

            if (!IsInitCanvas)
                return;

            System.TimeSpan timeSpan = nextActionTime.ToDateTime() - ServerTime.Now;
            // (백그라운드에 다녀올 경우(BackgroundOnPause) nextActionTime 값이 Now 보다 더 작을 수 있으므로
            // 0보다 작지 않은 값으로 방어코드를 넣음
            float milliseconds = Mathf.Max(0f, (float)timeSpan.TotalMilliseconds);

            uiBattleFreeFight.Show();
            uiBattleFreeFight.SetRound(round);

            switch (roundState)
            {
                case RoundState.Wait:
                    uiBattleFreeFight.Initialize(milliseconds);
                    uiBattleFreeFight.RestartTimer();

                    string notice = LocalizeKey._40011.ToText() // 잠시 후 {INDEX} 라운드가 시작됩니다.
                        .Replace(ReplaceKey.INDEX, round);
                    uiBattleReady.ShowNotice(notice);
                    break;

                case RoundState.Ready:
                    uiBattleFreeFight.Initialize(0f);
                    uiBattleFreeFight.RestartTimer();

                    uiBattleCountdown.Show(milliseconds);
                    uiBattleReady.Hide();
                    break;

                case RoundState.Start:
                    uiBattleFreeFight.Initialize(milliseconds);
                    uiBattleFreeFight.RestartTimer();
                    break;

                case RoundState.End:
                    uiBattleFreeFight.Initialize(0f);
                    uiBattleFreeFight.RestartTimer();

                    uiFreeFightResult.Show(FreeFightEventType.Normal, killCount, round, milliseconds);
                    break;
            }
        }

        /// <summary>
        /// 새로고침: 타겟 무시
        /// </summary>
        private void UpdateUnitListIgnoreSettings()
        {
            bool isIgnoreTargetSettings = IsIgnoreTargetSettings();
            foreach (var item in unitList)
            {
                item.SetIgnoreTarget(isIgnoreTargetSettings);
            }
        }

        /// <summary>
        /// 새로고침: 체력 최대 및 상태이상 제거
        /// </summary>
        private void UpdateMaxHpSettings()
        {
            foreach (var item in unitList)
            {
                item.ReadyToBattle(); // 준비 상태로 변경
            }
        }

        /// <summary>
        /// 타겟 무시 여부
        /// </summary>
        private bool IsIgnoreTargetSettings()
        {
            return roundState != RoundState.Start;
        }

        /// <summary>
        /// 처치 횟수
        /// </summary>
        private void SetKillCount(int killCount)
        {
            this.killCount = killCount;
        }

        /// <summary>
        /// 처치 횟수 업데이트
        /// </summary>
        private void RefreshKillCount()
        {
            uiKillCount.Show();
            uiKillCount.SetKillCount(killCount);

            uiBattleFreeFight.Show();
            uiBattleFreeFight.SetKillCount(killCount);
        }

        /// <summary>
        /// 플레이어 퇴장
        /// </summary>
        private void UpdatePlayerExit(int cid)
        {
            bool isPlayer = cid == player.Character.Cid;
            if (isPlayer)
            {
                // Do Nothing
                return;
            }

            PlayerBotEntity find = playerBotPool.Find(cid);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueLeave(cid))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 나감: {nameof(cid)} = {cid}");
#endif
                return;
            }

            UnitActor actor = find.GetActor();
            if (actor)
                actor.EffectPlayer.SpawnUnitTeleport();

            find.ResetExtraOption(ExtraBattleOptionType.MaxHp); // MaxHp 옵션 제거
            find.ResetExtraOption(ExtraBattleOptionType.AttackSpeed);
            find.ResetExtraOption(ExtraBattleOptionType.MoveSpeed);
            find.SetIgnoreTarget(false); // 무시타겟 설정 제거

            Dispose(find); // 제거
            playerBotPool.Recycle(find); // 회수
        }

        /// <summary>
        /// 플레이어 움직임
        /// </summary>
        private void UpdatePlayerMove(int cid, Vector3 position)
        {
            bool isPlayer = cid == player.Character.Cid;
            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueMove(cid, position))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 움직임: {nameof(cid)} = {cid}");
#endif
                return;
            }

            UnitActor unitActor = find.GetActor();
            if (unitActor == null)
                return;

            if (isPlayer)
            {
                unitActor.Movement.ForceWarp(position);
            }
            else
            {
                unitActor.Movement.ForceSetDestination(position);
            }
        }

        /// <summary>
        /// 플레이어 상태 - 출현
        /// </summary>
        private void UpdatePlayerAppear(int cid, Vector3 position, int maxHp)
        {
            bool isPlayer = cid == player.Character.Cid;
            if (isPlayer)
            {
                SpawnPlayer(position, maxHp); // 플레이어 생성
                SetPlayMode(); // 숨김모드 해제
            }
            else
            {
                PlayerBotEntity find = playerBotPool.Find(cid);
                if (find == null)
                {
                    const byte IDLE_STATE = (byte)PlayerState.Idle;

                    // 생성 도중이라면 생성큐 업데이트
                    if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueState(cid, IDLE_STATE))
                        return;

#if UNITY_EDITOR
                    Debug.LogError($"존재하지 않는 플레이어 출현: {nameof(cid)} = {cid}");
#endif
                    return;
                }

                SpawnPlayerBot(find, position, maxHp, hasCurHp: true, curHp: maxHp, PlayerState.Idle);
            }
        }

        /// <summary>
        /// 플레이어 대미지
        /// </summary>
        private void UpdatePlayerDamage(int cid, int damage, bool isCritical, int remainHp, int attackerCid, int attackerSkillId)
        {
            bool isPlayer = cid == player.Character.Cid;
            bool isPlayerAttack = attackerCid == player.Character.Cid;
            bool isSystemDamage = attackerCid == SYSTEM_CID; // 시스템에 의한 대미지

            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueHp(cid, remainHp))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 대미지: {nameof(cid)} = {cid}");
#endif

                return;
            }

            UnitEntity attacker;
            if (isSystemDamage)
            {
                attacker = null;
            }
            else
            {
                attacker = isPlayerAttack ? player : playerBotPool.Find(attackerCid);

                if (attacker == null)
                {
                    // 생성 도중
                    if (playerBotPool.HasQueue())
                    {
                        // 생성 도중에는 공격자가 존재하지 않을 수도 있음
                    }
                    else
                    {
#if UNITY_EDITOR
                        Debug.LogError($"존재하지 않는 플레이어가 공격함: {nameof(attackerCid)} = {attackerCid}");
#endif
                    }
                }
            }

            SkillInfo skillInfo = isSystemDamage ? null : FindSkill(attacker, attackerSkillId);

            int dmgPerHit;
            int blowCount;
            bool isBasicActiveSkill;
            ElementType elementType;
            bool isCriticalHit;
            bool isRush;
            if (skillInfo == null)
            {
                dmgPerHit = isSystemDamage ? DAMAGE_BY_SYSTEM : damage;
                blowCount = 1;
                isBasicActiveSkill = false;
                elementType = ElementType.Neutral;
                isCriticalHit = isSystemDamage ? true : isCritical;
                isRush = false;
            }
            else
            {
                dmgPerHit = damage == 0 ? 0 : Mathf.Max(1, damage / skillInfo.BlowCount);
                blowCount = skillInfo.BlowCount;
                isBasicActiveSkill = skillInfo.IsBasicActiveSkill;
                elementType = skillInfo.ElementType;
                isCriticalHit = isCritical;
                isRush = skillInfo.IsRush;
            }

            find.ApplyDamage(attacker, dmgPerHit, blowCount, isCriticalHit, isBasicActiveSkill, elementType, damage);

            if (attacker)
            {
                // 넉백 처리
                if (isRush)
                {
                    Vector3 dir = (find.LastPosition - attacker.LastPosition).normalized;
                    UnitActor actor = find.GetActor();
                    if (actor)
                    {
                        actor.Movement.KnockBack(dir, Constants.Battle.RushKnockBackPower);
                    }
                }
            }

            if (find.CurHP != remainHp)
            {
                Debug.Log($"체력 재조정: {nameof(cid)} = {cid}, {find.CurHP} -> {remainHp}");
                find.SetCurrentHp(remainHp); // 체력 재조정
            }

            // 공격자가 플레이어일 경우에 연계 공격 처리
            if (isPlayerAttack)
            {
                if (skillInfo == null || skillInfo.IsChainedSkill())
                {
                    // 잘못된 스킬 또는 이미 연계 공격인 상태
                }
                else
                {
                    if (find.IsDie || find.GetActor() == null)
                    {
                        // 피격자가 죽거나 피격자가 음슴
                    }
                    else
                    {
                        UnitActor attackerActor = attacker.GetActor();
                        if (attackerActor)
                        {
                            ElementType skillElementType = (skillInfo.IsBasicActiveSkill && skillInfo.ElementType == default) ? attacker.battleItemInfo.WeaponElementType : skillInfo.ElementType;
                            attackerActor.UseChainAttackSkill(find, skillElementType, skillInfo.SkillId); // 연계 스킬 사용
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 플레이어 스킬 모션
        /// </summary>
        private void UpdatePlayerSkillMotion(int cid, int skillId, int targetCid)
        {
            bool isPlayer = cid == player.Character.Cid;
            if (isPlayer)
            {
                Debug.LogError("잘못된 처리: 본인의 스킬 모션 진행"); // 플레이어의 모션은 오지 않지만, 혹시 모르니 막는다.
                return;
            }

            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue())
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 스킬모션 진행: {nameof(cid)} = {cid}");
#endif
                return;
            }

            UnitActor actor = find.GetActor();
            if (actor == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"공격자의 Actor가 존재하지 않음: {nameof(cid)} = {cid}");
#endif
                return;
            }

            bool isPlayerTarget = targetCid == player.Character.Cid;
            UnitEntity target = isPlayerTarget ? player : playerBotPool.Find(targetCid);
            if (target == null)
            {
                // 생성 도중
                if (playerBotPool.HasQueue())
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어에게 공격함: {nameof(targetCid)} = {targetCid}");
#endif
                return;
            }

            UnitActor targetActor = target.GetActor();
            if (targetActor == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"타겟의 Actor가 존재하지 않음: {nameof(targetCid)} = {targetCid}");
#endif
                return;
            }

            SkillInfo skillInfo = FindSkill(find, skillId);
            if (skillInfo == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 스킬: {nameof(skillId)} = {skillId}");
#endif
            }
            else
            {
                actor.UseSkill(targetActor, skillInfo, isChainableSkill: false, queueIdleMotion: true); // 스킬 사용
            }
        }

        /// <summary>
        /// 플레이어 사망
        /// </summary>
        private void UpdatePlayerDie(int cid, int damage, bool isCritical, int attackerCid, int attackerSkillId, int attackerKillCount)
        {
            bool isPlayer = cid == player.Character.Cid;
            bool isPlayerAttack = attackerCid == player.Character.Cid;
            bool isSystemDamage = attackerCid == SYSTEM_CID; // 시스템에 의한 대미지

            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
                const byte DIE_STATE = (byte)PlayerState.Die;

                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueState(cid, DIE_STATE))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 죽음: {nameof(cid)} = {cid}");
#endif
                return;
            }

            UnitEntity attacker;
            if (isSystemDamage)
            {
                attacker = null;
            }
            else
            {
                attacker = isPlayerAttack ? player : playerBotPool.Find(attackerCid);

                if (attacker == null)
                {
                    // 생성 도중
                    if (playerBotPool.HasQueue())
                    {
                        // 생성 도중에는 공격자가 존재하지 않을 수도 있음
                    }
                    else
                    {
#if UNITY_EDITOR
                        Debug.LogError($"존재하지 않는 플레이어가 공격함: {nameof(attackerCid)} = {attackerCid}");
#endif
                    }
                }
            }

            SkillInfo skillInfo = isSystemDamage ? null : FindSkill(attacker, attackerSkillId);

            int dmgPerHit;
            int blowCount;
            bool isBasicActiveSkill;
            ElementType elementType;
            bool isCriticalHit;
            bool isRush;
            if (skillInfo == null)
            {
                dmgPerHit = isSystemDamage ? DAMAGE_BY_SYSTEM : damage;
                blowCount = 1;
                isBasicActiveSkill = false;
                elementType = ElementType.Neutral;
                isCriticalHit = isSystemDamage ? true : isCritical;
                isRush = false;
            }
            else
            {
                dmgPerHit = damage == 0 ? 0 : Mathf.Max(1, damage / skillInfo.BlowCount);
                blowCount = skillInfo.BlowCount;
                isBasicActiveSkill = skillInfo.IsBasicActiveSkill;
                elementType = skillInfo.ElementType;
                isCriticalHit = isCritical;
                isRush = skillInfo.IsRush;
            }

            find.ApplyDamage(attacker, dmgPerHit, blowCount, isCriticalHit, isBasicActiveSkill, elementType, damage);
            find.Die(null); // 강제 사망처리

            // 플레이어가 사망
            if (isPlayer)
            {
                uiBattleFail.Show(UIBattleFail.ConfirmType.Rebirth, DEATH_COOL_TIME, isShowButton: false);
            }

            // 공격자가 죽였을 경우
            if (isPlayerAttack)
            {
                SetKillCount(attackerKillCount);
                RefreshKillCount();
            }
        }

        /// <summary>
        /// 플레이어 버프 적용
        /// </summary>
        private void UpdatePlayerApplyBuff(int cid, int skillId, int[] targetCids, long slotId)
        {
            if (targetCids == null)
                return;

            bool isPlayer = cid == player.Character.Cid;
            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            SkillInfo skillInfo = FindSkill(find, skillId);
            if (skillInfo == null)
                return;

            foreach (var targetCid in targetCids)
            {
                bool isTargetPlayer = targetCid == player.Character.Cid;
                UnitEntity target = isTargetPlayer ? player : playerBotPool.Find(targetCid);
                if (target == null)
                {
                    // 생성 도중
                    if (playerBotPool.HasQueue())
                        continue;

#if UNITY_EDITOR
                    Debug.LogError($"존재하지 않는 플레이어가 버프 타겟팅 됨: {nameof(targetCid)} = {targetCid}");
#endif
                    continue;
                }

                target.AddBattleBuff(skillInfo); // 버프 적용
            }
        }

        /// <summary>
        /// 플레이어 상태이상 적용
        /// </summary>
        private void UpdatePlayerApplyCrowdControl(int cid, CrowdControlType type)
        {
            bool isPlayer = cid == player.Character.Cid;
            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
                // 생성 도중
                if (battleItemPool.HasQueue())
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 상태이상 걸림: {nameof(cid)} = {cid}");
#endif
                return;
            }

            find.battleCrowdControlInfo.Apply(type); // 상태이상 적용
        }

        /// <summary>
        /// 플레이어 도트대미지
        /// </summary>
        private void UpdatePlayerDotDamage(int cid, int damage, int remainHp)
        {
            bool isPlayer = cid == player.Character.Cid;
            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueHp(cid, remainHp))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 도트대미지: {nameof(cid)} = {cid}");
#endif
                return;
            }

            find.SetCurrentHp(remainHp); // 감소된 체력 적용

            // 대미지 스펙 처리
            int blowCount = 1;
            bool isBasicActiveSkill = false;
            ElementType elementType = ElementType.Neutral;
            bool isCritical = false;
            find.ApplyDamage(null, damage, blowCount, isCritical, isBasicActiveSkill, elementType, damage);

            if (find.CurHP != remainHp)
            {
                Debug.Log($"체력 재조정: {nameof(cid)} = {cid}, {find.CurHP} -> {remainHp}");
                find.SetCurrentHp(remainHp); // 체력 재조정
            }
        }

        /// <summary>
        /// 플레이어 회복
        /// </summary>
        private void UpdatePlayerHeal(int cid, int plusHp, int remainHp)
        {
            bool isPlayer = cid == player.Character.Cid;
            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueHp(cid, remainHp))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 회복: {nameof(cid)} = {cid}");
#endif
                return;
            }

            find.SetCurrentHp(remainHp); // 회복된 체력 적용

            UnitActor actor = find.GetActor();
            if (actor == null)
                return;

            actor.EffectPlayer.ShowHealEffect();
        }

        /// <summary>
        /// 플레이어 무적 상태 시작
        /// </summary>
        private void UpdatePlyaerNoDamageStart()
        {
            // 현재 미사용
        }

        /// <summary>
        /// 플레이어 무적 상태 종료
        /// </summary>
        private void UpdatePlayerNoDamageEnd(int cid)
        {
            Debug.Log($"{nameof(UpdatePlayerNoDamageEnd)}: {nameof(cid)} = {cid}");
        }

        /// <summary>
        /// 플레이어 파워업 상태 시작
        /// </summary>
        private void UpdatePlayerPowerUpStart(int cid)
        {
            bool isPlayer = cid == player.Character.Cid;
            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
                const byte POWER_UP_STATE = (byte)PlayerState.PowerUp;

                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueState(cid, POWER_UP_STATE))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 파워업: {nameof(cid)} = {cid}");
#endif
                return;
            }

            UnitActor unitActor = find.GetActor();
            if (unitActor == null)
                return;

            unitActor.EffectPlayer.ShowPowerUpEffect();
        }

        /// <summary>
        /// 플레이어 파워업 상태 종료
        /// </summary>
        private void UpdatePlayerPowerUpEnd(int cid)
        {
            bool isPlayer = cid == player.Character.Cid;
            UnitEntity find = isPlayer ? player : playerBotPool.Find(cid);
            if (find == null)
            {
                const byte IDLE_STATE = (byte)PlayerState.Idle;

                // 생성 도중이라면 생성큐 업데이트
                if (playerBotPool.HasQueue() && playerBotPool.UpdateQueueState(cid, IDLE_STATE))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 플레이어가 파워업 종료: {nameof(cid)} = {cid}");
#endif
                return;
            }

            UnitActor unitActor = find.GetActor();
            if (unitActor == null)
                return;

            unitActor.EffectPlayer.ReleasePowerUpEffect();
        }

        /// <summary>
        /// 트랩 경고
        /// </summary>
        private void UpdateTrapReady(int trapId, short indexX, short indexZ)
        {
            BattleTrap find = battleTrapPool.Find(trapId);
            if (find == null)
            {
                const byte READY_STATE = (byte)TrapState.Ready;
                IBattleTrapInput trapInput = new FreeFightTrapPacket(trapId, indexX, indexZ, READY_STATE);

                // 생성 도중
                if (battleTrapPool.HasQueue())
                {
                    // 생성큐 업데이트
                    if (battleTrapPool.UpdateQueueState(trapId, READY_STATE))
                        return;

                    battleTrapPool.Enqueue(trapInput);
                    return;
                }

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 트랩 등장: {nameof(trapId)} = {trapId}");
#endif
                // 즉시 생성
                if (CreateTrap(trapInput))
                    return;
            }

            SpawnTrap(find, GetPosition(indexX, indexZ), TrapState.Ready);
        }

        /// <summary>
        /// 트랩 출현
        /// </summary>
        private void UpdateTrapAppear(int id)
        {
            BattleTrap find = battleTrapPool.Find(id);
            if (find == null)
            {
                const byte APPEAR_STATE = (byte)TrapState.Appear;

                // 생성 도중이라면 생성큐 업데이트
                if (battleTrapPool.HasQueue() && battleTrapPool.UpdateQueueState(id, APPEAR_STATE))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 트랩 출현: {nameof(id)} = {id}");
#endif
                return;
            }

            find.Appear(); // 트랩 출현
        }

        /// <summary>
        /// 트랩 사라짐
        /// </summary>
        private void UpdateTrapDisappear(int id)
        {
            BattleTrap find = battleTrapPool.Find(id);
            if (find == null)
            {
                const byte NONE_STATE = (byte)TrapState.None;

                // 생성 도중이라면 생성큐 업데이트
                if (battleTrapPool.HasQueue() && battleTrapPool.UpdateQueueState(id, NONE_STATE))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 트랩 사라짐: {nameof(id)} = {id}");
#endif
                return;
            }

            find.Disappear(); // 트랩 사라짐
        }

        /// <summary>
        /// 아이템 준비
        /// </summary>
        private void UpdateItemReady(int itemId, short indexX, short indexZ)
        {
            BattleItem find = battleItemPool.Find(itemId);
            if (find == null)
            {
                const byte READY_STATE = (byte)ItemState.Ready;
                IBattleItemInput itemInput = new FreeFightItemPacket(itemId, indexX, indexZ, READY_STATE);

                // 생성 도중
                if (battleItemPool.HasQueue())
                {
                    // 생성큐 업데이트
                    if (battleItemPool.UpdateQueueState(itemId, READY_STATE))
                        return;

                    battleItemPool.Enqueue(itemInput);
                    return;
                }

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 아이템 등장: {nameof(itemId)} = {itemId}");
#endif
                // 즉시 생성
                if (CreateItem(itemInput))
                    return;
            }

            SpawnItem(find, GetPosition(indexX, indexZ), ItemState.Ready);
        }

        /// <summary>
        /// 아이템 출현
        /// </summary>
        private void UpdateItemAppear(int id)
        {
            BattleItem find = battleItemPool.Find(id);
            if (find == null)
            {
                const byte APPEAR_STATE = (byte)ItemState.Appear;

                // 생성 도중이라면 생성큐 업데이트
                if (battleItemPool.HasQueue() && battleItemPool.UpdateQueueState(id, APPEAR_STATE))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 아이템 출현: {nameof(id)} = {id}");
#endif
                return;
            }

            find.Appear(); // 아이템 출현
        }

        /// <summary>
        /// 아이템 사라짐
        /// </summary>
        private void UpdateItemDisappear(int id)
        {
            BattleItem find = battleItemPool.Find(id);
            if (find == null)
            {
                const byte NONE_STATE = (byte)TrapState.None;

                // 생성 도중이라면 생성큐 업데이트
                if (battleItemPool.HasQueue() && battleItemPool.UpdateQueueState(id, NONE_STATE))
                    return;

#if UNITY_EDITOR
                Debug.LogError($"존재하지 않는 아이템 사라짐: {nameof(id)} = {id}");
#endif
                return;
            }

            find.Disappear(); // 아이템 사라짐
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
        /// 플레이어 봇 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnPlayerBots()
        {
            while (playerBotPool.HasQueue())
            {
                IMultiPlayerInput input = playerBotPool.Dequeue();
                if (CreatePlayerBot(input))
                    yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 트랩 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnTrap()
        {
            while (battleTrapPool.HasQueue())
            {
                IBattleTrapInput input = battleTrapPool.Dequeue();
                if (CreateTrap(input))
                    yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 아이템 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnItem()
        {
            while (battleItemPool.HasQueue())
            {
                IBattleItemInput input = battleItemPool.Dequeue();
                if (CreateItem(input))
                    yield return Timing.WaitForOneFrame;
            }
        }

        void OnStartController()
        {
            if (player.IsDie)
                return;

            UnitActor unitActor = player.GetActor();
            if (unitActor == null)
                return;

            CameraUtils.InvokePlayerTrackingEffect();

            unitActor.Movement.Stop();
            unitActor.AI.SetInputMove(isControl: true);
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

            UnitActor unitActor = player.GetActor();
            if (unitActor == null)
                return;

            Vector3 motion = mainCamera.transform.TransformDirection(position);
            motion.y = 0;
            motion.Normalize();
            unitActor.Movement.Move(motion);

            Vector3 dist = player.LastPosition - savedPlayerPosition;
            if (dist.sqrMagnitude < SEND_POSITION_SQR_MAGNITUDE)
                return;

            RequestTransform(isRushOrKnockBack: false); // 플레이어 위치 동기화
        }

        void OnResetController()
        {
            CameraUtils.Zoom(CameraZoomType.None);

            if (player.IsDie)
                return;

            UnitActor unitActor = player.GetActor();
            if (unitActor == null)
                return;

            unitActor.AI.SetInputMove(isControl: false);
            unitActor.Movement.Stop();

            RequestTransform(isRushOrKnockBack: false); // 플레이어 위치 동기화
        }

        /// <summary>
        /// 스킬 선택 사용
        /// </summary>
        void OnSelectSkill(SkillInfo info, UIBattleNormalSkillSlot.SlotType slotType)
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

        void OnToggleSkill()
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

        void OnClosedResult()
        {
            // 모든 라운드 종료
            if (round == totalRound)
                Finish(); // 게임 종료
        }

        void OnCharacterSpawn(UnitActor actor)
        {
            actor.Movement.OnRushEnd += RequestTransformWithRushOrKnockBack;
            actor.Movement.OnKnockBackEnd += RequestTransformWithRushOrKnockBack;
        }

        void OnCharacterDespawn(UnitActor actor)
        {
            actor.Movement.OnRushEnd -= RequestTransformWithRushOrKnockBack;
            actor.Movement.OnKnockBackEnd -= RequestTransformWithRushOrKnockBack;

            actor.Entity.OnSpawnActor -= OnCharacterSpawn;
            actor.Entity.OnDespawnActor -= OnCharacterDespawn;
        }

        void OnUserJoin(Response response)
        {
            IMultiPlayerInput characterPacket = response.GetPacket<SpecialDungeonCharacterPacket>("1");
            CreatePlayerBot(characterPacket);
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

        void OnUserDamage(Response response)
        {
            int cid = response.GetInt("1");
            int damage = response.GetInt("2"); // 0: 미스
            bool isCritical = response.GetByte("3") == 1; // 0:일반, 1:크리
            int remainHp = response.GetInt("4");
            int attackerCid = response.GetInt("5"); // -1일경우 아이템에 의한 대미지
            int attackerSkillId = response.GetInt("6");
            UpdatePlayerDamage(cid, damage, isCritical, remainHp, attackerCid, attackerSkillId); // 플레이어 대미지
        }

        void OnUserAttackMotion(Response response)
        {
            int cid = response.GetInt("1");
            int targetCid = response.GetInt("2");
            int skillId = response.GetInt("3");
            UpdatePlayerSkillMotion(cid, skillId, targetCid);
        }

        void OnUserDie(Response response)
        {
            int cid = response.GetInt("1");
            int damage = response.GetInt("2"); // 0: 미스
            bool isCritical = response.GetByte("3") == 1; // 0:일반, 1:크리
            int attackerCid = response.GetInt("4"); // -1일경우 거석에 의한 대미지
            int attackerSkillId = response.GetInt("5");
            int attackerKillCount = response.GetInt("6");
            UpdatePlayerDie(cid, damage, isCritical, attackerCid, attackerSkillId, attackerKillCount); // 플레이어 죽음
        }

        void OnUserAppear(Response response)
        {
            int cid = response.GetInt("1");
            int[] arrayPosValue = response.GetIntArray("2");
            int maxHp = response.GetInt("3");

            // 방어코드
            Vector3 position = arrayPosValue == null ? Vector3.zero : new Vector3(arrayPosValue[0] * 0.001f, arrayPosValue[1] * 0.001f, arrayPosValue[2] * 0.001f);
            UpdatePlayerAppear(cid, position, maxHp);
        }

        void OnUserNoDamageStart(Response response)
        {
            UpdatePlyaerNoDamageStart(); // 현재 미사용
        }

        void OnUserNoDamageEnd(Response response)
        {
            int cid = response.GetInt("1");
            UpdatePlayerNoDamageEnd(cid);
        }

        void OnUserPowerUpStart(Response response)
        {
            int cid = response.GetInt("1");
            UpdatePlayerPowerUpStart(cid);
        }

        void OnUserPowerUpEnd(Response response)
        {
            int cid = response.GetInt("1");
            UpdatePlayerPowerUpEnd(cid);
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

        void OnItemReady(Response response)
        {
            int itemId = response.GetShort("1");
            short indexX = response.GetShort("2");
            short indexZ = response.GetShort("3");
            UpdateItemReady(itemId, indexX, indexZ);
        }

        void OnItemAppear(Response response)
        {
            int itemId = response.GetShort("1");
            UpdateItemAppear(itemId);
        }

        void OnItemDisappear(Response response)
        {
            int itemId = response.GetShort("1");
            UpdateItemDisappear(itemId);
        }

        void OnUserBuff(Response response)
        {
            int cid = response.GetInt("1");
            int skillId = response.GetInt("2");
            int[] targetCids = response.GetIntArray("3");
            long slotId = response.GetLong("4"); // 사용하지 않음
            UpdatePlayerApplyBuff(cid, skillId, targetCids, slotId);
        }

        void OnUserCrowdControl(Response response)
        {
            int cid = response.GetInt("1");
            CrowdControlType type = response.GetByte("2").ToEnum<CrowdControlType>();
            UpdatePlayerApplyCrowdControl(cid, type);
        }

        void OnUserDotDamage(Response response)
        {
            int cid = response.GetInt("1");
            int damage = response.GetInt("2");
            int remainHp = response.GetInt("3");
            UpdatePlayerDotDamage(cid, damage, remainHp);
        }

        void OnUserPlusHp(Response response)
        {
            int cid = response.GetInt("1");
            int plusHp = response.GetInt("2");
            int remainHp = response.GetInt("3");
            UpdatePlayerHeal(cid, plusHp, remainHp);
        }

        void OnRoundNotice(Response response)
        {
            byte round = response.GetByte("1");
            byte roundState = response.GetByte("2");
            long nextActionTime = response.GetLong("3");

            // 라운드가 변경되었을 경우에 KillCount 초기화
            if (this.round != round)
            {
                SetKillCount(0);
                RefreshKillCount();
            }

            SetRound(round, roundState, nextActionTime);
            RefreshRound();
        }

        void OnChatMessage(Response response)
        {
            int cid = response.GetInt("1");
            string name = response.GetUtfString("2");
            string message = response.GetUtfString("3");
            byte job = response.GetByte("4");
            short jobLevel = response.GetShort("5");
            byte gender = response.GetByte("6");
        }
    }
}