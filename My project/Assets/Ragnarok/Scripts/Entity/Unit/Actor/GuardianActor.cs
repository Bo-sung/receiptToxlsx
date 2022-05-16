namespace Ragnarok
{
    public class GuardianActor : UnitActor
    {
        protected override bool CanBeLookTarget(TargetType targetType)
        {
            // 아군 타겟이 될 수 없다.
            switch (targetType)
            {
                case TargetType.Allies:
                case TargetType.AlliesCharacter:
                case TargetType.AlliesCupet:
                    return false;
            }

            return true;
        }
    }
}