using Ragnarok.AI;
using UnityEngine;

namespace Ragnarok
{
    public class GVGMultiPlayerAI : CharacterAI
    {
        protected override FSMSystem MakeFSM()
        {
            return new FSMSystem()

                /* 전투 대기 */
                .AddState(new ReadyState(actor, StateID.Ready)
                    .AddTransition(Transition.StartBattle, StateID.Idle)) // 전투 시작 => 기본 상태

                /* 전투 종료 */
                .AddState(new EndState(actor, StateID.End)
                    .AddTransition(Transition.StartBattle, StateID.Idle)) // 전투 시작 => 기본 상태

                /* 기본 자세 */
                .AddState(new PassiveHoldState(actor, StateID.Idle)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 죽음 */
                .AddState(new DieState(actor, StateID.Die)
                    .AddTransition(Transition.Rebirth, StateID.Rebirth) // 부활 => 부활
                    .AddTransition(Transition.StartBattle, StateID.Idle) // 다시 시작 => 기본 상태
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 부활 */
                .AddState(new RebirthState(actor, StateID.Rebirth)
                    .AddTransition(Transition.Finished, StateID.Idle) // 상태(부활) 종료 => 기본 상태
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 승리 */
                .AddState(new VictoryState(actor, StateID.Victory)
                    .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태
        }
    }
}