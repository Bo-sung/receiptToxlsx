namespace Ragnarok.AI
{
    /// <summary>
    /// 기본 자세
    /// </summary>
    public class IdleState : UnitFsmState
    {
        public IdleState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();
            
            actor.AI.ResetTarget(); // 타겟 초기화
        }

        public override void Update()
        {
            base.Update();

            // 죽음 상태 전환
            if (PreChangeState(Transition.Dead))
                return;

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

            // 바인딩 된 타겟이 움직일 경우
            if (IsMoveBindingTarget(actor))
            {
                actor.AI.ChangeState(Transition.MovedBindingTarget); // 바인딩 된 타겟과 멀어짐
                return;
            }
        }
    }
}