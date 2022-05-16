namespace Ragnarok.AI
{
    public class AutoRebirthDieState : UnitFsmState
    {
        RelativeRemainTime rebirthRemainTime;

        public AutoRebirthDieState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            actor.Movement.Stop();
            actor.Animator.PlayDie();

            actor.AI.ResetTarget(); // 타겟 초기화 (Group Camera로 인한 움직임 방지)
            actor.AI.SetInputMove(isControl: false); // 이동 상태 취소.

            rebirthRemainTime = BasisType.UNIT_DEATH_COOL_TIME.GetInt() * 0.001f;

            if (actor is GhostPlayerActor ghostActor) // 부활 타이머 이벤트 발동
            {
                ghostActor.NotifyRemainReviveTime(rebirthRemainTime.GetRemainTime());
            }

        }

        public override void End()
        {
            base.End();

            actor.EffectPlayer.PlayPanelBuffEffect();
        }

        public override void Update()
        {
            base.Update();

            if (rebirthRemainTime.GetRemainTime() > 0f)
                return;

            actor.AI.ChangeState(Transition.Rebirth);
        }
    }
}