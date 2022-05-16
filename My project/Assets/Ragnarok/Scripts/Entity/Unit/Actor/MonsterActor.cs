namespace Ragnarok
{
    public class MonsterActor : UnitActor
    {
        protected override bool CanBeLookTarget(TargetType targetType)
        {
            switch (targetType)
            {
                case TargetType.AlliesCharacter:
                case TargetType.EnemyCharacter:
                    return false; // 몬스터는 캐릭터가 아니다.
            }

            return true; // TargetType 과는 상관 없이 바라보기 타겟이 된다.
        }

        protected override ISkillArea ShowSkillAreaCircle(SkillInfo skillInfo, UnitActor target)
        {
            UnitActor pointTarget = skillInfo.PointType == EffectPointType.Target ? target : this;
            var skillArea = skillInfo.GetSkillArea();
            ISkillArea ret = new EmptySkillArea(skillInfo, Entity.LastPosition, target.Entity.LastPosition);

            if (Entity.type == UnitEntityType.BossMonster || Entity.type == UnitEntityType.MvpMonster || Entity.type == UnitEntityType.WorldBoss || Entity.type == UnitEntityType.TurretBoss)
            {
                if (skillArea > 0f) // -1 인 SkillArea 가 있을 수 있다.
                {
                    bool isAttackSkill = skillInfo.ActiveOptions.HasDamageValue || skillInfo.ActiveOptions.HasCrowdControl; // 공격형 스킬
                    ret = EffectPlayer.ShowSkillAreaCircle(lifeCycle, skillInfo, Entity.LastPosition, target.Entity.LastPosition);
                }
            }

            return ret;
        }
    }
}