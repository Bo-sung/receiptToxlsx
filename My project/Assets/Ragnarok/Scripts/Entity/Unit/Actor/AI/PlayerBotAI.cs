using Ragnarok.AI;

namespace Ragnarok
{
    public class PlayerBotAI : UnitAI
    {
        protected override FSMSystem MakeFSM()
        {
            return new FSMSystem()

                /* 전투대기 */
                .AddState(new ReadyState(actor, StateID.Ready)
                    .AddTransition(Transition.StartBattle, StateID.Idle) // 전투 시작 => 응답대기
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 전투 종료 */
                .AddState(new EndState(actor, StateID.End)
                    .AddTransition(Transition.StartBattle, StateID.Idle) // 전투 시작 => 응답대기
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 응답대기 */
                .AddState(new ResponseWaitState(actor, StateID.Idle)
                    .AddTransition(Transition.Match, StateID.Match) // 전투 매칭 => 매칭
                    .AddTransition(Transition.Groggy, StateID.Defenseless) // 행동불능 => 행동불능
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 전투매칭 */
                .AddState(new LoopBasicActiveSkillState(actor, StateID.Match)
                    .AddTransition(Transition.Finished, StateID.Idle) // 상태종료 => 응답대기
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 행동불능 */
                .AddState(new MazeRandomDefenselessState(actor, StateID.Defenseless)
                    .AddTransition(Transition.Finished, StateID.Idle) // 상태종료 => 응답대기
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 죽음 */
                .AddState(new DieEmptyState(actor, StateID.Die)
                    .AddTransition(Transition.Finished, StateID.Idle) // 상태종료 => 응답대기
                    .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태
        }

        public override void ChangeFrozenDefenselessState()
        {
            fsm.DeleteState(StateID.Defenseless);
            fsm.AddState(new MazeFrozenDefenselessState(actor, StateID.Defenseless)
                .AddTransition(Transition.Finished, StateID.Idle) // 상태종료 => 응답대기
                .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태
        }

        public override void ChangeRandomDefenselessState()
        {
            fsm.DeleteState(StateID.Defenseless);
            fsm.AddState(new MazeRandomDefenselessState(actor, StateID.Defenseless)
                .AddTransition(Transition.Finished, StateID.Idle) // 상태종료 => 응답대기
                .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태
        }
    }
}