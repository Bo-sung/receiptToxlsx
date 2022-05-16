using Ragnarok.AI;

namespace Ragnarok
{
    public class CharacterAI : UnitAI
    {
        private CharacterEntity characterEntity;

        public override void StartAI()
        {
            base.StartAI();

            //actor.EffectPlayer.ShowUnitCircle();
        }

        public override void OnReady(UnitEntity entity)
        {
            base.OnReady(entity);

            characterEntity = entity as CharacterEntity;

            if (characterEntity.Skill != null)
                RefreshAutoSkill(characterEntity.Skill.IsAutoSkill);
        }

        public override void AddEvent()
        {
            base.AddEvent();

            if (characterEntity)
            {
                if (characterEntity.Skill != null)
                {
                    characterEntity.Skill.OnSkillInit += ResetInputSkill;
                    characterEntity.Skill.OnChangeAutoSkill += RefreshAutoSkill;
                }
            }
        }

        public override void RemoveEvent()
        {
            base.RemoveEvent();

            if (characterEntity)
            {
                if (characterEntity.Skill != null)
                {
                    characterEntity.Skill.OnSkillInit -= ResetInputSkill;
                    characterEntity.Skill.OnChangeAutoSkill -= RefreshAutoSkill;
                }
            }
        }

        protected virtual void RefreshAutoSkill(bool isAutoSkill) { }

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
                    .AddTransition(Transition.SelectedCube, StateID.Move) // 큐브 선택 => 이동
                    .AddTransition(Transition.SelectedUnit, StateID.Chase) // 유닛 선택 => 추적
                    .AddTransition(Transition.UseSkill, StateID.Skill) // 스킬 사용 => 스킬
                    .AddTransition(Transition.SawTarget, StateID.Chase) // 타겟 발견 => 추적
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 이동 */
                .AddState(new MoveState(actor, StateID.Move)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.BeAttacked, StateID.Hit) // 피격 => 피격
                    .AddTransition(Transition.SelectedCube, StateID.Move) // 큐브 선택 => 이동
                    .AddTransition(Transition.SelectedUnit, StateID.Chase) // 유닛 선택 => 추적
                    .AddTransition(Transition.UseSkill, StateID.Skill) // 스킬 사용 => 스킬
                    .AddTransition(Transition.Finished, StateID.Idle) // 상태(이동) 종료 => 기본 자세
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 소속 타겟 쫓아다님 */
                .AddState(new FollowBindingTargetState(actor, StateID.FollowBindingTarget)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.ChangeBindingTarget, StateID.Chase) // 바인딩 된 타겟 변경 => 추적
                    .AddTransition(Transition.Finished, StateID.Idle) // 상태(소속 타겟 쫓아다님) 종료 => 기본 자세
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 추적 */
                .AddState(new ChaseState(actor, StateID.Chase)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.BeAttacked, StateID.Hit) // 피격 => 피격
                    .AddTransition(Transition.SelectedCube, StateID.Move) // 큐브 선택 => 이동
                    .AddTransition(Transition.SelectedUnit, StateID.Chase) // 유닛 선택 => 추적
                    .AddTransition(Transition.UseSkill, StateID.Skill) // 스킬 사용 => 스킬
                    .AddTransition(Transition.LostTarget, StateID.Investigate) // 타겟 놓침 => 근처 조사
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 스킬 */
                .AddState(new SkillState(actor, StateID.Skill)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.SelectedCube, StateID.Move) // 큐브 선택 => 이동
                    .AddTransition(Transition.SelectedUnit, StateID.Chase) // 유닛 선택 => 추적
                    .AddTransition(Transition.Finished, StateID.Chase) // 상태(스킬) 종료 => 추적
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 피격 */
                .AddState(new AttackerTargetingState(actor, StateID.Hit)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.SelectedCube, StateID.Move) // 큐브 선택 => 이동
                    .AddTransition(Transition.SelectedUnit, StateID.Chase) // 유닛 선택 => 추적
                    .AddTransition(Transition.SawTarget, StateID.Chase) // 타겟 발견 => 추적
                    .AddTransition(Transition.Finished, StateID.Chase) // 상태(스킬) 종료 => 추적
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 근처 조사 */
                .AddState(new InvestigateState(actor, StateID.Investigate)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.BeAttacked, StateID.Hit) // 피격 => 피격
                    .AddTransition(Transition.SelectedCube, StateID.Move) // 큐브 선택 => 이동
                    .AddTransition(Transition.SelectedUnit, StateID.Chase) // 유닛 선택 => 추적
                    .AddTransition(Transition.UseSkill, StateID.Skill) // 스킬 사용 => 스킬
                    .AddTransition(Transition.SawTarget, StateID.Chase) // 타겟 발견 => 추적
                    .AddTransition(Transition.Finished, StateID.Idle) // 상태(조사) 종료 => 기본 자세
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 회귀 */
                .AddState(new ReturnState(actor, StateID.Return)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.BeAttacked, StateID.Hit) // 피격 => 피격
                    .AddTransition(Transition.SelectedCube, StateID.Move) // 큐브 선택 => 이동
                    .AddTransition(Transition.SelectedUnit, StateID.Chase) // 유닛 선택 => 추적
                    .AddTransition(Transition.UseSkill, StateID.Skill) // 스킬 사용 => 스킬
                    .AddTransition(Transition.SawTarget, StateID.Chase) // 타겟 발견 => 추적
                    .AddTransition(Transition.Finished, StateID.Idle) // 상태(회귀) 종료 => 기본 자세
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                ///* 도망 */
                //.AddState(new EvadeState(actor, StateID.Evade))

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

        public override void SetFollowBindingTargetState()
        {
            fsm.DeleteState(StateID.Idle);
            fsm.DeleteState(StateID.Chase);

            fsm.AddState(new BindingIdleState(actor, StateID.Idle)
                .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                .AddTransition(Transition.MovedBindingTarget, StateID.FollowBindingTarget)
                .AddTransition(Transition.BeAttacked, StateID.Hit) // 피격 => 피격
                .AddTransition(Transition.SelectedCube, StateID.Move) // 큐브 선택 => 이동
                .AddTransition(Transition.SelectedUnit, StateID.Chase) // 유닛 선택 => 추적
                .AddTransition(Transition.UseSkill, StateID.Skill) // 스킬 사용 => 스킬
                .AddTransition(Transition.SawTarget, StateID.Chase) // 타겟 발견 => 추적
                .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태

            fsm.AddState(new BindingChaseState(actor, StateID.Chase)
                .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                .AddTransition(Transition.MovedBindingTarget, StateID.FollowBindingTarget)
                .AddTransition(Transition.BeAttacked, StateID.Hit) // 피격 => 피격
                .AddTransition(Transition.SelectedCube, StateID.Move) // 큐브 선택 => 이동
                .AddTransition(Transition.SelectedUnit, StateID.Chase) // 유닛 선택 => 추적
                .AddTransition(Transition.UseSkill, StateID.Skill) // 스킬 사용 => 스킬
                .AddTransition(Transition.LostTarget, StateID.Investigate) // 타겟 놓침 => 근처 조사
                .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태
        }

        public override void ResetFollowBindingTargetState()
        {
            fsm.DeleteState(StateID.Idle);
            fsm.DeleteState(StateID.Chase);

            fsm.AddState(new IdleState(actor, StateID.Idle)
                .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                .AddTransition(Transition.BeAttacked, StateID.Hit) // 피격 => 피격
                .AddTransition(Transition.SelectedCube, StateID.Move) // 큐브 선택 => 이동
                .AddTransition(Transition.SelectedUnit, StateID.Chase) // 유닛 선택 => 추적
                .AddTransition(Transition.UseSkill, StateID.Skill) // 스킬 사용 => 스킬
                .AddTransition(Transition.SawTarget, StateID.Chase) // 타겟 발견 => 추적
                .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태

            fsm.AddState(new ChaseState(actor, StateID.Chase)
                .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                .AddTransition(Transition.BeAttacked, StateID.Hit) // 피격 => 피격
                .AddTransition(Transition.SelectedCube, StateID.Move) // 큐브 선택 => 이동
                .AddTransition(Transition.SelectedUnit, StateID.Chase) // 유닛 선택 => 추적
                .AddTransition(Transition.UseSkill, StateID.Skill) // 스킬 사용 => 스킬
                .AddTransition(Transition.LostTarget, StateID.Investigate) // 타겟 놓침 => 근처 조사
                .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태
        }
    }
}