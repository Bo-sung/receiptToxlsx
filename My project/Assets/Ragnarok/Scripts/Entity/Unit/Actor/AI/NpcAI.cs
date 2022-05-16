using Ragnarok.AI;

namespace Ragnarok
{
    public class NpcAI : UnitAI
    {
        protected override FSMSystem MakeFSM()
        {
            return new FSMSystem()

                /* 전투 대기 */
                .AddState(new ReadyState(actor, StateID.Ready)
                    .AddTransition(Transition.StartBattle, StateID.PassiveHold)) // 전투 시작 -> 고정

                /* 전투 종료 */
                .AddState(new EndState(actor, StateID.End)
                    .AddTransition(Transition.StartBattle, StateID.PassiveHold)) // 전투 시작 -> 고정

                /* 고정 */
                .AddState(new PassiveHoldState(actor, StateID.PassiveHold)
                    //.AddTransition(Transition.Dead, StateID.Die) // 죽음 -> 사망
                    .AddTransition(Transition.Dead, StateID.Ready) // 죽음 -> 사망
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태


                /* 사망 */
                .AddState(new DieState(actor, StateID.Die)
                    .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태
        }
    }
}