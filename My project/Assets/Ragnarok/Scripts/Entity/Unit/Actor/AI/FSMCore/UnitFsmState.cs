using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.AI
{
    public abstract class UnitFsmState : FSMState
    {
        protected readonly UnitActor actor;
        protected readonly BattleManager battleManager;

        public UnitFsmState(UnitActor actor, StateID id) : base(id)
        {
            this.actor = actor;
            battleManager = BattleManager.Instance;
        }

        public override void Begin()
        {
        }

        public override void End()
        {
        }

        public override void Update()
        {
            if (actor.Entity)
                actor.Entity.CheckDurationEffect(); // 지속 효과 체크

            //// 죽음 상태 전환
            //if (PreChangeState(Transition.Dead))
            //    return;
        }

        /// <summary>
        /// 외부 스킬 입력 처리
        /// </summary>
        protected bool ProcessInputSkill()
        {
            // 스킬 사용 불가
            if (actor.Entity.battleCrowdControlInfo.GetCannotUseSkill())
                return false;

            SkillInfo inputSkill = actor.AI.externalInput.GetSelectedSkill();

            if (inputSkill == null)
                return false;

            // 쫓아다니는 적에게 스킬 사용이 가능하지 체크
            UnitActor target = actor.AI.Target;
            TargetUnit targetUnit = FindSkillTarget(target, inputSkill, isInputSkill: true);

            if (targetUnit.IsInvalid())
            {
                // 버프형 스킬 또는 회복형 스킬의 경우에는 타겟을 바꿔서라도 사용
                if (inputSkill.ActiveSkillType == ActiveSkill.Type.RecoveryHp || inputSkill.ActiveSkillType == ActiveSkill.Type.Buff)
                    targetUnit = FindSkillTarget(inputSkill, isInputSkill: true);
            }

            if (targetUnit.IsInvalid())
                return false;

            if (actor.AI.CheckMpCost(targetUnit.selectedSkill))
                return false;

            //// 돌진 스킬일 때, 돌진 가능한 지형인지 체크
            //if (inputSkill.IsRush && !actor.Movement.IsLinear(targetUnit.target.Entity.LastPosition))
            //    return false;

            actor.AI.externalInput.Reset();
            actor.AI.UseSkill(targetUnit.target, targetUnit.selectedSkill); // 스킬 사용
            return true;
        }

        /// <summary>
        /// 외부 터치 입력 처리
        /// </summary>
        protected bool ProcessInputMove()
        {
            // 이동 불가
            if (actor.Entity.battleCrowdControlInfo.GetCannotMove())
                return false;

            return actor.AI.externalInput.GetIsController();
        }

        /// <summary>
        /// 바인딩 타겟 변경 확인
        /// </summary>
        protected bool ProcessChangeBindingTarget(UnitActor actor)
        {
            // 바인딩 Actor 체크
            UnitActor bindingActor = actor.AI.GetBindingActor();

            if (bindingActor == null)
                return false;

            if (bindingActor.Entity.IsDie)
                return false;

            // 바인딩 Actor의 Target
            UnitActor bindingTarget = bindingActor.AI.Target;

            if (bindingTarget == null)
                return false;

            if (bindingTarget.Entity.IsDie)
                return false;

            //// 바인딩 Actor의 Target과 나의 Target 비교
            //if (bindingFollowTarget == actor.AI.FollowTarget)
            //    return false;

            actor.AI.SetTarget(bindingTarget); // 타겟 세팅
            return actor.AI.ChangeState(Transition.ChangeBindingTarget);
        }

        /// <summary>
        /// 바인딩 된 타겟이 움직임
        /// </summary>
        protected bool IsMoveBindingTarget(UnitActor actor)
        {
            UnitActor bindingActor = actor.AI.GetBindingActor();

            // 바인딩 된 타겟이 없거나, 죽었을 경우
            if (bindingActor == null || bindingActor.Entity.IsDie)
                return false;

            return !bindingActor.Movement.IsStopped;
        }

        /// <summary>
        /// 가장 가까운 타겟 바라보기
        /// </summary>
        protected bool ProcessSawTarget(UnitActor actor)
        {
            if (!actor.AI.isAutoChangeState)
                return false;

            UnitActor minTarget;
            if (actor.AI.FixedTarget && !actor.AI.FixedTarget.Entity.IsDie) // 고정 타겟이 존재할 경우
            {
                minTarget = actor.AI.FixedTarget;
            }
            else
            {
                minTarget = battleManager.unitList.FindMinTarget(actor, TargetType.Enemy);
            }

            if (minTarget == null)
                return false;

            actor.AI.SetTarget(minTarget); // 타겟 세팅
            return actor.AI.ChangeState(Transition.SawTarget);
        }

        /// <summary>
        /// 가장 가까운 특정 타입의 타켓
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected bool ProcessSawTarget(UnitActor actor, UnitEntityType type)
        {
            if (!actor.AI.isAutoChangeState)
                return false;

            UnitActor minTarget;
            if (actor.AI.FixedTarget && !actor.AI.FixedTarget.Entity.IsDie) // 고정 타겟이 존재할 경우
            {
                minTarget = actor.AI.FixedTarget;
            }
            else
            {
                minTarget = battleManager.unitList.FindMinTarget(actor, TargetType.Enemy, type);
            }

            if (minTarget == null)
                return false;

            actor.AI.SetTarget(minTarget); // 타겟 세팅
            return actor.AI.ChangeState(Transition.SawTarget);
        }

        /// <summary>
        /// 회복 스킬 사용
        /// </summary>
        protected bool UseRecoverySkill(UnitActor actor)
        {
            // 스킬 사용 불가
            if (actor.Entity.battleCrowdControlInfo.GetCannotUseSkill())
                return false;

            if (!actor.AI.isAutoChangeState)
                return false;

            TargetUnit targetUnit = FindTargetUnit(ActiveSkill.Type.RecoveryHp);

            if (targetUnit.IsInvalid())
                return false;

            if (actor.AI.CheckMpCost(targetUnit.selectedSkill))
                return false;

            actor.AI.UseSkill(targetUnit.target, targetUnit.selectedSkill);
            return true;
        }

        protected TargetUnit FindBasicActiveSkillTarget(UnitActor target)
        {
            // 참조 평타 스킬 반환
            TargetUnit targetUnit = battleManager.unitList.FindSkillTarget(actor, target, ActiveSkill.Type.Attack, skills: actor.Entity.battleSkillInfo.GetExtraBasicActiveSkills());

            if (!targetUnit.IsInvalid())
                return targetUnit;

            return FindSkillTarget(target, actor.Entity.battleSkillInfo.basicActiveSkill, isInputSkill: false);
        }

        protected TargetUnit FindTargetUnit(ActiveSkill.Type condition)
        {
            return FindTargetUnit(target: null, condition);
        }

        protected TargetUnit FindTargetUnit(UnitActor target, ActiveSkill.Type condition)
        {
            return battleManager.unitList.FindSkillTarget(actor, target, condition, skills: actor.Entity.battleSkillInfo.GetActiveSkills());
        }

        protected TargetUnit FindSkillTarget(SkillInfo skillInfo, bool isInputSkill)
        {
            return FindSkillTarget(target: null, skillInfo, isInputSkill);
        }

        protected TargetUnit FindSkillTarget(UnitActor target, SkillInfo skillInfo, bool isInputSkill)
        {
            return battleManager.unitList.FindSkillTarget(actor, target, skillInfo, isInputSkill);
        }

        protected UnitActor[] GetValidTargets()
        {
            return battleManager.unitList.GetValidTargets(actor, actor.Entity.battleSkillInfo.GetActiveSkills());
        }

        protected UnitActor[] GetValidTargets(SkillInfo basicAttackSkill, IEnumerable<SkillInfo> skills)
        {
            return battleManager.unitList.GetValidTargets(actor, basicAttackSkill, skills);
        }

        protected bool IsValidTarget(UnitActor target, SkillInfo basicAttackSkill, IEnumerable<SkillInfo> skills)
        {
            return battleManager.unitList.IsValidTarget(actor, target, basicAttackSkill, skills);
        }

        protected bool PreChangeState(Transition transition)
        {
            if (!HasTransition(transition))
            {
                Debug.LogError($"잘못된 State 바꾸기 시도: {nameof(id)} = {id}, {nameof(transition)} = {transition}");
                return false;
            }

            if (!IsCheck(transition))
                return false;

            actor.AI.ChangeState(transition);
            return true;
        }

        private bool IsCheck(Transition transition)
        {
            switch (transition)
            {
                case Transition.StartBattle:
                    break;
                case Transition.EndBattle:
                    break;
                case Transition.Finished:
                    break;
                case Transition.SelectedUnit:
                    break;
                case Transition.SelectedCube:
                    break;
                case Transition.SelectedSkill:
                    break;
                case Transition.ResetInputSkill:
                    break;
                case Transition.SawTarget:
                    break;
                case Transition.LostTarget:
                    break;
                case Transition.UseSkill:
                    break;
                case Transition.EmptyTarget:
                    break;

                case Transition.Dead:
                    return actor.Entity && actor.Entity.IsDie;

                case Transition.Evade:
                    break;
                case Transition.Rebirth:
                    break;
                case Transition.ChangeBindingTarget:
                    break;
                case Transition.MoveAround:
                    break;
                case Transition.MovedBindingTarget:
                    break;
                default:
                    break;
            }

            return false;
        }
    }
}