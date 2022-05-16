namespace Ragnarok.AI
{
    /// <summary>
    /// 전투 대기 상태
    /// </summary>
    public class ReadyState : UnitFsmState
    {
        public ReadyState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            actor.Animator.PlayIdle(); // 기본 자세
            actor.Movement.Stop(); // 이동 멈춤
        }

        public override void Update()
        {
            base.Update();

            //ChangeState(Transition.StartBattle);
        }
    }
}