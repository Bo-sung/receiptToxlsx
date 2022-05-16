using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class BattleManager : Singleton<BattleManager>, IBattleManagerImpl
    {
        public static readonly string TAG = nameof(BattleManager);

        public delegate void PreStartEvent(BattleMode mode);
        public delegate void StartEvent(BattleMode mode);
        public delegate void ReadyEvent();
        public delegate void EndEvent();
        public delegate void ChangeBattleModeEvent(BattleMode mode);
        public delegate void EnterFailEvent(BattleMode mode);

        /// <summary>
        /// 스킬 사용 시 오토 체크 해제 여부
        /// </summary>
        public static bool isAntiAutoSkill = false;

        /// <summary>
        /// 스킬 사용 시 거리 체크 해제 여부
        /// </summary>
        public static bool isAntiChaseSkill = false;

        /// <summary>
        /// [길드 미로 등] 대미지 독립적으로 처리 여부
        /// </summary>
        public static bool isIndependenceDamage = false;

        /// <summary>
        /// [길드 미로, 시나리오 미로] 최대 체력 수치가 직접 주어짐
        /// </summary>
        public static bool isFixedMaxHp = false;

        /// <summary>
        /// 스킬 포인트 사용
        /// </summary>
        public static bool isUseSkillPoint = false;

        /// <summary>
        /// 전투 유닛 관리
        /// </summary>
        public readonly BattleUnitList unitList;

        /// <summary>
        /// 전투 입장 처리
        /// </summary>
        private BattleEntry battleEntry;

        /// <summary>
        /// 새로 입장 할 전투
        /// </summary>
        private BattleEntry newBattleEntry;

        /// <summary>
        /// 전투 시작 전 호출
        /// </summary>
        public static event PreStartEvent OnPreStart;

        /// <summary>
        /// 전투 시작 시 호출
        /// </summary>
        public static event StartEvent OnStart;

        /// <summary>
        /// 씬, 몬스터 배치까지 완벽히 준비된 상태에서 호출
        /// </summary>
        public static event ReadyEvent OnReady;

        /// <summary>
        /// 전투 종료 시 호출
        /// </summary>
        public static event EndEvent OnEnd;

        /// <summary>
        /// 전투 모드 변경 시 호출
        /// </summary>
        public static event ChangeBattleModeEvent OnChangeBattleMode;

        /// <summary>
        /// 입장 실패 시 호출
        /// </summary>
        public static event EnterFailEvent OnEnterFail;

        /// <summary>
        /// 현재 진행중인 Mode
        /// </summary>
        public BattleMode Mode => battleEntry == null ? default : battleEntry.mode;

        public BattleMode GetSaveBattleMode() => saveBattleMode;

        public BattleMode LastAutoBattleMode { get; private set; } = BattleMode.Stage; // Stage, TimePatrol 둘중 하나
        // 게임 재접속 성공시 다시 요청
        private BattleMode saveBattleMode;
        private int? saveId;
        private IBattleInput saveBattleInput;

        public bool IsPlayingFadeIn() => isPlayingFadeIn;

        private bool isStartFadeIn;
        private bool isPlayingFadeIn;

        public BattleManager()
        {
            unitList = new BattleUnitList();
            ConnectionManager.OnServerChange += OnServerChange;
            SceneLoader.OnTitleSceneLoaded += BattleEntry.ResetNextAction;
        }

        ~BattleManager()
        {
            ConnectionManager.OnServerChange -= OnServerChange;
            SceneLoader.OnTitleSceneLoaded -= BattleEntry.ResetNextAction;
        }

        protected override void OnTitle()
        {
            ResetBattle();
        }

        /// <summary>
        /// 전투 리셋
        /// </summary>
        public void ResetBattle()
        {
            ResetUnitList(); // 전투 유닛 리셋
            ResetBattleEntry(); // 전투 입장 처리 리셋
        }

        /// <summary>
        /// 현재 전투 나가기
        /// </summary>
        public void ExitBattle()
        {
            StartBattle(LastAutoBattleMode);
        }

        public void StartBattle(BattleMode mode, bool isStartFadeIn = true)
        {
            this.isStartFadeIn = isStartFadeIn;
            SetSaveBattleMode(mode, null, null);
            if (Mode == mode)
            {
                Timing.RunCoroutine(YieldReStartBattle(), TAG);
                return;
            }
            Timing.RunCoroutine(YieldStartBattle(mode), TAG);
        }

        /// <summary>
        /// 특정 전투 시작
        /// </summary>
        public void StartBattle(BattleMode mode, int id, bool isStartFadeIn = true)
        {
            this.isStartFadeIn = isStartFadeIn;
            SetSaveBattleMode(mode, id, null);
            if (Mode == mode)
            {
                Timing.RunCoroutine(YieldReStartBattle(id), TAG);
                return;
            }

            Timing.RunCoroutine(YieldStartBattle(mode, id), TAG);
        }

        /// <summary>
        /// 특정 전투 시작
        /// </summary>
        public void StartBattle(BattleMode mode, IBattleInput battleInput, bool isStartFadeIn = true)
        {
            this.isStartFadeIn = isStartFadeIn;
            SetSaveBattleMode(mode, null, battleInput);
            if (Mode == mode)
            {
                Timing.RunCoroutine(YieldReStartBattle(battleInput), TAG);
                return;
            }

            Timing.RunCoroutine(YieldStartBattle(mode, battleInput), TAG);
        }

        /// <summary>
        /// 전투 시작
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        IEnumerator<float> YieldStartBattle(BattleMode mode)
        {
            Debug.Log($"YieldStartBattle={mode}");
            // 기존 BattleEntry 나가기 요청
            if (battleEntry != null)
            {
                if (!battleEntry.IsSkipExit)
                {
                    yield return Timing.WaitUntilDone(battleEntry.YieldExitBattle());

                    // 나가기 실패
                    if (!battleEntry.IsSuccessExit)
                    {
                        //CloseFade();
                        yield break;
                    }
                }
            }

            if (isStartFadeIn && !isPlayingFadeIn)
            {
                isPlayingFadeIn = true;
                yield return Timing.WaitUntilDone(UI.Show<UIFade>().YieldFadeIn());
            }

            // 새로운 BattleEntry 입장 요청
            newBattleEntry = BattleEntry.Factory.Create(mode);

            // 포커스 처리
            if (battleEntry != null)
                battleEntry.LostFocus();

            newBattleEntry.SetFocus();

            yield return Timing.WaitUntilDone(newBattleEntry.YieldEnterBattle(), TAG);

            // 입장 실패
            if (!newBattleEntry.IsSuccessEnter)
            {
                // 입장 실패로 인한 포커스 처리
                newBattleEntry.LostFocus();

                if (battleEntry != null)
                    battleEntry.SetFocus();

                OnEnterFail?.Invoke(newBattleEntry.mode); // 입장 실패 이벤트

                CloseFade();
                Debug.LogError(string.Concat("입장 실패: ", newBattleEntry.mode));
                yield break;
            }
            
            newBattleEntry.EndBattle();
            newBattleEntry.ResetRewardStack();
            newBattleEntry.Ready();
        }

        /// <summary>
        /// 전투 시작
        /// </summary>
        IEnumerator<float> YieldStartBattle(BattleMode mode, int id)
        {
            Debug.Log($"YieldStartBattle={mode}, {id}");
            // 기존 BattleEntry 나가기 요청
            if (battleEntry != null)
            {
                if (!battleEntry.IsSkipExit)
                {
                    yield return Timing.WaitUntilDone(battleEntry.YieldExitBattle());

                    // 나가기 실패
                    if (!battleEntry.IsSuccessExit)
                    {
                        CloseFade();
                        Debug.LogError(string.Concat("퇴장 실패: ", battleEntry.mode));
                        yield break;
                    }
                }
            }

            if (isStartFadeIn && !isPlayingFadeIn)
            {
                isPlayingFadeIn = true;
                yield return Timing.WaitUntilDone(UI.Show<UIFade>().YieldFadeIn());
            }

            // 새로운 BattleEntry 입장 요청
            newBattleEntry = BattleEntry.Factory.Create(mode);

            // 포커스 처리
            if (battleEntry != null)
                battleEntry.LostFocus();

            newBattleEntry.SetFocus();

            yield return Timing.WaitUntilDone(newBattleEntry.YieldEnterBattle(id), TAG);

            // 입장 실패
            if (!newBattleEntry.IsSuccessEnter)
            {
                // 입장 실패로 인한 포커스 처리
                newBattleEntry.LostFocus();

                if (battleEntry != null)
                    battleEntry.SetFocus();

                OnEnterFail?.Invoke(newBattleEntry.mode); // 입장 실패 이벤트

                CloseFade();
                Debug.LogError(string.Concat("입장 실패: ", newBattleEntry.mode));
                yield break;
            }

            newBattleEntry.EndBattle();
            newBattleEntry.ResetRewardStack();
            newBattleEntry.Ready();
        }

        /// <summary>
        /// 전투 시작
        /// </summary>
        IEnumerator<float> YieldStartBattle(BattleMode mode, IBattleInput battleInput)
        {
            Debug.Log($"YieldStartBattle={mode}, {battleInput.GetDump()}");
            // 기존 BattleEntry 나가기 요청
            if (battleEntry != null)
            {
                if (!battleEntry.IsSkipExit)
                {
                    yield return Timing.WaitUntilDone(battleEntry.YieldExitBattle());

                    // 나가기 실패
                    if (!battleEntry.IsSuccessExit)
                    {
                        CloseFade();
                        Debug.LogError(string.Concat("퇴장 실패: ", battleEntry.mode));
                        yield break;
                    }
                }
            }

            if (isStartFadeIn && !isPlayingFadeIn)
            {
                isPlayingFadeIn = true;
                yield return Timing.WaitUntilDone(UI.Show<UIFade>().YieldFadeIn());
            }

            // 새로운 BattleEntry 입장 요청
            newBattleEntry = BattleEntry.Factory.Create(mode);

            // 포커스 처리
            if (battleEntry != null)
                battleEntry.LostFocus();

            newBattleEntry.SetFocus();

            yield return Timing.WaitUntilDone(newBattleEntry.YieldEnterBattle(battleInput), TAG);

            // 입장 실패
            if (!newBattleEntry.IsSuccessEnter)
            {
                // 입장 실패로 인한 포커스 처리
                newBattleEntry.LostFocus();

                if (battleEntry != null)
                    battleEntry.SetFocus();

                OnEnterFail?.Invoke(newBattleEntry.mode); // 입장 실패 이벤트

                CloseFade();
                Debug.LogError(string.Concat("입장 실패: ", newBattleEntry.mode));
                yield break;
            }

            newBattleEntry.EndBattle();
            newBattleEntry.ResetRewardStack();
            newBattleEntry.Ready();
        }

        IEnumerator<float> YieldReStartBattle()
        {
            if (battleEntry == null)
                yield break;

            if (!battleEntry.IsSkipExit)
                yield return Timing.WaitUntilDone(battleEntry.YieldExitBattle());

            if (isStartFadeIn && !isPlayingFadeIn)
            {
                isPlayingFadeIn = true;
                yield return Timing.WaitUntilDone(UI.Show<UIFade>().YieldFadeIn());
            }

            yield return Timing.WaitUntilDone(battleEntry.YieldEnterBattle(), TAG);

            // 입장 실패
            if (!battleEntry.IsSuccessEnter)
            {
                CloseFade();
                yield break;
            }

            battleEntry.EndBattle();
            battleEntry.ResetRewardStack();
            battleEntry.Ready();
        }

        IEnumerator<float> YieldReStartBattle(int id)
        {
            if (battleEntry == null)
                yield break;

            if (!battleEntry.IsSkipExit)
                yield return Timing.WaitUntilDone(battleEntry.YieldExitBattle());

            if (isStartFadeIn && !isPlayingFadeIn)
            {
                isPlayingFadeIn = true;
                yield return Timing.WaitUntilDone(UI.Show<UIFade>().YieldFadeIn());
            }

            yield return Timing.WaitUntilDone(battleEntry.YieldEnterBattle(id), TAG);

            // 입장 실패
            if (!battleEntry.IsSuccessEnter)
            {
                CloseFade();
                yield break;
            }

            battleEntry.EndBattle();
            battleEntry.ResetRewardStack();
            battleEntry.Ready();
        }

        IEnumerator<float> YieldReStartBattle(IBattleInput battleInput)
        {
            if (battleEntry == null)
                yield break;

            if (!battleEntry.IsSkipExit)
                yield return Timing.WaitUntilDone(battleEntry.YieldExitBattle());

            if (isStartFadeIn && !isPlayingFadeIn)
            {
                isPlayingFadeIn = true;
                yield return Timing.WaitUntilDone(UI.Show<UIFade>().YieldFadeIn());
            }

            yield return Timing.WaitUntilDone(battleEntry.YieldEnterBattle(battleInput), TAG);

            // 입장 실패
            if (!battleEntry.IsSuccessEnter)
            {
                CloseFade();
                yield break;
            }

            battleEntry.EndBattle();
            battleEntry.ResetRewardStack();
            battleEntry.Ready();
        }

        /// <summary>
        /// 유닛 등록
        /// </summary>
        bool IBattleManagerImpl.Add(UnitEntity unit, bool isEnemy)
        {
            unit.SetEnemy(isEnemy); // 유닛 타입 세팅

            // 이미 유닛이 추가되어 있음
            if (IsExists(unit))
                return false;

            unit.OnDieNext += OnDie; // 이벤트 등록
            unit.OnDamageValue += OnDamageValue;
            unitList.Add(unit); // 추가
            return true;
        }

        /// <summary>
        /// 유닛 해제
        /// </summary>
        bool IBattleManagerImpl.Remove(UnitEntity unit)
        {
            return UnitRemove(unit);
        }

        /// <summary>
        /// 모든 아군 죽음 여부
        /// </summary>
        bool IBattleManagerImpl.IsAllAlliesDead()
        {
            foreach (var item in unitList)
            {
                // 살아있는 아군이 존재
                if (!item.IsEnemy && !item.IsDie)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 모든 적 죽음 여부
        /// </summary>
        bool IBattleManagerImpl.IsAllEnemyDead()
        {
            foreach (var item in unitList)
            {
                // 살아있는 적군이 존재
                if (item.IsEnemy && !item.IsDie)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 플레이어 죽음 여부
        /// </summary>
        public bool IsPlayerDead()
        {
            // 플레이어가 유닛목록에 없어도 플레이어 죽음으로 간주.
            bool hasPlayer = unitList.Exists(e => e.type == UnitEntityType.Player);
            if (!hasPlayer)
                return true;

            return IsDead(UnitEntityType.Player);
        }

        /// <summary>
        /// 특정 타입 죽음 여부
        /// </summary>
        public bool IsDead(UnitEntityType type)
        {
            foreach (var item in unitList)
            {
                if (item.type == type && item.IsDie)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 적군 유닛 리셋
        /// </summary>
        void IBattleManagerImpl.ResetEnemyUnitList()
        {
            ResetEnemyUnitList();
        }

        public void SetBattleEntry(BattleEntry battleEntry)
        {
            // 현재와 같은 Mode 의 경우
            if (Mode == battleEntry.mode)
                return;

            // 입장까지 성공 후 기존 엔트리 종료
            ResetBattle();

            this.battleEntry = battleEntry;

            // 이벤트 추가
            if (this.battleEntry != null)
                this.battleEntry.Run();

            BattleMode currentMode = Mode;
            OnChangeBattleMode?.Invoke(currentMode); // 전투 모드 변경 이벤트 호출

            switch (this.battleEntry.mode)
            {
                // 프롤로그는 UI 유지
                case BattleMode.Prologue:
                    break;
                case BattleMode.Stage:
                case BattleMode.TimePatrol:
                    LastAutoBattleMode = this.battleEntry.mode;
                    UIManager.Instance.ShortCut(); // 전투 모드 변경 시에 UI들 꺼줌..
                    break;
                default:
                    UIManager.Instance.ShortCut(); // 전투 모드 변경 시에 UI들 꺼줌..
                    break;
            }

            // 스킬 거리 제한 없음
            isAntiChaseSkill = currentMode.IsAntiChaseSkill();
            // 스킬 수동
            isAntiAutoSkill = currentMode.IsAntiAutoSkill();
            // 대미지 독립 처리
            isIndependenceDamage = currentMode.IsIndependenceDamage();
            // 고정 최대 체력
            isFixedMaxHp = currentMode.IsFixedMaxHp();
            // 스킬 포인트 사용
            isUseSkillPoint = currentMode.IsUseSkillPoint();
        }

        void IBattleManagerImpl.InvokePreStart()
        {
            OnPreStart?.Invoke(Mode);
        }

        void IBattleManagerImpl.InvokeStart()
        {
            OnStart?.Invoke(Mode);
        }

        void IBattleManagerImpl.InvokeReady()
        {
            isPlayingFadeIn = false;
            OnReady?.Invoke();
        }

        void IBattleManagerImpl.ReadyAllUnit()
        {
            foreach (var item in unitList)
            {
                UnitActor actor = item.GetActor();

                if (Issue.ACTOR_NULL)
                {
                    if (actor == null)
                    {
                        Debug.LogError($"actor 못찾음 = {item.type}");
                        continue;
                    }
                }

                actor.Show(); // 코드 수정
                actor.AI.ReadyToBattle(); // 준비완료

                if (item.type == UnitEntityType.NormalMonster)
                {
                    actor.EffectPlayer.PlayPanelBuffEffect(); // 몬스터 소환 이펙트
                }
            }
        }

        void IBattleManagerImpl.InvokeEnd()
        {
            OnEnd?.Invoke();
        }

        void IBattleManagerImpl.PauseAllUnit()
        {
            foreach (var item in unitList)
            {
                UnitActor actor = item.GetActor();
                if (actor == null)
                    continue;

                actor.AI.Pause();
                actor.Movement.Pause();
            }
        }

        void IBattleManagerImpl.EndAllUnit()
        {
            ResetEnemyUnitList(); // 모든 적군 리스트 제거

            ResetCupetUnitList(); // 모든 큐펫 제거

            // 아군 AI 종료
            foreach (var item in unitList)
            {
                UnitActor actor = item.GetActor();
                if (actor == null)
                    continue;

                actor.AI.EndAI();
            }
        }

        /// <summary>
        /// 전투 모드 반환
        /// </summary>
        private BattleEntry GetBattleEntry(BattleMode mode)
        {
            if (Mode == mode)
                return battleEntry;

            return BattleEntry.Factory.Create(mode);
        }

        /// <summary>
        /// 유닛 리셋
        /// </summary>
        public void ResetUnitList()
        {
            for (int i = unitList.size - 1; i >= 0; i--)
            {
                unitList[i].DespawnActor(); // Actor Despawn
                UnitRemove(unitList[i]); // Remove
            }
        }

        /// <summary>
        /// 적군 유닛 리셋
        /// </summary>
        private void ResetEnemyUnitList()
        {
            for (int i = unitList.size - 1; i >= 0; i--)
            {
                // 몬스터의 경우
                if (unitList[i].IsEnemy)
                {
                    unitList[i].DespawnActor(); // Actor Despawn
                    UnitRemove(unitList[i]); // Remove
                }
            }
        }

        /// <summary>
        /// 큐펫 유닛 리셋
        /// </summary>
        private void ResetCupetUnitList()
        {
            for (int i = unitList.size - 1; i >= 0; i--)
            {
                // 큐펫의 경우
                if (unitList[i].type == UnitEntityType.PlayerCupet || unitList[i].type == UnitEntityType.MultiCupet || unitList[i].type == UnitEntityType.GhostCupet)
                {
                    unitList[i].DespawnActor(); // Actor Despawn
                    UnitRemove(unitList[i]); // Remove
                }
            }
        }

        /// <summary>
        /// 전투 입장 처리 리셋
        /// </summary>
        private void ResetBattleEntry()
        {
            if (battleEntry == null)
                return;

            battleEntry.ResetProtocolEvent();
            battleEntry.EndBattle(); // 전투 종료
            battleEntry.Destroy(); // Entry 종료
            battleEntry = null; // 해제
        }

        /// <summary>
        /// 유닛 존재 여부
        /// </summary>
        public bool IsExists(UnitEntity unit)
        {
            foreach (var item in unitList)
            {
                if (item.Equals(unit))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 유닛 해제
        /// </summary>
        private bool UnitRemove(UnitEntity unit)
        {
            if (unit == null)
                return false;

            unit.OnDieNext -= OnDie; // 이벤트 해제
            unit.OnDamageValue -= OnDamageValue;
            return unitList.Remove(unit); // 등록 해제
        }

        /// <summary>
        /// 유닛 죽었을 때 호출
        /// </summary>
        void OnDie(UnitEntity unit, UnitEntity attacker)
        {
            if (battleEntry == null)
                return;

            battleEntry.OnDie(unit, attacker);
        }

        void OnDamageValue(UnitEntity unit, UnitEntity attacker, int damage)
        {
            if (battleEntry == null)
                return;

            battleEntry.OnDamageValue(unit, attacker, damage);
        }

        //#if UNITY_EDITOR
        public BattleEntry GetCurrentEntry()
        {
            return battleEntry;
        }
        //#endif

        /// <summary>
        /// 시작하려는 배틀모드 저장 (서버 변경 시 재접속)
        /// </summary>
        /// <param name="battleMode"></param>
        /// <param name="id"></param>
        public void SetSaveBattleMode(BattleMode battleMode, int? id, IBattleInput battleInput)
        {
            if (battleInput != null)
            {
                Debug.Log($"SetSaveBattleMode {battleMode} {battleInput.GetDump()}");
            }
            else if (id.HasValue)
            {
                Debug.Log($"SetSaveBattleMode {battleMode} {id.Value}");
            }
            else
            {
                Debug.Log($"SetSaveBattleMode {battleMode}");
            }
            saveBattleMode = battleMode;
            saveId = id;
            saveBattleInput = battleInput;
        }

        /// <summary>
        /// 서버 변경 이벤트
        /// </summary>
        void OnServerChange(BattleMode pre, BattleMode cur)
        {
            Timing.KillCoroutines(TAG);
            StartFadeIn();
            if (newBattleEntry != null)
            {
                newBattleEntry.OnServerChange();
                newBattleEntry = null;
            }
        }

        public void StartFadeIn(float duration = 0.3f)
        {
            if (!isPlayingFadeIn)
            {
                isPlayingFadeIn = true;
                UI.Show<UIFade>().PlayFadeIn(null, duration);
            }
        }

        void CloseFade()
        {
            isPlayingFadeIn = false;
            UI.Close<UIFade>();
        }

        public void StartSavedMode()
        {
            if (battleEntry != null)
                battleEntry.SetSkipExit();

            if (saveBattleInput != null)
            {
                StartBattle(saveBattleMode, saveBattleInput, isStartFadeIn: false);
            }
            else if (saveId.HasValue)
            {
                StartBattle(saveBattleMode, saveId.Value, isStartFadeIn: false);
            }
            else
            {
                StartBattle(saveBattleMode, isStartFadeIn: false);
            }
        }        
    }
}