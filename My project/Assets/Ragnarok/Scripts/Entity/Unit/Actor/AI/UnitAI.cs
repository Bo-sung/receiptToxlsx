#define TEST_BUILD

using UnityEngine;
using Ragnarok.AI;
using System.Collections.Generic;
using MEC;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="CharacterAI"/>
    /// <see cref="MonsterAI"/>
    /// <see cref="CupetAI"/>
    /// </summary>
    public abstract class UnitAI : MonoBehaviour, IEntityActorElement<UnitEntity>
    {
        public const int UPDATE_TICK = 6; // 6Tick 마다 호출 (60FPS 기준 0.1초)
        public const int SKILL_TICK = 2;
        int fixedTick;
        bool checkTick;
        StateID lastStateId;

        [SerializeField] UnitActor target;
        [SerializeField] UnitActor fixedTarget;
        [SerializeField] UnitActor follower; // 추격자 (현재: 마지막 추격자만 세팅)
        [SerializeField] UnitActor attacker;
        [SerializeField] UnitActor skillTarget;
        [SerializeField] UnitActor bindingActor;
        [SerializeField] UnitActor collisionActor; // 충돌 액터

        protected UnitActor actor; // Actor
        protected FSMSystem fsm; // State-Machine
        private CoroutineHandle regenHpHandle;
        private CoroutineHandle regenMpHandle;

        System.Action<UnitActor> onFocusTarget;

#if UNITY_EDITOR
        public bool isDebug;
#endif

        /// <summary>
        /// 타겟
        /// </summary>
        public UnitActor Target => target;

        /// <summary>
        /// 고정타겟
        /// </summary>
        public UnitActor FixedTarget => fixedTarget;

        /// <summary>
        /// 추격자 (현재: 마지막 추격자만 저장)
        /// </summary>
        public UnitActor Follower => follower;

        /// <summary>
        /// 공격자
        /// </summary>
        public UnitActor Attacker => attacker;

        /// <summary>
        /// 스킬 타겟
        /// </summary>
        public UnitActor SkillTarget
        {
            get { return skillTarget; }
            private set { skillTarget = value; }
        }

        /// <summary>
        /// 사용한 스킬 정보
        /// </summary>
        public SkillInfo UsedSkill { get; private set; }

        /// <summary>
        /// 맨 처음 위치
        /// </summary>
        public Vector3 HomePos { get; private set; }

        /// <summary>
        /// 현재 AI 상태
        /// </summary>
        public StateID CurrentState => fsm.Current.id;

        /// <summary>
        /// 서성거리는 범위
        /// </summary>
        public float PatrolRange { get; private set; } = 3f;

        /// <summary>
        /// 타겟 변경 시 호출 (등록과 동시에 호출)
        /// </summary>
        public event System.Action<UnitActor> OnFocusTarget
        {
            add
            {
                onFocusTarget += value;
                value(target); // 등록과 동시에 호출
            }
            remove { onFocusTarget -= value; }
        }

        /// <summary>
        /// 타겟 변경 시 호출 (등록과 동시에 호출)
        /// </summary>
        public event System.Action<UnitActor> OnLostFocusTarget;

        public event System.Action<StateID> OnChangedState;

        /// <summary>
        /// 충돌 체크
        /// </summary>
        public event System.Action<UnitActor> OnCollisionActor;

        [System.Obsolete("prototype")]
        public event System.Action<UnitActor, UnitEntity> OnTempCollisionActor;

        /// <summary>
        /// State 자동 변환
        /// </summary>
        public bool isAutoChangeState = true;

        UnitEntity entity;
        public readonly ExternalInput externalInput = new ExternalInput();

        public bool IsPause { get; private set; }

        /// <summary>
        /// 추격자가 있는 적군도 타겟팅 하기 여부
        /// </summary>
        public bool IsLookEnemyContainsFollower { get; private set; }

        /// <summary>
        /// 스킬 사용시 회전 고정 여부
        /// </summary>
        public bool IsLookFixed { get; private set; }

        protected virtual void Awake()
        {
            actor = GetComponent<UnitActor>();
            fsm = MakeFSM();
        }

        protected virtual void Update()
        {
            fixedTick++;

            if (CurrentState != StateID.Skill)
            {
                checkTick = fixedTick < UPDATE_TICK;
            }
            else
            {
                checkTick = fixedTick < SKILL_TICK; // 스킬의 경우도 틱 추가
            }

            if (checkTick)
            {
                if (fsm != null && lastStateId == CurrentState) // State가 변했다면 6틱을 기다리지 않고 즉시 Update한다.
                    return;
            }

#if UNITY_EDITOR
            if (isDebug)
            {
                Debug.LogError(string.Concat("current = ", CurrentState));
            }
#endif

            fixedTick = 0;

            if (fsm != null)
            {
                lastStateId = CurrentState;
                fsm.Current.Update();
            }
        }

        /// <summary>
        /// 처음 위치 세팅
        /// </summary>
        public void SetHomePosition(Vector3 home, bool isWarp)
        {
            HomePos = home; // 처음 위치 세팅

            if (isWarp)
                WarpToHomePosition();
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public void ReadyToBattle()
        {
            if (!actor.Entity.type.IsHideHpMonster())
                actor.EffectPlayer.ShowName(); // 이름 표시

            StartAI();
        }

        /// <summary>
        /// 워프가 필요한 경우, HomePos로 워프
        /// </summary>
        public void WarpToHomePosition()
        {
            actor.Movement.Warp(HomePos); // 이동
        }

        /// <summary>
        /// AI 시작
        /// </summary>
        public virtual void StartAI()
        {
            if (ChangeState(Transition.StartBattle))
                actor.Entity.ReadyToBattle(); // 전투 준비

            ResetTarget(); // 타겟 초기화

            StartHpRegen();
            StartMpRegen();
        }

        /// <summary>
        /// AI 종료
        /// </summary>
        public virtual void EndAI()
        {
            ResetTarget(); // 타겟 초기화

            fixedTarget = null;
            target = null;
            fixedTarget = null;
            // follower = null; // EndAI 가 되더라도 추격자는 null 되지 않는다
            attacker = null;
            skillTarget = null;
            bindingActor = null;
            collisionActor = null;
            UsedSkill = null;
            externalInput.Reset();
            IsPause = false;

            ChangeState(Transition.EndBattle);

            StopHpRegen();
            StopMpRegen();
        }

        public virtual void OnReady(UnitEntity entity)
        {
            this.entity = entity;
        }

        public virtual void OnRelease()
        {
            EndAI();
            RemoveEvent();
        }

        public virtual void AddEvent()
        {
            if (actor && actor.Entity)
            {
                actor.Entity.OnDie += OnDie;
                actor.Entity.OnHit += OnHit;
                externalInput.OnSetSkill += OnSetSkill;
            }
        }

        public virtual void RemoveEvent()
        {
            if (actor && actor.Entity)
            {
                actor.Entity.OnDie -= OnDie;
                actor.Entity.OnHit -= OnHit;
                externalInput.OnSetSkill -= OnSetSkill;
            }
        }

        private void OnSetSkill(SkillInfo skillInfo)
        {
            if (skillInfo == null)
                return;

            if (UsedSkill?.SkillType == SkillType.BasicActiveSkill && !skillInfo.HasRemainCooldownTime() && actor.IsPlayingBasicActiveSkill())
            {
                actor.CancelSkill(); // 기존 스킬 캔슬
                ChangeState(Transition.Finished);
            }
        }

        /// <summary>
        /// State-Macine 조건 변경
        /// </summary>
        public bool ChangeState(Transition trans)
        {
            return fsm.PerformTransition(trans);
        }

        public virtual void SetCollisionActor(UnitActor actor)
        {
            if (GetInstanceID(collisionActor) == GetInstanceID(actor))
                return;

            collisionActor = actor;
            OnCollisionActor?.Invoke(collisionActor);
            OnTempCollisionActor?.Invoke(collisionActor, entity);
        }

        /// <summary>
        /// 타겟 초기화
        /// </summary>
        public void ResetTarget()
        {
            SetTarget(null);
            SetCollisionActor(null);

            if (UsedSkill?.SkillType == SkillType.BasicActiveSkill)
                actor.CancelSkill(); // 기존 스킬 캔슬
        }

#if UNITY_EDITOR
        public void SetTestState()
        {
            fsm.SetState(StateID.Test);
        }
#endif

        public void SetFixedTarget(UnitActor other)
        {
            fixedTarget = other;
            SetTarget(other);
        }

        /// <summary>
        /// 타겟 세팅
        /// </summary>
        public void SetTarget(UnitActor other)
        {
            if (GetInstanceID(target) == GetInstanceID(other))
                return;

            OnLostFocusTarget?.Invoke(target); // 기존 타겟 변경 이벤트 호출

            // 기존 타겟
            if (target)
                target.AI.SetFollower(null); // 기존 타겟의 추격자 Reset

            target = other; // 타겟 세팅

            // 현재 타겟
            if (target)
                target.AI.SetFollower(actor); // 현재 타겟의 초격자 Reset

            onFocusTarget?.Invoke(target); // 타겟 변경 이벤트 호출
        }

        public void StartHpRegen()
        {
            StopHpRegen();
            regenHpHandle = Timing.RunCoroutine(YieldRegenHp());
        }

        public void StopHpRegen()
        {
            Timing.KillCoroutines(regenHpHandle);
        }

        public void StartMpRegen()
        {
            StopMpRegen();
            regenMpHandle = Timing.RunCoroutine(YieldRegenMp());
        }

        public void StopMpRegen()
        {
            Timing.KillCoroutines(regenMpHandle);
        }

        /// <summary>
        /// 추격자 세팅
        /// </summary>
        protected virtual void SetFollower(UnitActor other)
        {
            follower = other;
        }

        /// <summary>
        /// 입력 스킬 입력
        /// </summary>
        public void SetInputSkill(SkillInfo skillInfo)
        {
            if (skillInfo == null)
                return;

            externalInput.SetSkill(skillInfo);
        }

        /// <summary>
        /// 외부 이동 입력
        /// </summary>
        public void SetInputMove(bool isControl)
        {
            ResetTarget();
            externalInput.SetMove(isControl);
        }

        /// <summary>
        /// 외부 이동 상태 반환
        /// </summary>
        /// <returns></returns>
        public bool IsInputMove()
        {
            return externalInput.GetIsController();
        }

        /// <summary>
        /// 입력 스킬 초기화
        /// </summary>
        public void ResetInputSkill()
        {
            externalInput.Reset();
        }

        /// <summary>
        /// 공격
        /// </summary>
        public void UseSkill(UnitActor target, SkillInfo skill)
        {
            if (!fsm.HasTransition(Transition.UseSkill))
                return;

            // 스킬 사용
            SkillTarget = target;
            UsedSkill = skill;
            ChangeState(Transition.UseSkill);
        }

        /// <summary>
        /// 바인딩 Actor 설정
        /// </summary>
        public void SetBindingActor(UnitActor bindingActor)
        {
            this.bindingActor = bindingActor;
        }

        /// <summary>
        /// 바인딩 되어있는 Actor 반환
        /// </summary>
        public UnitActor GetBindingActor()
        {
            if (bindingActor == null)
                return null;

            if (bindingActor.Entity.IsDie)
                return null;

            return bindingActor;
        }

        public void Pause()
        {
            IsPause = true;
        }

        public void Resume()
        {
            IsPause = false;
        }

        /// <summary>
        /// 체력 회복
        /// </summary>
        IEnumerator<float> YieldRegenHp()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(1f);
                actor.Entity.RegenHp();
            }
        }

        /// <summary>
        /// 마나 회복
        /// </summary>
        IEnumerator<float> YieldRegenMp()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(Constants.Battle.REGEN_MP_DELAY);
                actor.Entity.RegenMp();
            }
        }

        /// <summary>
        /// State-Machine 생성
        /// </summary>
        protected abstract FSMSystem MakeFSM();

        protected virtual void OnDie(UnitEntity unit, UnitEntity attacker)
        {
            ChangeState(Transition.Dead);
        }

        protected virtual void OnHit(UnitEntity unit, UnitEntity attacker, int value, int count, bool isCritical, bool isBasicActiveSkill, ElementType elementType, int elementFactor)
        {
            // 오토 스킬의 경우
            if (!isAutoChangeState)
                return;

            if (attacker == null)
                return;

            // 무시 타입의 경우 아무것도 하지 않는다.
            if (attacker.IsIgnoreTarget)
                return;

            // 나와 enemy 타입이 같을 경우 아무것도 하지 않는다
            if (unit.IsEnemy && attacker.IsEnemy)
                return;

            // 고정 타겟이 존재할 경우에는 아무것도 하지 않는다.
            if (fixedTarget && !fixedTarget.Entity.IsDie)
                return;

            // 나를 공격한 유닛으로 인한 추격일 경우 타겟 변경
            UnitActor attackerActor = attacker?.GetActor();
            if (attackerActor && !attacker.IsDie)
            {
                this.attacker = attackerActor;
                ChangeState(Transition.BeAttacked); // 타겟 바라보기
            }

            // 이동 중일 경우에 대한 처리
            //float distanceToAttacker = (actor.LastPosition - attacker.LastPosition).sqrMagnitude; // 공격자와의 거리
            //float distanceToHitPos = (actor.LastPosition - movePos).sqrMagnitude; // 이동지점과의 거리

            //// 공격을 무시하고 멀리 이동하려고 할 때
            //if (distanceToHitPos > distanceToAttacker)
            //    return;

            //actor.AI.ChangeState(Transition.OnSkillHit); // 스킬 피격 받음
        }

        public virtual void SetPatrolPosition(Vector3[] positions)
        {

        }

        public virtual Vector3 GetRandomPatrolPosition()
        {
            return default;
        }

        public virtual bool IsPatrolPosition()
        {
            return default;
        }

        public virtual void SetFindDistance(float distance)
        {

        }

        public virtual float GetFindDistance()
        {
            return 4f;
        }

        /// <summary>
        /// 서성거리 범위 세팅
        /// </summary>
        /// <param name="patrolRange"></param>
        public void SetPatrolRange(float patrolRange)
        {
            PatrolRange = patrolRange;
        }

        private int GetInstanceID(UnitActor actor)
        {
            return actor == null ? 0 : actor.GetInstanceID();
        }

        public virtual void ChangeAutoRebirthDieState()
        {
        }

        public virtual void ChangeAutoDespawnDieState()
        {
        }

        public virtual void ChangeDieEmptyState()
        {
        }

        public virtual void ChangeFrozenDefenselessState()
        {
        }

        public virtual void ChangeRandomDefenselessState()
        {
        }

        public virtual void SetFollowBindingTargetState()
        {
        }

        public virtual void ResetFollowBindingTargetState()
        {
        }

        /// <summary>
        /// 스킬 사용시 MP 체크 여부 (플레이어만 체크)
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckMpCost(SkillInfo skillInfo)
        {
            return false;
        }

        /// <summary>
        /// 추격자가 있는 적군 타겟팅 포함
        /// </summary>
        public void SetLookEnemyContainsFollower()
        {
            IsLookEnemyContainsFollower = true;
        }

        /// <summary>
        /// 추격자가 있는 적군 타겟팅 제외
        /// </summary>
        public void ResetLookEnemyContainsFollower()
        {
            IsLookEnemyContainsFollower = false;
        }

        /// <summary>
        /// 스킬 사용 시 회전 고정
        /// </summary>
        public void SetLookFixed()
        {
            IsLookFixed = true;
        }

        /// <summary>
        /// 스킬 사용 시 회전
        /// </summary>
        public void ResetLookFixed()
        {
            IsLookFixed = false;
        }

#if UNITY_EDITOR

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        void OnDrawGizmos()
        {
            if (actor.Entity == null)
                return;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, actor.Entity.battleUnitInfo.CognizanceDistance);
            Gizmos.color = Color.white;
        }

        private int instanceID;
        private int InstanceID
        {
            get
            {
                if (instanceID == 0)
                    instanceID = GetInstanceID();

                return instanceID;
            }
        }

        private static int currentInstanceID;

        [SerializeField]
        protected bool isShowGUI;
        public bool IsShowGUI { get { return isShowGUI; } }

        protected virtual void OnValidate()
        {
            if (isShowGUI)
                currentInstanceID = InstanceID;
        }

        GUIStyle guiStyle;
        Rect guiRect;

        protected virtual void OnGUI()
        {
            if (currentInstanceID != InstanceID)
                return;

            if (guiStyle == null)
            {
                guiStyle = new GUIStyle(GUI.skin.label);
                guiStyle.normal.textColor = Color.white;
                guiStyle.alignment = TextAnchor.MiddleCenter;

                guiRect = new Rect(10f, 10f, 75f, 50f);
            }

            guiRect = GUI.Window(0, guiRect, DoMyWindow, string.Empty);
        }

        void DoMyWindow(int windowID)
        {
            GUI.Label(new Rect(0, 0, guiRect.width, guiRect.height), fsm.Current.ToString(), guiStyle);
            GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
        }
#endif
    }
}