namespace Ragnarok.AI
{
    /// <summary>
    /// 평타 반복 연출
    /// </summary>
    public class LoopBasicActiveSkillState : UnitFsmState
    {
        /// <summary>
        /// 사용중인 애니메이션
        /// </summary>
        private string aniName;

        /// <summary>
        /// 평타
        /// </summary>
        private SkillInfo basicActiveSkill;

        public LoopBasicActiveSkillState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            actor.Movement.Stop(); // 이동 정지

            basicActiveSkill = actor.Entity.battleSkillInfo.basicActiveSkill;
            SkillSetting setting = actor.GetSkillSetting(basicActiveSkill);
            aniName = setting.aniName;

            UnitActor target = actor.AI.Target;
            if (target)
            {
                actor.Movement.Look(target.Entity.LastPosition);
            }
        }

        public override void End()
        {
            base.End();

            aniName = null;
            basicActiveSkill = null;
        }

        public override void Update()
        {
            base.Update();

            // 해당 스킬 애니메이션 중
            if (actor.Animator.IsPlay(aniName))
                return;

            actor.Animator.Play(aniName, UnitAnimator.AniType.BasicAttack);

            UnitActor target = actor.AI.Target;
            if (target)
            {
                actor.ShowEffectAndProjectile(target, basicActiveSkill);
            }
        }
    }
}