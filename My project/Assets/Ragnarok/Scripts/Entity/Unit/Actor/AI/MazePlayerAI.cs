using Ragnarok.AI;

namespace Ragnarok
{
    public class MazePlayerAI : UnitAI
    {
        float distance;

        public override void SetFindDistance(float distance)
        {
            this.distance = distance;
        }

        public override float GetFindDistance()
        {
            return distance;
        }

        public override void StartAI()
        {
            base.StartAI();

            actor.EffectPlayer.ShowUnitCircle();
        }

        protected override FSMSystem MakeFSM()
        {
            return new FSMSystem()

                /* 전투 대기 */
                .AddState(new ReadyState(actor, StateID.Ready)
                    .AddTransition(Transition.StartBattle, StateID.CollisionTarget)) // 전투 시작 => 기본 상태

                /* 전투 종료 */
                .AddState(new EndState(actor, StateID.End)
                    .AddTransition(Transition.StartBattle, StateID.CollisionTarget)) // 전투 시작 => 기본 상태

                /* 기본 자세 */
                .AddState(new CollisionTargetState(actor, StateID.CollisionTarget)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.Hold, StateID.PassiveHold) // 고정 => 고정 (NPC와 대화 등)
                    .AddTransition(Transition.Groggy, StateID.Defenseless) // 행동불능 => 행동불능
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 고정 (NPC와 대화 등) */
                .AddState(new PassiveHoldState(actor, StateID.PassiveHold)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.Finished, StateID.CollisionTarget) // 상태 종료 => 기본
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 전투불능 */
                .AddState(new MazeRandomDefenselessState(actor, StateID.Defenseless)
                    .AddTransition(Transition.Finished, StateID.CollisionTarget) // 상태종료 => 기본
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 죽음 */
                .AddState(new DieState(actor, StateID.Die)
                    .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태
        }

        public override void ChangeFrozenDefenselessState()
        {
            fsm.DeleteState(StateID.Defenseless);
            fsm.AddState(new MazeFrozenDefenselessState(actor, StateID.Defenseless)
                .AddTransition(Transition.Finished, StateID.CollisionTarget) // 상태종료 => 응답대기
                .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태
        }

        public override void ChangeRandomDefenselessState()
        {
            fsm.DeleteState(StateID.Defenseless);
            fsm.AddState(new MazeRandomDefenselessState(actor, StateID.Defenseless)
                .AddTransition(Transition.Finished, StateID.CollisionTarget) // 상태종료 => 응답대기
                .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태
        }
    }
}
