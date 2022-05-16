namespace Ragnarok
{
    public class GhostCupetActor : CupetActor
    {
        public override bool IsLookTarget(UnitActor other, TargetType targetType)
        {
            // 타겟 무시 타입의 경우 타겟이 될 수 없다.
            if (other.Entity.IsIgnoreTarget)
                return false;

            if (AI.IsLookEnemyContainsFollower)
            {
                // 추격자가 있는 적군 포함
            }
            else
            {
                // 추격자가 있는 적군의 경우 타겟이 될 수 없다.
                if (other.Entity.IsEnemy && other.AI.Follower)
                {
                    // 일반 몬스터는 타겟이 될 수 없다.
                    if (other.Entity.type == UnitEntityType.NormalMonster)
                        return false;
                }
            }

            return base.IsLookTarget(other, targetType);
        }
    }
}
