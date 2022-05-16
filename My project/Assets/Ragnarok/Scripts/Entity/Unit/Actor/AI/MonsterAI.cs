using Ragnarok.AI;

namespace Ragnarok
{
    public class MonsterAI : UnitAI
    {
        protected override FSMSystem MakeFSM()
        {
            return new FSMSystem()

                /* 전투 대기 */
                .AddState(new ReadyState(actor, StateID.Ready)
                    .AddTransition(Transition.StartBattle, StateID.Patrol)) // 전투 시작 => 순찰

                /* 전투 종료 */
                .AddState(new EndState(actor, StateID.End)
                    .AddTransition(Transition.StartBattle, StateID.Patrol)) // 전투 시작 => 순찰

                /* 순찰 */
                .AddState(new PatrolState(actor, StateID.Patrol)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.BeAttacked, StateID.Hit) // 피격 => 피격
                    .AddTransition(Transition.SawTarget, StateID.Chase) // 타겟 발견 => 추적
                    .AddTransition(Transition.BeDetected, StateID.Chase) // 감지 당함 => 추적
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 추적 */
                .AddState(new ChaseState(actor, StateID.Chase)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.BeAttacked, StateID.Hit) // 피격 => 피격
                    .AddTransition(Transition.LostTarget, StateID.Investigate) // 타겟 놓침 => 근처 조사
                    .AddTransition(Transition.UseSkill, StateID.Skill) // 스킬 사용 => 스킬
                    .AddTransition(Transition.BeAngry, StateID.Anger) // AI종료 => 대기 
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
                    .AddTransition(Transition.SawTarget, StateID.Chase) // 타겟 발견 => 추적
                    .AddTransition(Transition.Finished, StateID.Return) // 상태(조사) 종료 => 회귀
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 회귀 */
                .AddState(new ReturnState(actor, StateID.Return)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.BeAttacked, StateID.Hit) // 피격 => 피격
                    .AddTransition(Transition.SawTarget, StateID.Chase) // 타겟 발견 => 추적
                    .AddTransition(Transition.Finished, StateID.Patrol) // 상태(회귀) 종료 => 추적
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 도망 */
                .AddState(new EvadeState(actor, StateID.Evade)
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 죽음 */
                .AddState(new DieState(actor, StateID.Die)
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 승리 */
                .AddState(new VictoryState(actor, StateID.Victory)
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                .AddState(new AngerState(actor, StateID.Anger)
                    .AddTransition(Transition.Dead, StateID.Die)
                    .AddTransition(Transition.Finished, StateID.Chase)
                    .AddTransition(Transition.UseSkill, StateID.Skill)
                    .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태

        }
    }
}