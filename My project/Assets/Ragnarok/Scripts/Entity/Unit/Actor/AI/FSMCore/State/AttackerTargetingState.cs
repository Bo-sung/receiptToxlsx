namespace Ragnarok.AI
{
    public class AttackerTargetingState : UnitFsmState
    {
        public AttackerTargetingState(UnitActor actor, StateID id) : base(actor, id)
        {
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

            UnitActor attacker = actor.AI.Attacker;

            // 공격자가 없거나 죽었을 경우
            if (attacker == null || attacker.Entity.IsDie)
            {
                actor.AI.ChangeState(Transition.Finished); // 상태(피격) 종료
                return;
            }

            // 무시 타입의 경우 아무것도 하지 않는다.
            if (attacker.Entity.IsIgnoreTarget)
            {
                actor.AI.ChangeState(Transition.Finished); // 상태(피격) 종료
                return;
            }

            // 나와 enemy 타입이 같을 경우 아무것도 하지 않는다
            if (attacker.Entity.IsEnemy && actor.Entity.IsEnemy)
            {
                actor.AI.ChangeState(Transition.Finished); // 상태(피격) 종료
                return;
            }

            // 타겟이 존재하지 않을 경우
            UnitActor follow = actor.AI.Target;
            if (follow == null)
            {
                actor.AI.SetTarget(attacker); // 타겟 세팅
                actor.AI.ChangeState(Transition.SawTarget); // 타겟 바라보기
                return;
            }

            // 타겟과 공격자가 같을 경우
            if (follow.GetInstanceID() == attacker.GetInstanceID())
            {
                actor.AI.ChangeState(Transition.Finished); // 상태(피격) 종료
                return;
            }

            // 공격자의 거리가 쫒고 있는 타겟보다 가까울 때
            if (GetDistance(attacker) < GetDistance(follow))
            {
                actor.AI.SetTarget(attacker); // 타겟 세팅
                actor.AI.ChangeState(Transition.SawTarget); // 타겟 바라보기
                return;
            }

            actor.AI.ChangeState(Transition.Finished); // 상태(피격) 종료
        }

        private float GetDistance(UnitActor target)
        {
            return (actor.Entity.LastPosition - target.Entity.LastPosition).sqrMagnitude;
        }
    }
}