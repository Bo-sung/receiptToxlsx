namespace Ragnarok.AI
{
    /// <summary>
    /// 전투 종료 상태
    /// </summary>
    public class EndState : UnitFsmState
    {
        public EndState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            actor.AI.ResetTarget(); // 타겟 초기화
        }

        public override void End()
        {
            base.End();

            actor.Animator.PlayIdle(); // 기본 자세
            actor.Movement.Stop(); // 이동 멈춤
        }
    }
}