namespace Ragnarok.AI
{
    public class ReturnState : UnitFsmState
    {
        public ReturnState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            // 처음 위치로 이동
            actor.Movement.SetDestination(actor.AI.HomePos);
        }

        public override void Update()
        {
            base.Update();

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

            // 가장 가까운 타겟 바라보기
            if (ProcessSawTarget(actor))
                return;

            // 이동 없음
            if (actor.Movement.IsStopped)
            {
                actor.AI.ChangeState(Transition.Finished); // 상태(이동) 종료
                return;
            }
        }
    }
}