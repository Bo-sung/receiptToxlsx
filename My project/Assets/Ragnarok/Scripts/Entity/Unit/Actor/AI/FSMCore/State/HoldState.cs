using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.AI
{
    public class HoldState : UnitFsmState
    {
        private const float CHECK_SKILL_TIME = 1f;

        /// <summary>
        /// 스킬 체크 시간 (버프)
        /// </summary>
        private float checkBuffSkillTime;

        /// <summary>
        /// 스킬 체크 시간 (액티브)
        /// </summary>
        private float checkActiveSkillTime;

        public HoldState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            actor.AI.ResetTarget(); // 타겟 초기화

            CharacterAnimator characterAnimator = actor.Animator as CharacterAnimator;
            if(characterAnimator != null)
                characterAnimator.QueueIdle();
        }

        public override void Update()
        {
            base.Update();

            // 죽음 상태 전환
            if (PreChangeState(Transition.Dead))
                return;

            // 외부 스킬 입력 존재
            if (ProcessInputSkill())
                return;

            // 외부 이동 입력 존재
            if (ProcessInputMove())
                return;

            // 회복 스킬 사용
            if (UseRecoverySkill(actor))
                return;

            // 바인딩 타겟 변경 확인
            if (ProcessChangeBindingTarget(actor))
                return;

            // 타겟이 사라지거나 죽었을 경우
            UnitActor target = actor.AI.Target;
            if (target == null || target.Entity.IsDie)
            {
                actor.AI.ResetTarget(); // 타겟 초기화
            }

            // 타겟이 범위를 벗어났는지 체크
            if (actor.AI.Target != null)
            {
                if (!IsValidTarget(actor.AI.Target, basicAttackSkill: actor.Entity.battleSkillInfo.basicActiveSkill, skills: actor.Entity.battleSkillInfo.GetActiveSkills()))
                    actor.AI.ResetTarget();
            }

            // 타겟이 없다면 범위 내에서 타겟을 탐색 (평타와 스킬 중 최대 거리 범위)
            if (actor.AI.Target is null)
            {
                // 범위 내 가능한 타겟 리스트 추출
                var availableTargets = GetValidTargets(basicAttackSkill: actor.Entity.battleSkillInfo.basicActiveSkill, skills: actor.Entity.battleSkillInfo.GetActiveSkills());

                // 가장 가까운 타겟 선정 
                UnitActor minDistTarget = null;
                float minDist = float.MaxValue;
                foreach (var unit in availableTargets)
                {
                    float dist = actor.GetDistance(unit);
                    if (dist < minDist)
                    {
                        minDistTarget = unit;
                        minDist = dist;
                    }
                }

                actor.AI.SetTarget(minDistTarget);
                return;
            }

            // 스킬 사용
            if (actor.Entity.battleCrowdControlInfo.GetCannotUseSkill())
            {
                // 스킬 사용 불가
            }
            else
            {
                // 버프 스킬 사용
                if (UseBuffSkill(actor))
                    return;

                // 쫒던 타겟에게 스킬 사용
                if (UseActiveSkill(actor, target))
                    return;
            }

            // 평타 사용
            if (actor.Entity.battleCrowdControlInfo.GetCannotUseBasicAttack())
            {
                // 평타 사용 불가
            }
            else
            {
                // 쫓던 타겟에데 평타 스킬 사용
                if (UseBasicActiveSkill(actor, target))
                    return;
            }
        }

        public override void End()
        {
            base.End();
        }

        /// <summary>
        /// 버프 스킬 사용
        /// </summary>
        private bool UseBuffSkill(UnitActor actor)
        {
            if (!actor.AI.isAutoChangeState)
                return false;

            // 버프 스킬 사용 시간 체크 (1초당)
            if (checkBuffSkillTime > Time.realtimeSinceStartup)
                return false;

            checkBuffSkillTime = Time.realtimeSinceStartup + CHECK_SKILL_TIME;

            TargetUnit targetUnit = FindTargetUnit(ActiveSkill.Type.Buff);

            if (targetUnit.IsInvalid())
                return false;

            if (actor.AI.CheckMpCost(targetUnit.selectedSkill))
                return false;

            actor.AI.UseSkill(targetUnit.target, targetUnit.selectedSkill);
            return true;
        }

        /// <summary>
        /// 타겟에게 사용 가능한 스킬 선택
        /// </summary>
        private bool UseActiveSkill(UnitActor actor, UnitActor target)
        {
            if (!actor.AI.isAutoChangeState)
                return false;

            if (checkActiveSkillTime > Time.realtimeSinceStartup)
                return false;

            checkActiveSkillTime = Time.realtimeSinceStartup + CHECK_SKILL_TIME;

            TargetUnit targetUnit = FindTargetUnit(target, ActiveSkill.Type.Attack);

            if (targetUnit.IsInvalid())
                return false;

            if (actor.AI.CheckMpCost(targetUnit.selectedSkill))
                return false;

            actor.AI.UseSkill(targetUnit.target, targetUnit.selectedSkill);
            return true;
        }

        /// <summary>
        /// 타겟에게 평타 스킬 선택
        /// </summary>
        private bool UseBasicActiveSkill(UnitActor actor, UnitActor target)
        {
            TargetUnit targetUnit = FindBasicActiveSkillTarget(target);

            if (targetUnit.IsInvalid())
                return false;

            actor.AI.UseSkill(targetUnit.target, targetUnit.selectedSkill);
            return true;
        }
    }
}