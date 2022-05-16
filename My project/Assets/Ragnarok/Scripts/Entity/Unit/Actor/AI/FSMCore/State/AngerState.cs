using UnityEngine;

namespace Ragnarok.AI
{
    public class AngerState : UnitFsmState
    {
        private enum PatternType
        {
            None = 0,
            SpawnSkill = 1,
            ActiveSkill = 2
        }

        private enum TargetingStrategy
        {
            None = 0,
            CurrentTarget = 1,
            Nearest = 2,
            Farthest = 3,
            Weakest = 4,
            Self = 5
        }

        private MonsterData monsterData;
        private RelativeRemainTime angerAnimTimer;
        private bool isAutoBattle; // 입장전투인가?

        public AngerState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            if (!(actor.Entity is MonsterEntity))
            {
                Debug.LogError("AngerState 는 MonsterEntity 전용입니다.");
                actor.AI.ChangeState(Transition.Finished);
                return;
            }

            var monsterEntity = actor.Entity as MonsterEntity;

            if (!monsterEntity.Monster.CanBeAngry)
            {
                Debug.LogError("Anger 상태가 될 수 없는 몬스터가 AgerState 가 되었습니다.");
                actor.AI.ChangeState(Transition.Finished);
                return;
            }

            monsterData = monsterEntity.Monster.GetMonsterData();
            monsterEntity.SetAngryState(true);

            actor.Animator.PlayIdle();
            (actor.EffectPlayer as MonsterEffectPlayer).PlayAngerEffect();

            angerAnimTimer = 1; // 대강 1초라고 가정
            var curEntryMode = BattleManager.Instance.GetCurrentEntry().mode;
            isAutoBattle = curEntryMode == BattleMode.ScenarioMazeBoss || curEntryMode == BattleMode.MultiBossMaze || curEntryMode == BattleMode.MatchMultiMazeBoss;
        }

        public override void Update()
        {
            base.Update();

            // 해당 스킬 애니메이션 중
            if (angerAnimTimer.GetRemainTime() > 0)
                return;

            PatternType patternType = monsterData.pattern_type.ToEnum<PatternType>();

            if (patternType == PatternType.SpawnSkill)
            {
                BattleManager.Instance.GetCurrentEntry().OnMonsterRequestSpawn(actor.Entity as MonsterEntity, monsterData.pattern_value3, monsterData.pattern_value4);
                actor.AI.ChangeState(Transition.Finished);
            }
            else if (patternType == PatternType.ActiveSkill)
            {
                SkillData skillData = SkillDataManager.Instance.Get(monsterData.pattern_value3, 1);

                if(skillData == null)
                {
                    Debug.LogError($"보스 몬스터 패턴 에러= 몬스터ID={monsterData.id}, {nameof(monsterData.pattern_type)}={monsterData.pattern_type}, 없는 스킬 ID={nameof(monsterData.pattern_value3)}({monsterData.pattern_value3}), 레벨=1");
                    return;
                }

                SkillInfo skillInfo = new ActiveSkill();
                skillInfo.SetData(skillData);
                skillInfo.SetSkillRate(10000);

                UnitActor target = null;
                TargetingStrategy targetingStrategy = monsterData.pattern_value4.ToEnum<TargetingStrategy>();

                var curTarget = actor.AI.Target;

                if (targetingStrategy == TargetingStrategy.CurrentTarget && curTarget != null && !curTarget.Entity.IsDie && !curTarget.IsPooled)
                {
                    target = curTarget;
                }
                else
                {
                    if (isAutoBattle)
                    {
                        if (targetingStrategy == TargetingStrategy.Nearest || targetingStrategy == TargetingStrategy.CurrentTarget)
                        {
                            target = battleManager.unitList.FindRandomUnitInRangeGroup(actor, skillInfo.TargetType, AttackType.MeleeAttack);
                            if (target == null)
                                target = battleManager.unitList.FindRandomUnitInRangeGroup(actor, skillInfo.TargetType, AttackType.RangedAttack);
                        }
                        else if (targetingStrategy == TargetingStrategy.Farthest)
                        {
                            target = battleManager.unitList.FindRandomUnitInRangeGroup(actor, skillInfo.TargetType, AttackType.RangedAttack);
                            if (target == null)
                                target = battleManager.unitList.FindRandomUnitInRangeGroup(actor, skillInfo.TargetType, AttackType.MeleeAttack);
                        }
                        else if (targetingStrategy == TargetingStrategy.Weakest)
                        {
                            target = battleManager.unitList.FindWeakestUnit(actor, skillInfo.TargetType);
                        }
                        else
                        {
                            target = actor;
                        }
                    }
                    else
                    {
                        if (targetingStrategy == TargetingStrategy.Nearest || targetingStrategy == TargetingStrategy.CurrentTarget)
                            target = battleManager.unitList.FindMinTarget(actor, skillInfo.TargetType);
                        else if (targetingStrategy == TargetingStrategy.Farthest)
                            target = battleManager.unitList.FindMaxTarget(actor, skillInfo.TargetType);
                        else if (targetingStrategy == TargetingStrategy.Weakest)
                            target = battleManager.unitList.FindWeakestUnit(actor, skillInfo.TargetType);
                        else
                            target = actor;
                    }
                }

                if (target != null)
                    actor.AI.UseSkill(target, skillInfo);
                else
                    actor.AI.ChangeState(Transition.Finished);
            }
        }
    }
}