namespace Ragnarok.AI
{
    public class AutoDespawnDieState : UnitFsmState
    {
        const float REBIRTH_TIME = 1.8f;

        RelativeRemainTime remainTime;

        public AutoDespawnDieState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            actor.Movement.Stop();
            actor.Animator.PlayDie();

            actor.AI.ResetTarget(); // 타겟 초기화 (Group Camera로 인한 움직임 방지)
            actor.AI.SetInputMove(isControl: false); // 이동 상태 취소.

            remainTime = REBIRTH_TIME;
        }

        public override void Update()
        {
            base.Update();

            if (remainTime.GetRemainTime() > 0f)
                return;

            actor.Entity.DespawnActor();
        }
    }
}