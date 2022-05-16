namespace Ragnarok
{
    public class NpcActor : UnitActor
    {
        protected override bool CanBeLookTarget(TargetType targetType)
        {
            return true; // TargetType 과는 상관 없이 바라보기 타겟이 된다.
        }
    }
}