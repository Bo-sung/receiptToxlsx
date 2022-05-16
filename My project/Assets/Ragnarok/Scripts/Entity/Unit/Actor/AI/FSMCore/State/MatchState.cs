namespace Ragnarok.AI
{
    /// <summary>
    /// 전투 매칭 상태
    /// </summary>
    public class MatchState : UnitFsmState
    {
        public MatchState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            actor.Animator.PlayIdle(); // 기본 자세
            actor.Movement.Stop(); // 이동 멈춤
        }
    }
}