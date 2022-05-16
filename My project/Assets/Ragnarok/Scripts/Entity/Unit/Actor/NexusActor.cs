namespace Ragnarok
{
    public class NexusActor : UnitActor
    {
        protected override bool CanBeLookTarget(TargetType targetType)
        {
            // 넥서스는 타겟 불가
            return false;
        }
    }
}