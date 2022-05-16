using Ragnarok.AI;

namespace Ragnarok
{
    public class TurretAI : MonsterAI
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
            .AddState(new HoldAndActiveSkillState(actor, StateID.Idle)
                .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                .AddTransition(Transition.UseSkill, StateID.Skill) // 스킬 사용 => 스킬
                .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

            /* 스킬 */
            .AddState(new SkillState(actor, StateID.Skill)
                .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                .AddTransition(Transition.Finished, StateID.Idle) // 상태(스킬) 종료 => 추적
                .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

            /* 죽음 */
            .AddState(new DieEmptyState(actor, StateID.Die)
                .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태            
        }
    }
}