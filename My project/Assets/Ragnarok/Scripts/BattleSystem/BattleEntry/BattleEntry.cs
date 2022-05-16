using MEC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ragnarok
{
    public abstract class BattleEntry
    {
        protected const string TAG = nameof(BattleEntry);

        private static System.Action nextSingleAction;

        private bool isSuccessExit = true;
        public virtual bool IsSuccessExit
        {
            get { return isSuccessExit; }
            protected set
            {
                isSuccessExit = value;
            }
        }

        private bool isSuccessEnter = true;
        public bool IsSuccessEnter
        {
            get { return isSuccessEnter; }
            protected set
            {
                isSuccessEnter = value;

                // 입장 성공과 동시에 바로 Entry 세팅 처리! (입장과 동시에 AddEvent)
                // 입장 성공 후 바로 날아오는 프로토콜이 누락되지 않도록!
                if (isSuccessEnter)
                {
                    impl.SetBattleEntry(this);
                }
                else
                {
                    // 입장 실패 시 프로토콜 이벤트 제거
                    RemoveProtocolEvent();
                }
            }
        }
        public virtual bool IsSkipExit { get; protected set; } = false;

        public readonly BattleMode mode;

        protected readonly CharacterEntity player;
        protected readonly IBattleManagerImpl impl;
        protected readonly BattleUnitList unitList;
        protected readonly SpawnUnitFactory factory; // 소환 정보
        protected readonly Buffer<RewardData> rewardBuffer; // 보상 버퍼
        protected readonly CameraController cameraController;
        private readonly SoundManager soundManager;

        protected string sceneName; // 씬 이름
        private string bgmName; // 배경음악 이름
        private bool isChangeScene; // 씬 변경 여부
        private SceneLoader.SceneType sceneType; // 씬 타입

        private bool isSceneLoaded; // 씬 로드 완료
        private bool isFinishedSceneLoaded; // 씬 마지막 로드 완료

        protected bool IsInitCanvas; // canvas 초기화

        /// <summary>
        /// 모든 준비 완료 여부
        /// </summary>
        public bool IsAllReady { get; protected set; }

        protected bool IsPause { get; private set; }

        /// <summary>
        /// 다른 BattleMode 로 진입할 때 false 로 변경
        /// </summary>
        protected bool IsFocus { get; private set; }

        protected BattleEntry(BattleMode mode)
        {
            this.mode = mode;

            player = Entity.player;
            player.SetState(UnitEntity.UnitState.Stage);
            unitList = BattleManager.Instance.unitList;
            impl = BattleManager.Instance;
            factory = new SpawnUnitFactory();
            rewardBuffer = new Buffer<RewardData>();
            cameraController = CameraController.Instance;
            soundManager = SoundManager.Instance;
            AddProtocalEvent();
        }

        /// <summary>
        /// Entry 실행 (초기화 / 이벤트 추가)
        /// </summary>
        public void Run()
        {
            AddEvent(); // 이벤트 추가
        }

        /// <summary>
        /// Entry 종료 (이벤트 제거 / 폐기)
        /// </summary>
        public void Destroy()
        {
            RemoveEvent(); // 이벤트 제거
            Dispose(); // 폐기
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected virtual void Dispose()
        {
            StopAllCoroutine(); // 모든 코루틴 멈춤

            DestroyCanvas(); // 캔버스 폐기
            ResetRewardStack(); // 스택 초기화

            cameraController.ClearMember(); // 카메라 그룹 모든 맴버 초기화
        }

        /// <summary>
        /// 유닛 폐기
        /// </summary>
        protected virtual void Dispose(UnitEntity entity)
        {
            if (entity == null)
                return;

            entity.ResetPassiveBuffSkills();
            entity.DespawnActor();
            //entity.ResetData();
            //entity.Dispose();
            impl.Remove(entity);
        }

        /// <summary>
        /// 준비
        /// </summary>
        protected void Ready(UnitEntity entity)
        {
            UnitActor actor = entity.GetActor();
            actor.AI.ReadyToBattle(); // 전투 시작
        }

        /// <summary>
        /// 프로토콜 이벤트 추가
        /// </summary>
        protected virtual void AddProtocalEvent()
        {
            Debug.Log($"TAG={TAG} [{mode}] AddProtocalEvent");
        }

        /// <summary>
        /// 프로토콜 이벤트 제거
        /// </summary>
        protected virtual void RemoveProtocolEvent()
        {
            Debug.Log($"TAG={TAG} [{mode}] RemoveProtocolEvent");
        }

        /// <summary>
        /// 이벤트 추가
        /// </summary>
        protected virtual void AddEvent()
        {
            UI.AddEventLocalize(OnLocalize);
            SceneLoader.OnSceneLoaded += OnSceneLoaded;
            SceneLoader.OnFinishLoadScene += OnFinishLoadScene;
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        protected virtual void RemoveEvent()
        {
            UI.RemoveEventLocalize(OnLocalize);
            SceneLoader.OnSceneLoaded -= OnSceneLoaded;
            SceneLoader.OnFinishLoadScene -= OnFinishLoadScene;
        }

        public virtual void OnBack()
        {
            UI.ShowExitPopup();
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected abstract void InitCanvas();

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected abstract void DisposeCanvas();

        /// <summary>
        /// 보상 스택 초기화
        /// </summary>
        public virtual void ResetRewardStack()
        {
            rewardBuffer.Clear();
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        public virtual void EndBattle()
        {
            IsAllReady = false;
            impl.EndAllUnit();
            impl.InvokeEnd();

            StopAllCoroutine(); // 코루틴 해제
        }

        /// <summary>
        /// 나가기 요청
        /// </summary>
        public virtual IEnumerator<float> YieldExitBattle()
        {
            IsSuccessExit = true;
            yield break;
        }

        /// <summary>
        /// 진입 요청
        /// </summary>
        public virtual IEnumerator<float> YieldEnterBattle()
        {
            UI.HideIndicator();
            yield break;
        }

        /// <summary>
        /// 진입 요청
        /// </summary>
        public virtual IEnumerator<float> YieldEnterBattle(int id)
        {
            UI.HideIndicator();
            yield break;
        }

        /// <summary>
        /// 진입 요청
        /// </summary>
        public virtual IEnumerator<float> YieldEnterBattle(IBattleInput battleInput)
        {
            UI.HideIndicator();
            yield break;
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public virtual void Ready()
        {
        }

        /// <summary>
        /// 죽은 유닛 처리
        /// </summary>
        public abstract void OnDie(UnitEntity unitEntity, UnitEntity attacker);

        public virtual void OnDamageValue(UnitEntity unitEntity, UnitEntity attacker, int damage)
        {

        }

        public virtual void OnMonsterRequestSpawn(MonsterEntity bossMonster, int spawnID, int spawnCount) { }

        /// <summary>
        /// 전투 준비
        /// </summary>
        /// <param name="sceneName">씬 이름</param>
        /// <param name="bgmName">배경음악 이름</param>
        /// <param name="isChangeScene">씬 변경 여부</param>
        protected void Ready(string sceneName, string bgmName, bool isChangeScene, SceneLoader.SceneType sceneType)
        {
            impl.InvokePreStart(); // 전투 시작전 이벤트 호출

            this.sceneName = sceneName;
            this.bgmName = bgmName;
            this.isChangeScene = isChangeScene;
            this.sceneType = sceneType;
            isSceneLoaded = false;
            isFinishedSceneLoaded = false;

            NextFirstFadeIn();
        }

        /// <summary>
        /// 첫번째 FadeIn 연출 완료 후 호출
        /// </summary>
        private void NextFirstFadeIn()
        {
            if (isChangeScene) // 씬을 바꿔야 하는 경우
            {
                SceneLoader.LoadingStoryboardType loadingStoryboardType = SceneLoader.LoadingStoryboardType.Immediate; // 씬을 로드할 방식 설정
                SceneLoader.LoadDungeon(sceneName, loadingStoryboardType, sceneType);
            }
            else
            {
                isSceneLoaded = true;
                OnFinishLoadScene();
            }

            SceneLoaded();
        }

        /// <summary>
        /// 씬 로드 완료
        /// </summary>
        private void SceneLoaded()
        {
            if (!isSceneLoaded)
                return;

            if (!isFinishedSceneLoaded)
                return;

            isSceneLoaded = false; // 초기화
            isFinishedSceneLoaded = false; // 초기화

            NextSecondFade();
        }

        /// <summary>
        /// 두번째 Fade 후 호출
        /// </summary>
        private void NextSecondFade()
        {
            CreateCanvas(); // 필요한 UI 생성
            OnReady();

            // 씬 세팅, 전투 준비
            Timing.RunCoroutine(YieldComposeScene()
                .Append(FinishComposeScene), TAG);
        }

        /// <summary>
        /// 준비 완료 후 호출 - FadeOut 전
        /// </summary>
        protected virtual void OnReady()
        {
            OnLocalize(); // 로컬라이즈 호출
            impl.InvokeStart();
        }

        /// <summary>
        /// 씬 세팅
        /// </summary>
        protected abstract IEnumerator<float> YieldComposeScene();

        /// <summary>
        /// 씬 세팅 종료
        /// </summary>
        private void FinishComposeScene()
        {
            if (impl.IsPlayingFadeIn())
            {
                UIFade uiFade = UI.GetUI<UIFade>();

                if (uiFade == null)
                {
                    AfterFinishComposeScene();
                }
                else
                {
                    uiFade.PlayFadeOut(AfterFinishComposeScene); // Fade UI 사라지기
                }
            }
            else
            {
                AfterFinishComposeScene();
            }
        }

        private void AfterFinishComposeScene()
        {
            soundManager.PlayBgm(bgmName); // 배경음악 실행

            // 씬 세팅, 전투 준비
            Timing.RunCoroutine(YieldComposeSceneAfter()
                .Append(OnAllReady).Append(InvokeNextAction), TAG);
        }

        protected void PlaySfx(string name)
        {
            soundManager.PlaySfx(name);
        }

        /// <summary>
        /// 씬 세팅
        /// </summary>
        protected virtual IEnumerator<float> YieldComposeSceneAfter()
        {
            yield break;
        }

        /// <summary>
        /// 모든 준비 완료 후 호출 - FadeOut 후
        /// </summary>
        protected virtual void OnAllReady()
        {
            IsAllReady = true;
            impl.InvokeReady();
            impl.ReadyAllUnit(); // 전투 준비
        }

        /// <summary>
        /// 캔버스 생성
        /// </summary>
        private void CreateCanvas()
        {
            if (IsInitCanvas)
                return;

            InitCanvas();
            IsInitCanvas = true;
        }

        /// <summary>
        /// 캔버스 제거
        /// </summary>
        private void DestroyCanvas()
        {
            if (!IsInitCanvas)
                return;

            IsInitCanvas = false;
            DisposeCanvas();
        }

        /// <summary>
        /// 코루틴 제거
        /// </summary>
        protected virtual void StopAllCoroutine()
        {
            Timing.KillCoroutines(TAG);
        }

        protected abstract void OnSceneLoaded(GameObject[] roots);

        /// <summary>
        /// 로컬라이즈 업데이트
        /// </summary>
        protected abstract void OnLocalize();

        /// <summary>
        /// 현재 전투 나가기
        /// </summary>
        protected virtual void ExitEntry()
        {
            impl.ExitBattle(); // 현재 전투 나가기
        }

        /// <summary>
        /// 씬 로드 시 호출
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Intro는 Entry가 없음
            if (SceneLoader.CurrentType == SceneLoader.SceneType.Intro)
                return;

            OnSceneLoaded(scene.GetRootGameObjects());

            isSceneLoaded = true;
            SceneLoaded();
        }

        /// <summary>
        /// 씬 모든 준비 완료 시 호출
        /// </summary>
        private void OnFinishLoadScene()
        {
            // Intro는 Entry가 없음
            if (SceneLoader.CurrentType == SceneLoader.SceneType.Intro)
                return;

            isFinishedSceneLoaded = true;
            SceneLoaded();
        }

        public void SetSkipExit()
        {
            IsSkipExit = true;
        }

        public void Pause()
        {
            IsPause = true;

            foreach (UnitEntity unit in unitList)
            {
                UnitActor actor = unit.GetActor();
                if (actor == null)
                    continue;

                actor.AI.Pause();
                actor.Movement.Pause();
            }
        }

        public void Resume()
        {
            foreach (UnitEntity unit in unitList)
            {
                UnitActor actor = unit.GetActor();
                if (actor == null)
                    continue;

                actor.AI.Resume();
                actor.Movement.Resume();
            }

            IsPause = false;
        }

        protected void SetNextSingleAction(System.Action action)
        {
            nextSingleAction = action;
        }

        private void InvokeNextAction()
        {
            if (nextSingleAction != null)
            {
                nextSingleAction();
                nextSingleAction = null;
            }
        }

        public void OnServerChange()
        {
            RemoveProtocolEvent();
        }

        public void ResetProtocolEvent()
        {
            RemoveProtocolEvent();
        }

        public virtual void SetFocus()
        {
            IsFocus = true;
        }

        public virtual void LostFocus()
        {
            IsFocus = false;
        }

        public static void ResetNextAction()
        {
            nextSingleAction = null;
        }

        /// <summary>
        /// 가장 가까운 네비위치 반환
        /// </summary>
        protected Vector3 FindClosestEdge(Vector3 pos)
        {
            bool isHit = UnityEngine.AI.NavMesh.SamplePosition(pos, out var hit, Constants.Map.GuildMaze.NAVMESH_SAMPLE_POSITION_RANGE, UnityEngine.AI.NavMesh.AllAreas);
            return isHit ? hit.position : pos;
        }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is BattleEntry value)
                return Equals(value);

            return false;
        }

        public override int GetHashCode()
        {
            return mode.GetHashCode();
        }

        public bool Equals(BattleEntry value)
        {
            if (value == null)
                return false;

            return mode == value.mode;
        }

        public static bool operator ==(BattleEntry a, BattleEntry b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(BattleEntry a, BattleEntry b)
        {
            return !(a == b);
        }

        #endregion

        public static class Factory
        {
            public static BattleEntry Create(BattleMode mode)
            {
                switch (mode)
                {
                    case BattleMode.Stage:
                        return new StageEntry();

                    case BattleMode.Defence:
                        return new DefenceDungeonEntry();

                    case BattleMode.WorldBoss:
                        return new WorldBossEntry();

                    case BattleMode.League:
                        return new LeagueEntry();

                    case BattleMode.Lobby:
                        return new LobbyEntry();

                    case BattleMode.CentralLab:
                        return new CentralLabEntry();

                    case BattleMode.ClickerDungeon:
                        return new ClickerDungeonEntry();

                    case BattleMode.SpecialDungeon:
                        return new SpecialDungeonEntry();

                    case BattleMode.Challenge:
                        return new ChallengeEntry();

                    case BattleMode.Siege:
                        return new SiegeEntry();

                    case BattleMode.TamingMaze:
                        return new TamingMazeEntry();

                    case BattleMode.MultiMazeLobby:
                        return new MultiMazeLobbyEntry();

                    case BattleMode.MultiMaze:
                        return new MultiMazeEntry();

                    case BattleMode.MultiBossMaze:
                        return new MultiBossMazeEntry();

                    case BattleMode.ScenarioMaze:
                        return new ScenarioMazeEntry();

                    case BattleMode.ScenarioMazeBoss:
                        return new ScenarioMazeBossEntry();

                    case BattleMode.Duel:
                        return new DuelEntry();

                    case BattleMode.MatchMultiMaze:
                        return new MatchMultiMazeEntry();

                    case BattleMode.MatchMultiMazeBoss:
                        return new MatchMultiBossMazeEntry();

                    case BattleMode.Prologue:
                        return new PrologueEntry();

                    case BattleMode.FreeFight:
                        return new FreeFightEntry();

                    case BattleMode.GuildLobby:
                        return new GuildLobbyEntry();

                    case BattleMode.GuildAttack:
                        return new GuildAttackEntry();

                    case BattleMode.ChristmasMatchMultiMaze:
                        return new ChristmasMatchMultiMazeEntry();

                    case BattleMode.ChristmasFreeFight:
                        return new ChristmasFreeFightEntry();

                    case BattleMode.WaterBombFreeFight:
                        return new WaterBombFreeFightEntry();

                    case BattleMode.EndlessTower:
                        return new EndlessTowerEntry();

                    case BattleMode.ForestMaze:
                        return new ForestMazeEntry();

                    case BattleMode.ForestMazeBoss:
                        return new ForestMazeBossEntry();

                    case BattleMode.TimePatrol:
                        return new TimePatrolEntry();

                    case BattleMode.GuildBattle:
                        return new GuildBattleEntry();

                    case BattleMode.DarkMaze:
                        return new DarkMazeEntry();

                    case BattleMode.GateMaze:
                        return new GateMazeEntry();

                    case BattleMode.GateBoss:
                        return new GateBossEntry();

                    case BattleMode.GateWorldBoss:
                        return new GateWorldBossEntry();

#if UNITY_EDITOR
                    case BattleMode.Test:
                        return new TestEntry();
#endif

                    default:
                        throw new System.ArgumentException($"유효하지 않은 처리: {nameof(mode)} = {mode}");
                }
            }
        }
    }
}