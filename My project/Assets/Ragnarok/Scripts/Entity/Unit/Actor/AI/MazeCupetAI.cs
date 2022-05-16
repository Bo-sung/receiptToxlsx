using Ragnarok.AI;

namespace Ragnarok
{
    public class MazeCupetAI : CupetAI
    {
        protected override FSMSystem MakeFSM()
        {
            return new FSMSystem()

                /* 전투 대기 */
                .AddState(new ReadyState(actor, StateID.Ready)
                    .AddTransition(Transition.StartBattle, StateID.FollowBindingTarget)) // 전투 시작 => 소속 타겟 쫓아다님

                /* 전투 종료 */
                .AddState(new EndState(actor, StateID.End)
                    .AddTransition(Transition.StartBattle, StateID.FollowBindingTarget)) // 전투 시작 => 소속 타겟 쫓아다님

                /* 소속 타겟 쫓아다님 */
                .AddState(new MazeFollowBindingTargetState(actor, StateID.FollowBindingTarget)
                    //.AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.BeAttacked, StateID.Hit) // 피격 => 피격
                    .AddTransition(Transition.UseSkill, StateID.Skill) // 스킬 사용 => 스킬
                    .AddTransition(Transition.ChangeBindingTarget, StateID.Chase) // 바인딩 된 타겟 변경 => 추적
                                                                                  //.AddTransition(Transition.Finished, StateID.Idle) // 상태(소속 타겟 쫓아다님) 종료 => 기본 자세
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 기본 자세 */
                .AddState(new IdleState(actor, StateID.Idle)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.BeAttacked, StateID.Hit) // 피격 => 피격
                    .AddTransition(Transition.UseSkill, StateID.Skill) // 스킬 사용 => 스킬
                    .AddTransition(Transition.ChangeBindingTarget, StateID.Chase) // 바인딩 된 타겟 변경 => 추적
                    .AddTransition(Transition.MovedBindingTarget, StateID.FollowBindingTarget) // 바인딩 된 타겟이 움직임 => 바인딩 된 타겟 쫓아다님
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 추적 */
                .AddState(new ChaseState(actor, StateID.Chase)
                    //.AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.BeAttacked, StateID.Hit) // 피격 => 피격
                    .AddTransition(Transition.LostTarget, StateID.FollowBindingTarget) // 타겟 놓침 => 바인딩 된 타겟 쫓아다님
                    .AddTransition(Transition.UseSkill, StateID.Skill) // 스킬 사용 => 스킬
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 스킬 */
                .AddState(new SkillState(actor, StateID.Skill)
                    //.AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.Finished, StateID.Chase) // 상태(스킬) 종료 => 추적
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 피격 */
                .AddState(new AttackerTargetingState(actor, StateID.Hit)
                    //.AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.SawTarget, StateID.Chase) // 타겟 발견 => 추적
                    .AddTransition(Transition.Finished, StateID.Chase) // 상태(스킬) 종료 => 추적
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                ///* 죽음 */
                //.AddState(new DieState(actor, StateID.Die)
                //    .AddTransition(Transition.Rebirth, StateID.Rebirth) // 부활 => 부활
                //    .AddTransition(Transition.EndBattle, StateID.Ready)) // AI종료 => 준비
                //    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

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