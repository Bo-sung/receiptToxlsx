namespace Ragnarok.AI
{
    /// <summary>
    /// 단순 죽음 State
    /// </summary>
    public class DieEmptyState : UnitFsmState
    {
        public DieEmptyState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            actor.Movement.Stop();
            actor.Animator.PlayDie();

            actor.AI.ResetTarget(); // 타겟 초기화 (Group Camera로 인한 움직임 방지)
            actor.AI.SetInputMove(isControl: false); // 이동 상태 취소.
        }
    }
}