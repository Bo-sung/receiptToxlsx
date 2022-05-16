using UnityEngine;

namespace Ragnarok.AI
{
    public class SkillState : UnitFsmState
    {
        /// <summary>
        /// 사용중인 애니메이션
        /// </summary>
        private string aniName;
        private RelativeRemainTime attackMotionTimer;

        public SkillState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            UnitActor target = actor.AI.SkillTarget; // 타겟
            SkillInfo skillInfo = actor.AI.UsedSkill; // 타겟을 향해 사용한 스킬

            if (target == null)
            {
                Debug.LogError($"타겟이 존재하지 않는 스킬: InputSkill 처리 필요");
                actor.AI.ChangeState(Transition.Finished); // 상태(스킬) 종료
                return;
            }

            SkillType skillType = skillInfo.SkillType;

            // 스킬 사용 불가
            if ((actor.Entity.battleCrowdControlInfo.GetCannotUseSkill() && skillType == SkillType.Active) || (actor.Entity.battleCrowdControlInfo.GetCannotUseBasicAttack() && skillType == SkillType.BasicActiveSkill))
            {
                actor.AI.ChangeState(Transition.Finished); // 상태(스킬) 종료
                return;
            }

            (float duration, string aniName) = actor.UseSkill(target, skillInfo, isChainableSkill: false);
            attackMotionTimer = duration;

            this.aniName = aniName;
        }

        public override void End()
        {
            base.End();

            attackMotionTimer = 0f;
            aniName = null;
        }

        public override void Update()
        {
            base.Update();

            //// 해당 스킬 애니메이션 중
            //if (actor.Animator.IsPlay(aniName))
            //    return;

            // 스킬 진행 중
            if (attackMotionTimer.GetRemainTime() > 0)
                return;

            // 상태(스킬) 종료
            actor.AI.ChangeState(Transition.Finished);
        }
    }
}