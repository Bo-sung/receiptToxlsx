namespace Ragnarok.AI
{
    public class AutoRebirthFromHomeDieState : UnitFsmState
    {
        private const float REBIRTH_TIME = 2f;

        RelativeRemainTime rebirthRemainTime;

        public AutoRebirthFromHomeDieState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            actor.Movement.Stop();
            actor.Animator.PlayDie();

            actor.AI.ResetTarget(); // 타겟 초기화 (Group Camera로 인한 움직임 방지)
            actor.AI.SetInputMove(isControl: false); // 이동 상태 취소.

            rebirthRemainTime = REBIRTH_TIME;
        }

        public override void End()
        {
            base.End();

            actor.Movement.Warp(actor.AI.HomePos);
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