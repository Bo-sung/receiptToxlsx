using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class EndlessTowerEntry : BattleEntry
    {
        public const string SCENE_NAME = "Scene_Special_EndlessTower";
        private const string BGM = "Endless_tower";

        private const float POSITION_DISTANCE = 3f;
        private const CameraController.View DEFAULT_VIEW = CameraController.View.Quater40_2_5;

        // <!-- Models --!>
        private readonly DungeonModel dungeonModel;
        private readonly SharingModel sharingModel;
        private readonly AgentModel agentModel;
        private readonly CharacterModel characterModel;
        private readonly StatusModel statusModel;

        // <!-- Repositories --!>
        private readonly EndlessTowerDataManager endlessTowerDataRepo;
        private readonly JobDataManager jobDataRepo;
        private readonly StatDataManager statDataRepo;
        private readonly MonsterDataManager monsterDataRepo;
        private readonly SoundManager soundManager;
        private readonly int maxFloor;

        // <!-- Scene Compositions --!>
        private readonly Buffer<UnitEntity> unitBuffers;
        private readonly GhostBattlePlayers agentCharacterFactory;
        private readonly Buffer<AgentCharacterPacket> agentPacketBuffer;
        private readonly BetterList<CharacterEntity> characterList;
        private readonly BetterList<int> characterNextHpList;
        private readonly BetterList<int> characterNextMpList;

        // <!-- UIs --!>
        private UIController uiController;
        private UIBossComing uiBossComing;
        private UIBattleInfo uiBattleInfo;
        private UIBattleBossHp uiBattleBossHp;
        private UIBattlePause uiBattlePause;
        private UIBattleEndlessTower uiBattleEndlessTower;
        private UIBattleGuildAttackSkillList uiBattleGuildAttackSkillList;
        private UIBattleAgentList uiBattleAgentList;
        private UIResultClear uiResultClear;
        private UICamSelect uiCamSelect;

        // <!-- Temp Data --!>
        private Map map;
        private EndlessTowerData currentData;
        private long endTime;
        private byte killCount;
        private CharacterEntity currentPlayer; // 조종 중인 캐릭터
        private UnitEntity bossMonster; // 보스 몬스터
        private bool isFollowBinding;
        private bool isRequestClear; //클리어 후 서버에 요청 보냈는지 여부
        private bool isRequestExit;
        private bool IsResponseClear; //클리어 후 서버 응답 받은지 여부
        private bool isOnPortal; // 포털 진입 여부
        private bool needInitialize = true;
        private int nextFloor;

        public EndlessTowerEntry() : base(BattleMode.EndlessTower)
        {
            player.SetState(UnitEntity.UnitState.Stage);

            dungeonModel = Entity.player.Dungeon;
            sharingModel = Entity.player.Sharing;
            agentModel = Entity.player.Agent;
            characterModel = Entity.player.Character;
            statusModel = Entity.player.Status;

            endlessTowerDataRepo = EndlessTowerDataManager.Instance;
            jobDataRepo = JobDataManager.Instance;
            statDataRepo = StatDataManager.Instance;
            monsterDataRepo = MonsterDataManager.Instance;
            soundManager = SoundManager.Instance;
            maxFloor = BasisType.ENDLESS_TOWER_MAX_FLOOR.GetInt();

            unitBuffers = new Buffer<UnitEntity>();
            agentCharacterFactory = new GhostBattlePlayers();
            agentPacketBuffer = new Buffer<AgentCharacterPacket>();
            characterList = new BetterList<CharacterEntity>();
            characterNextHpList = new BetterList<int>();
            characterNextMpList = new BetterList<int>();
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            map = null;
            agentCharacterFactory.Clear();
            agentPacketBuffer.Clear();
            characterList.Clear();
            characterNextHpList.Clear();
            characterNextMpList.Clear();

            currentPlayer = null;
            bossMonster = null;

            SetNextSingleAction(ShowDungeonUI);
        }

        private void ShowDungeonUI()
        {
            UIDungeon.viewType = UIDungeon.ViewType.EndlessTower;
            UI.Show<UIDungeon>();
        }

        protected override void AddProtocalEvent()
        {
            base.AddProtocalEvent();

            Protocol.ENDLESS_DUNGEON_STORYCLEAR.AddEvent(OnEndlessTowerClear);
        }

        protected override void RemoveProtocolEvent()
        {
            base.RemoveProtocolEvent();

            Protocol.ENDLESS_DUNGEON_STORYCLEAR.RemoveEvent(OnEndlessTowerClear);
        }

        /// <summary>
        /// 이벤트 추가
        /// </summary>
        protected override void AddEvent()
        {
            base.AddEvent();

            dungeonModel.OnEndlessTowerExit += OnEndlessTowerExit;
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            dungeonModel.OnEndlessTowerExit -= OnEndlessTowerExit;
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
            uiController = UI.Show<UIController>();
            uiBossComing = UI.Show<UIBossComing>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiBattleBossHp = UI.Show<UIBattleBossHp>();
            uiBattlePause = UI.Show<UIBattlePause>();
            uiBattleEndlessTower = UI.Show<UIBattleEndlessTower>();
            uiBattleGuildAttackSkillList = UI.Show<UIBattleGuildAttackSkillList>();
            uiBattleAgentList = UI.Show<UIBattleAgentList>();
            uiResultClear = UI.Show<UIResultClear>();
            uiCamSelect = UI.Show<UICamSelect>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiBattleEndlessTower.OnFinish += Finish;
            uiBattlePause.OnPause += GiveUp;
            uiBattleGuildAttackSkillList.OnSelect += OnSelectSkill;
            uiBattleAgentList.OnSelectAgent += OnSelectAgent;
            uiBattleAgentList.OnSelectMoveBind += SetFollowBindingTarget;
            uiBattleAgentList.OnSelectSkillAuto += SetAutoSkill;
            uiResultClear.OnFinishDungeon += ExitEntry;

            uiBattleBossHp.SetOffset(UIBattleBossHp.Offset.BossMonster);
            uiBattleBossHp.SetMode(UIBattleBossHp.Mode.Default);
            uiBattleBossHp.Hide();
            uiResultClear.Hide();
            uiCamSelect.Hide(); // 단순 CameraView 세팅 후에 Hide

            cameraController.SetView(DEFAULT_VIEW);
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
            uiBattleEndlessTower.OnFinish -= Finish;
            uiBattlePause.OnPause -= GiveUp;
            uiBattleGuildAttackSkillList.OnSelect -= OnSelectSkill;
            uiBattleAgentList.OnSelectAgent -= OnSelectAgent;
            uiBattleAgentList.OnSelectMoveBind -= SetFollowBindingTarget;
            uiBattleAgentList.OnSelectSkillAuto -= SetAutoSkill;
            uiResultClear.OnFinishDungeon -= ExitEntry;

            uiBattleBossHp.SetOffset();

            UI.Close<UIController>();
            UI.Close<UIBossComing>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIBattleBossHp>();
            UI.Close<UIBattlePause>();
            UI.Close<UIBattleEndlessTower>();
            UI.Close<UIBattleGuildAttackSkillList>();
            UI.Close<UIBattleAgentList>();
            UI.Close<UIResultClear>();
            UI.Close<UICamSelect>();

            uiController = null;
            uiBossComing = null;
            uiBattleInfo = null;
            uiBattleBossHp = null;
            uiBattlePause = null;
            uiBattleEndlessTower = null;
            uiBattleGuildAttackSkillList = null;
            uiBattleAgentList = null;
            uiResultClear = null;
            uiCamSelect = null;
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
            uiBattleInfo.Set(DungeonType.EnlessTower.ToText());
        }

        /// <summary>
        /// 뒤로가기
        /// </summary>
        public override void OnBack()
        {
            GiveUp();
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        public override void EndBattle()
        {
            base.EndBattle();

            isRequestClear = false;
            isRequestExit = false;
            isOnPortal = false;
            IsResponseClear = false;
            killCount = 0;
            nextFloor = 0;
        }

        /// <summary>
        /// 다음 층 시작 시 호출
        /// </summary>
        public override IEnumerator<float> YieldEnterBattle()
        {
            IsSuccessEnter = currentData != null; // 진행중인 층 존재
            if (!IsSuccessEnter)
                yield break;
        }

        /// <summary>
        /// 어둠의 재 사용으로 진입 요청
        /// </summary>
        public override IEnumerator<float> YieldEnterBattle(int skipItemCount)
        {
            Task<Response> task = dungeonModel.RequestEndlessTowerStart(skipItemCount);
            yield return Timing.WaitUntilTrue(task.IsComplete);
            Response response = task.Result;
            IsSuccessEnter = response.isSuccess;
            if (!IsSuccessEnter)
                yield break;

            Analytics.TrackEvent(TrackType.EndlessTower);

            Task pauseShareTask = sharingModel.RequestPauseShareCharacter(); // 셰어캐릭터 공유 일시정지
            yield return Timing.WaitUntilTrue(pauseShareTask.IsComplete);

            int floor = response.GetInt("1"); // 시작 층
            long currentTime = response.GetLong("2"); // 현재시간
            endTime = response.GetLong("3"); // 종료시간

            ServerTime.Initialize(currentTime); // 서버 시간 세팅

            IsSuccessEnter = SetFloor(floor);
            if (!IsSuccessEnter)
                yield break;

            SetAgents();

            characterList.Add(player);
            for (int i = 0; i < agentCharacterFactory.size; i++)
            {
                characterList.Add(agentCharacterFactory[i]);
            }
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            base.Ready();

            string sceneName = SCENE_NAME; // 씬 이름
            //string bgmName = currentData.IsBossFloor() ? BOSS_BGM : BGM; // 배경음 이름
            string bgmName = BGM; // 배경음 이름
            bool isChangeScene = map == null; // 씬 변경 여부 - 씬이 다를 경우
            SceneLoader.SceneType sceneType = SceneLoader.SceneType.Dungeon;
            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비
        }

        /// <summary>
        /// 씬 로드 완료 후 호출
        /// </summary>
        protected override void OnSceneLoaded(GameObject[] roots)
        {
            foreach (GameObject item in roots)
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

            if (needInitialize)
            {
                // (백그라운드에 다녀올 경우(BackgroundOnPause) endTime 값이 Now 보다 더 작을 수 있으므로
                // 0보다 작지 않은 값으로 방어코드를 넣음
                System.TimeSpan timeSpan = endTime.ToDateTime() - ServerTime.Now;
                float milliseconds = Mathf.Max(0f, (float)timeSpan.TotalMilliseconds);
                uiBattleEndlessTower.Initialize(milliseconds);
                uiBattleEndlessTower.RestartTimer();
            }

            uiBattleEndlessTower.SetFloor(currentData.GetFloor());

            // Map Light 세팅
            map.SetEndlessTowerLight(currentData.IsBossFloor() ? EndlessTowerMap.LightType.Boss : EndlessTowerMap.LightType.Normal);
        }

        /// <summary>
        /// 씬 구성
        /// </summary>
        protected override IEnumerator<float> YieldComposeScene()
        {
            if (map == null)
            {
                Debug.LogError($"맵(Map)이 존재하지 않습니다: {nameof(sceneName)} = {sceneName}");
                yield break;
            }

            // 초기화 필요
            if (needInitialize)
            {
                yield return Timing.WaitUntilDone(YieldSpawnCharacters(), TAG); // 캐릭터 생성
            }
            else
            {
                // 죽은 캐릭터 UnitList 해제
                for (int i = 0; i < characterList.size; i++)
                {
                    if (characterList[i].IsDie)
                    {
                        Dispose(characterList[i]);
                    }
                }
            }

            ResetCharacterPosition(); // 캐릭터 위치 세팅

            yield return Timing.WaitUntilDone(YieldSpawnMonster(), TAG); // 몬스터 생성

            // 보스 생성
            if (currentData.IsBossFloor())
                SpawnBossMonster();
        }

        /// <summary>
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            // 초기화 필요
            if (needInitialize)
            {
                SelectCharacter(player); // 기본으로 플레이어 선택

                uiBattleAgentList.SetCharacters(characterList.ToArray());
                SetFollowBindingTarget(uiBattleAgentList.IsFollowBinding);
                SetAutoSkill(uiBattleAgentList.IsAutoSkill);
            }
            else
            {
                // Hp 및 Mp 세팅
                for (int i = 0; i < characterList.size; i++)
                {
                    if (characterList[i].IsDie)
                        continue;

                    characterList[i].SetCurrentHp(characterNextHpList[i]);
                    characterList[i].SetCurrentMp(characterNextMpList[i]);
                }
                characterNextHpList.Clear();
                characterNextMpList.Clear();
            }

            // 보스 층
            if (currentData.IsBossFloor())
            {
                uiBossComing.ShowBoss(currentData);
                uiBattleBossHp.SetHp(bossMonster.CurHP, bossMonster.MaxHP);
            }

            RefreshFollowBindingTarget(); // 바인딩 타겟 새로고침
            RefreshBossTargetingArrow(); // 보스 타겟 화살표 새로고침

            needInitialize = false;
        }

        /// <summary>
        /// 유닛 죽음 처리
        /// </summary>
        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
            if (unitEntity is CharacterEntity characterEntity)
            {
                // 죽은 캐릭터가 선택한 캐릭터의 경우
                if (currentPlayer.Equals(characterEntity))
                {
                    SelectNextCharcater(characterEntity); // 다음 캐릭터 선택
                }
            }
            else if (unitEntity.type == UnitEntityType.NormalMonster)
            {
                if (currentData.IsBossFloor())
                {
                    // Do Nothing
                }
                else
                {
                    ++killCount;
                    UI.RewardToast(currentData.GetNormalRewards(), isExceptGoods: false);
                }
            }
            else if (unitEntity.type == UnitEntityType.BossMonster)
            {
                impl.ResetEnemyUnitList(); // 적군 캐릭터 제거
            }

            if (impl.IsAllEnemyDead()) // 모든 적이 죽었는지 확인
            {
                ShowPortal();
            }
            else if (impl.IsAllAlliesDead())
            {
                Finish();
            }
        }

        /// <summary>
        /// 엔들리스 타워 해당 층 클리어
        /// </summary>
        void OnEndlessTowerClear(Response response)
        {
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            IsResponseClear = true;
            nextFloor = response.GetInt("1");
            Quest.QuestProgress(QuestType.ENDLESS_TOWER_FLOOR_CLEAR_COUNT, Mathf.Max(1, nextFloor - 1)); // 엔들리스 타워 특정 특 클리어 횟수

            TryEnterNextloor();
        }

        /// <summary>
        /// 엔들리스 타워 종료
        /// </summary>
        void OnEndlessTowerExit(int floor, RewardData[] rewards)
        {
            isRequestExit = true;

            uiBattleEndlessTower.StopTimer(); // 타이머 꺼줌

            Timing.RunCoroutine(YieldShowResult(rewards), TAG);
        }

        private IEnumerator<float> YieldShowResult(RewardData[] rewards)
        {
            yield return Timing.WaitForSeconds(2f);

            uiResultClear.Show(rewards, DungeonType.EnlessTower, false, UIResultClear.ResultType.Result);
            uiResultClear.SetDescription(string.Empty);
        }

        void OnStartController()
        {
            if (currentPlayer == null || currentPlayer.IsDie)
                return;

            UnitActor actor = currentPlayer.GetActor();
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

            if (currentPlayer == null || currentPlayer.IsDie)
                return;

            if (currentPlayer.battleCrowdControlInfo.GetCannotMove())
                return;

            UnitActor actor = currentPlayer.GetActor();
            if (actor == null)
                return;

            if (actor.IsPlayingActiveSkill()) // 액티브스킬 사용 중 이동 불가
                return;

            if (actor.AI.IsInputMove() == false) // OnStartController를 거치지 않고 Drag상태로 시작한 경우, 프로펠러 현상을 방지.
            {
                actor.AI.SetInputMove(isControl: true);
            }

            Vector3 motion = mainCamera.transform.TransformDirection(position);
            motion.y = 0;
            motion.Normalize();
            actor.Movement.Move(motion);
        }

        void OnResetController()
        {
            CameraUtils.Zoom(CameraZoomType.None);

            if (currentPlayer == null || currentPlayer.IsDie)
                return;

            UnitActor actor = currentPlayer.GetActor();
            if (actor == null)
                return;

            actor.AI.SetInputMove(isControl: false);
            actor.Movement.Stop();
        }

        private void Finish()
        {
            if (isRequestExit)
                return;

            isRequestExit = true;

            for (int i = 0; i < unitList.size; i++)
            {
                if (unitList[i].IsDie)
                    continue;

                UnitActor actor = unitList[i].GetActor();
                if (actor == null)
                    continue;

                actor.AI.EndAI();
            }

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", killCount);
            Protocol.ENDLESS_DUNGEON_END.SendAsync(sfs).WrapNetworkErrors();
        }

        private void GiveUp()
        {
            AsyncShowGiveUpMessage().WrapNetworkErrors();
        }

        private async Task AsyncShowGiveUpMessage()
        {
            if (!await UI.SelectPopup(LocalizeKey._90073.ToText())) // 던전을 포기 하시겠습니까?
                return;

            Finish();
        }

        void OnSelectSkill(SkillInfo info, UIBattleNormalSkillSlot.SlotType slotType)
        {
            if (slotType != UIBattleNormalSkillSlot.SlotType.Skill)
                return;

            if (currentPlayer == null || currentPlayer.IsDie)
                return;

            UnitActor unitActor = currentPlayer.GetActor();
            if (unitActor == null)
                return;

            int mpCost = currentPlayer.CurMp - info.MpCost;
            if (mpCost < 0)
            {
                UI.ShowToastPopup(LocalizeKey._90142.ToText()); // 스킬 포인트가 부족합니다.
                return;
            }

            unitActor.AI.SetInputSkill(info);
        }

        void OnSelectAgent(CharacterEntity characterEntity)
        {
            if (characterEntity == null || characterEntity.IsDie)
                return;

            SelectCharacter(characterEntity);
        }

        /// <summary>
        /// 바인딩 타켓 세팅
        /// </summary>
        private void SetFollowBindingTarget(bool isFollowBinding)
        {
            this.isFollowBinding = isFollowBinding;
            RefreshFollowBindingTarget();
        }

        /// <summary>
        /// 자동스킬 세팅
        /// </summary>
        private void SetAutoSkill(bool isAutoSkill)
        {
            for (int i = 0; i < characterList.size; i++)
            {
                characterList[i].Skill.SetAntiSkillAuto(!isAutoSkill);
            }
        }

        /// <summary>
        /// 현재 전투 나가기
        /// </summary>
        protected override void ExitEntry()
        {
            base.ExitEntry();

            player.Skill.SetAntiSkillAuto(false); // 스킬 자동 나가기 처리
        }

        /// <summary>
        /// 캐릭터 선택
        /// </summary>
        private void SelectCharacter(CharacterEntity entity)
        {
            if (ReferenceEquals(currentPlayer, entity))
                return;

            OnResetController(); // 이동 도중 플레이어가 바뀌면 허공에서 계속 달리게 된다.

            // 이전 조종 캐릭터에 대한 처리
            if (currentPlayer != null)
            {
                Release(currentPlayer.GetActor());
            }

            currentPlayer = entity;

            // 현재 조종 캐릭터에 대한 처리
            UnitActor unitActor = currentPlayer.GetActor();
            if (unitActor)
            {
                cameraController.SetPlayer(unitActor.CachedTransform); // 카메라 대상 변경

                CharacterEffectPlayer charaEffectPlayer = unitActor.EffectPlayer as CharacterEffectPlayer;
                charaEffectPlayer.SetAttackImpulse(true); // // 공격 시 카메라 흔들림 설정
                charaEffectPlayer.ShowUnitCircle(); // UnitCircle 설정
            }

            uiBattleGuildAttackSkillList.SetCharacter(currentPlayer); // 스킬 조작 UI 업데이트

            int index = characterList.IndexOf(currentPlayer);
            uiBattleAgentList.SetSelect(index); // 선택

            RefreshFollowBindingTarget(); // 바인딩 타겟 새로고침
            RefreshBossTargetingArrow(); // 보스 타겟 화살표 새로고침
        }

        /// <summary>
        /// 바인딩 타겟 새로고침
        /// </summary>
        private void RefreshFollowBindingTarget()
        {
            if (isFollowBinding) // 바인딩 상태
            {
                UnitActor unitActor = currentPlayer?.GetActor(); // 현재 선택한 캐릭터
                for (int i = 0; i < characterList.size; i++)
                {
                    UnitActor actor = characterList[i].GetActor();
                    if (actor == null)
                        continue;

                    if (characterList[i].Equals(currentPlayer))
                    {
                        actor.AI.SetBindingActor(null);
                    }
                    else
                    {
                        actor.AI.SetBindingActor(unitActor); // 현재 선택한 캐릭터로 바인딩
                    }
                }
            }
            else // 바인딩 상태가 아닐 경우
            {
                for (int i = 0; i < characterList.size; i++)
                {
                    UnitActor actor = characterList[i].GetActor();
                    if (actor == null)
                        continue;

                    actor.AI.SetBindingActor(null); // 바인딩 제거
                }
            }
        }

        /// <summary>
        /// 보스 타겟 화살표 새로고침
        /// </summary>
        private void RefreshBossTargetingArrow()
        {
            if (bossMonster == null)
            {
                for (int i = 0; i < characterList.size; i++)
                {
                    UnitActor actor = characterList[i].GetActor();
                    if (actor == null)
                        continue;

                    actor.EffectPlayer.ReleaseBossTargetingArrow();
                }
            }
            else
            {
                for (int i = 0; i < characterList.size; i++)
                {
                    UnitActor actor = characterList[i].GetActor();
                    if (actor == null)
                        continue;

                    if (characterList[i].Equals(currentPlayer))
                    {
                        actor.EffectPlayer.ShowBossTargetingArrow(bossMonster);
                    }
                    else
                    {
                        actor.EffectPlayer.ReleaseBossTargetingArrow();
                    }
                }
            }
        }

        /// <summary>
        /// 다음 캐릭터 선택
        /// </summary>
        private void SelectNextCharcater(CharacterEntity characterEntity)
        {
            int size = characterList.size; // 캐릭터 수
            int index = characterList.IndexOf(characterEntity); // 현재 캐릭터 index

            while (--size > 0) // 죽은캐릭을 제외하고 찾음 (--size로 1 깎으면서 시작)
            {
                // 다음 캐릭터
                if (++index >= characterList.size)
                    index = 0;

                // 죽은 캐릭터
                if (characterList[index].IsDie)
                    continue;

                SelectCharacter(characterList[index]);
                break;
            }
        }

        /// <summary>
        /// 다음 층 존재 여부
        /// </summary>
        private bool HasNextFloor()
        {
            int nextFloor = currentData.GetFloor() + 1; // 다음 층

            // 최대층을 초과
            if (nextFloor > maxFloor)
                return false;
            EndlessTowerData data = endlessTowerDataRepo.GetByFloor(nextFloor);
            return data != null;
        }

        /// <summary>
        /// 층에 해당하는 데이터 세팅
        /// </summary>
        private bool SetFloor(int floor)
        {
            EndlessTowerData data = endlessTowerDataRepo.GetByFloor(floor);
            if (data == null)
                return false;

            currentData = data;
            return true;
        }

        /// <summary>
        /// 동료 세팅
        /// </summary>
        private void SetAgents()
        {
            foreach (var item in agentModel.GetEquipedCombatAgents())
            {
                if (item.ID == 0 || item.AgentData == null)
                    continue;

                agentPacketBuffer.Add(new AgentCharacterPacket(item.AgentData));
            }

            int jobLevel = characterModel.JobLevel;
            int totalStatPoint = statDataRepo.GetTotalPoint(jobLevel); // JobLevel 로 맞춘다
            int maxStatus = statusModel.MaxStatus();
            for (int i = 0; i < agentPacketBuffer.size; i++)
            {
                int jobId = agentPacketBuffer[i].job;
                JobData agentJobData = jobDataRepo.Get(jobId);
                if (agentJobData == null)
                {
                    Debug.LogError($"JobData가 존재하지 않습니다: {nameof(jobId)} = {jobId}");
                    continue;
                }
                JobData.StatValue basicStat = new JobData.StatValue(0);
                JobData.StatValue maxStat = new JobData.StatValue(maxStatus);
                short[] plusGuidStats = agentJobData.GetAutoStatGuidePoints(totalStatPoint, basicStat, maxStat);
                agentPacketBuffer[i].UpdateStatus(jobLevel, plusGuidStats[0], plusGuidStats[1], plusGuidStats[2], plusGuidStats[3], plusGuidStats[4], plusGuidStats[5]);
            }

            // 멀티 플레이어 세팅
            foreach (var item in agentPacketBuffer.GetBuffer(isAutoRelease: true))
            {
                agentCharacterFactory.AddGhostPlayer(item);
            }
        }

        /// <summary>
        /// 포탈 오픈
        /// </summary>
        private void ShowPortal()
        {
            RequestClear();

            soundManager.PlaySfx("[SYSTEM] Congratulation");

            map.TweenEndlessTowerLight(EndlessTowerMap.LightType.Clear); // Map Light 세팅
            UI.ShowToastPopup(LocalizeKey._90290.ToText()); // 포탈이 열렸습니다. 다음 층으로 이동하세요.
        }

        /// <summary>
        /// 해당 층 클리어 호출
        /// </summary>
        private void RequestClear()
        {
            if (isRequestExit)
                return;

            if (isRequestClear)
                return;

            isRequestClear = true;

            if (HasNextFloor())
            {
                float recoveryRate = currentData.GetRecoveryRate(); // 회복률
                for (int i = 0; i < characterList.size; i++)
                {
                    int nextHp = characterList[i].CurHP;
                    int nextMp = characterList[i].CurMp;

                    if (!characterList[i].IsDie)
                    {
                        nextHp += MathUtils.ToInt(characterList[i].MaxHP * recoveryRate);
                        nextMp += MathUtils.ToInt(characterList[i].MaxMp * recoveryRate);
                    }

                    characterNextHpList.Add(nextHp); // 다음 층 hp 세팅
                    characterNextMpList.Add(nextMp); // 다음 층 mp 세팅
                }
            }

            Protocol.ENDLESS_DUNGEON_STORYCLEAR.SendAsync().WrapNetworkErrors();
        }

        /// <summary>
        /// 캐릭터 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnCharacters()
        {
            for (int i = 0; i < characterList.size; i++)
            {
                if (!impl.Add(characterList[i], isEnemy: false))
                    continue;

                characterList[i].OnSpawnActor += OnCharacterSpawn;
                characterList[i].OnDespawnActor += OnCharacterDespawn;

                UnitActor unitActor = characterList[i].SpawnActor(); // 유닛 소환
                unitActor.Movement.SetDefaultSpeed(Constants.Battle.DEFAULT_MOVE_SPEED);
                unitActor.AI.SetFollowBindingTargetState(); // 바인딩 설정
                unitActor.AI.SetLookEnemyContainsFollower(); // 추격자가 있는 적군도 타겟팅 포함
                yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 캐릭터 위치 세팅
        /// </summary>
        private void ResetCharacterPosition()
        {
            for (int i = 0; i < unitList.size; i++)
            {
                // 적군 제외
                if (unitList[i].IsEnemy)
                    continue;

                unitBuffers.Add(unitList[i]);
            }

            Vector3 posCenter = map.GetPlayerPosition();
            UnitEntity[] entities = unitBuffers.GetBuffer(isAutoRelease: true);
            Vector3[] positions = map.GetAroundPosition(posCenter, POSITION_DISTANCE, entities.Length);
            for (int i = 0; i < positions.Length; i++)
            {
                UnitActor unitActor = entities[i].GetActor(); // 유닛 소환
                unitActor.AI.SetHomePosition(positions[i], isWarp: true); // 위치 세팅
            }
        }

        /// <summary>
        /// 일반 몬스터 생성
        /// </summary>
        private IEnumerator<float> YieldSpawnMonster()
        {
            int zoneCount = map.ZoneCount;
            int monsterLevel = currentData.normal_monster_level;
            for (int zoneIndex = 0; zoneIndex < zoneCount; zoneIndex++)
            {
                foreach (int monsterId in currentData.GetNormalMonsterId())
                {
                    UnitEntity monsterEntity = factory.CreateNormalMonster(monsterId, monsterLevel);

                    // 추가 실패 (이미 등록 되어있음)
                    if (!impl.Add(monsterEntity, isEnemy: true))
                        continue;

                    unitBuffers.Add(monsterEntity);
                }

                UnitEntity[] entities = unitBuffers.GetBuffer(isAutoRelease: true);
                Vector3[] positions = map.GetMonsterPositions(zoneIndex, entities.Length); // 위치 반환
                for (int i = 0; i < positions.Length; i++)
                {
                    UnitActor monsterActor = entities[i].SpawnActor(); // 유닛 소환
                    monsterActor.AI.SetHomePosition(positions[i], isWarp: true); // 위치 세팅

                    yield return Timing.WaitForOneFrame;
                }
            }
        }

        /// <summary>
        /// 보스 몬스터 생성
        /// </summary>
        private void SpawnBossMonster()
        {
            bossMonster = factory.CreateBossMonster(currentData);

            UnitActor bossActor;
            if (impl.Add(bossMonster, isEnemy: true))
            {
                bossMonster.OnChangeHP += OnChangedBossMonster;
                bossMonster.OnDespawnActor += OnBossMonsterDespawn;

                bossActor = bossMonster.SpawnActor(); // 유닛 소환
            }
            else
            {
                bossActor = bossMonster.GetActor();
            }

            Vector3 homePosition = map.GetBossPosition();
            bossActor.AI.SetHomePosition(homePosition, isWarp: true);
            bossActor.EffectPlayer.ShowUnitCircle();

            MonsterData monsteData = monsterDataRepo.Get(bossMonster.battleUnitInfo.Id);
            float hudOffset = monsteData.GetHudOffset();
            PoolObject bossHud = bossActor.EffectPlayer.SetHpHudTarget(new Vector3(0, hudOffset, 0));

            uiBattleBossHp.Show(
                bossMonster.battleUnitInfo.Level,
                bossMonster.GetName(),
                bossMonster.battleUnitInfo.UnitElementType,
                UIBattleBossHp.MonsterType.NormalBoss,
                monsteData.cost,
                bossHud.CachedGameObject, 0);
        }

        void OnCharacterSpawn(UnitActor actor)
        {
            actor.Movement.OnTempPortal += OnPortal;
        }

        void OnCharacterDespawn(UnitActor actor)
        {
            actor.AI.ResetFollowBindingTargetState(); // 바인딩 해제
            actor.AI.ResetLookEnemyContainsFollower(); // 추격자가 있는 적군 타겟팅 제외

            actor.Entity.OnSpawnActor -= OnCharacterSpawn;
            actor.Entity.OnDespawnActor -= OnCharacterDespawn;
            actor.Movement.OnTempPortal -= OnPortal;

            Release(actor);
        }

        /// <summary>
        /// 다음층 시작
        /// </summary>
        private void TryEnterNextloor()
        {
            // 포털에 진입한 상태가 아님
            if (!isOnPortal)
                return;

            // 이미 종료한 경우에는 처리하지 않음
            if (isRequestExit)
                return;

            // 클리어 정보가 없을 경우에는 포털 금지
            if (!isRequestClear)
                return;

            // 서버에게서 클리어 확인 받았는지 확인.
            if (!IsResponseClear)
                return;

            if (!SetFloor(nextFloor))
                return;

            impl.StartBattle(BattleMode.EndlessTower, isStartFadeIn: true);
        }

        void OnPortal(GameObject portal, UnitEntity entity)
        {
            // 나갔을 경우
            if (portal == null)
            {
                isOnPortal = false;
            }
            else
            {
                isOnPortal = currentPlayer.Equals(entity);
            }

            TryEnterNextloor();
        }

        void OnBossMonsterDespawn(UnitActor actor)
        {
            actor.Entity.OnChangeHP -= OnChangedBossMonster;
            actor.Entity.OnDespawnActor -= OnBossMonsterDespawn;

            if (uiBattleBossHp)
                uiBattleBossHp.Hide();

            bossMonster = null; // 보스 몬스터 초기화

            RefreshFollowBindingTarget(); // 바인딩 타겟 새로고침
        }

        void OnChangedBossMonster(int cur, int max)
        {
            uiBattleBossHp.TweenHp(cur, max);
        }

        private void Release(UnitActor actor)
        {
            if (actor == null)
                return;

            CharacterEffectPlayer charaEffectPlayer = actor.EffectPlayer as CharacterEffectPlayer;
            charaEffectPlayer.SetAttackImpulse(false); // 공격 시 카메라흔들림 제거
            charaEffectPlayer.HideUnitCircle(); // UnitCircle 제거
            charaEffectPlayer.ReleaseBossTargetingArrow();
        }
    }
}