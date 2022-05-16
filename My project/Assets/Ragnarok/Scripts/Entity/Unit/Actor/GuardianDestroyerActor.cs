namespace Ragnarok
{
    public class GuardianDestroyerActor : MonsterActor
    {
        public override bool IsLookTarget(UnitActor other, TargetType targetType)
        {
            //if (!base.IsLookTarget(other, targetType))
            //    return false;

            // 수호자 파괴자는 수호자만 적군으로 인식한다
            switch (targetType)
            {
                case TargetType.Enemy:
                case TargetType.EnemyCharacter:
                case TargetType.EnemyCupet:
                    return (other.Entity.type == UnitEntityType.Guardian);
            }

            return false;
        }
    }
}