namespace Ragnarok
{
    public class CupetActor : UnitActor
    {
        protected override bool CanBeLookTarget(TargetType targetType)
        {
            // 큐펫의 경우 캐릭터 타겟이 될 수 없다.
            switch (targetType)
            {
                case TargetType.AlliesCharacter:
                case TargetType.EnemyCharacter:
                    return false;
            }

            return true;
        }

        protected override bool IsShowDark()
        {
            return Entity.IsEnemy; // 적군의 경우 어둡게
        }
    }
}