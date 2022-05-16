using Ragnarok.AI;
using UnityEngine;

namespace Ragnarok
{
    public class GhostCupetAI : CupetAI
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
                .AddState(new IdleState(actor, StateID.Idle)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.BeAttacked, StateID.Hit) // 피격 => 피격
                    .AddTransition(Transition.UseSkill, StateID.Skill) // 스킬 사용 => 스킬
                    .AddTransition(Transition.SawTarget, StateID.Chase) // 타겟 발견 => 추적
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 추적 */
                .AddState(new ChaseState(actor, StateID.Chase)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.BeAttacked, StateID.Hit) // 피격 => 피격
                    .AddTransition(Transition.UseSkill, StateID.Skill) // 스킬 사용 => 스킬
                    .AddTransition(Transition.LostTarget, StateID.Investigate) // 타겟 놓침 => 근처 조사
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 스킬 */
                .AddState(new SkillState(actor, StateID.Skill)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.Finished, StateID.Chase) // 상태(스킬) 종료 => 추적
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 피격 */
                .AddState(new AttackerTargetingState(actor, StateID.Hit)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.SawTarget, StateID.Chase) // 타겟 발견 => 추적
                    .AddTransition(Transition.Finished, StateID.Chase) // 상태(스킬) 종료 => 추적
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 근처 조사 */
                .AddState(new InvestigateState(actor, StateID.Investigate)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.BeAttacked, StateID.Hit) // 피격 => 피격
                    .AddTransition(Transition.UseSkill, StateID.Skill) // 스킬 사용 => 스킬
                    .AddTransition(Transition.SawTarget, StateID.Chase) // 타겟 발견 => 추적
                    .AddTransition(Transition.Finished, StateID.Idle) // 상태(조사) 종료 => 기본 자세
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 죽음 */
                .AddState(new AutoDespawnDieState(actor, StateID.Die)
                    .AddTransition(Transition.StartBattle, StateID.Idle) // 다시 시작 => 기본 상태
                    .AddTransition(Transition.Finished, StateID.Idle) // 상태종료 => 응답대기
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 승리 */
                .AddState(new VictoryState(actor, StateID.Victory)
                    .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태
        }
    }
}