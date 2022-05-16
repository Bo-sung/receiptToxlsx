using Ragnarok.AI;

namespace Ragnarok
{
    public class MonsterBotAI : UnitAI
    {
        protected override FSMSystem MakeFSM()
        {
            return new FSMSystem()

                /* 전투대기 */
                .AddState(new ReadyState(actor, StateID.Ready)
                    .AddTransition(Transition.StartBattle, StateID.Idle) // 전투 시작 => 응답대기
                    .AddTransition(Transition.EndBattle, StateID.End) // AI종료 => 전투대기
                    .AddTransition(Transition.Groggy, StateID.Defenseless) // 기절 => 무방비상태
                    .AddTransition(Transition.MoveAround, StateID.Patrol)) // 배회 => 배회

                /* 전투 종료 */
                .AddState(new EndState(actor, StateID.End)
                    .AddTransition(Transition.StartBattle, StateID.Idle) // 전투 시작 => 응답대기
                    .AddTransition(Transition.EndBattle, StateID.End) // AI종료 => 전투대기
                    .AddTransition(Transition.Groggy, StateID.Defenseless) // 기절 => 무방비상태
                    .AddTransition(Transition.MoveAround, StateID.Patrol)) // 배회 => 배회

                /* 무방비상태 */
                .AddState(new ReadyWaitState(actor, StateID.Defenseless) // (깜빡임 상태)
                    .AddTransition(Transition.Finished, StateID.Idle) // 상태종료 => 응답대기
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 응답대기 */
                .AddState(new ResponseWaitState(actor, StateID.Idle)
                    .AddTransition(Transition.Match, StateID.Match) // 전투 매칭 => 매칭
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.Groggy, StateID.Defenseless) // 기절 => 무방비상태
                    .AddTransition(Transition.MoveAround, StateID.Patrol) // 배회 => 배회
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 전투매칭 */
                .AddState(new LoopBasicActiveSkillState(actor, StateID.Match)
                    .AddTransition(Transition.Finished, StateID.Idle) // 상태종료 => 응답대기
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.MoveAround, StateID.Patrol) // 배회 => 배회
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 주변순회 */
                .AddState(new PatrolState(actor, StateID.Patrol)
                    .AddTransition(Transition.Match, StateID.Match) // 전투 매칭 => 매칭
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 죽음 */
                .AddState(new DieEmptyState(actor, StateID.Die)
                    .AddTransition(Transition.Finished, StateID.Idle) // 상태종료 => 응답대기
                    .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태
        }

        public override void ChangeAutoDespawnDieState()
        {
            fsm.DeleteState(StateID.Die);
            fsm.AddState(new AutoDespawnDieState(actor, StateID.Die)
                .AddTransition(Transition.Finished, StateID.Idle) // 상태종료 => 응답대기
                .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태
        }

        public override void ChangeDieEmptyState()
        {
            fsm.DeleteState(StateID.Die);
            fsm.AddState(new DieEmptyState(actor, StateID.Die)
                .AddTransition(Transition.Finished, StateID.Idle) // 상태종료 => 응답대기
                .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태
        }
    }
}