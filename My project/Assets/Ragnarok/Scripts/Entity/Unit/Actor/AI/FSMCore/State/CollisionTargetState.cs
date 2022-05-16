namespace Ragnarok.AI
{
    public class CollisionTargetState : UnitFsmState
    {
        private const float FIND_DISTANCE = 4f;

        public CollisionTargetState(UnitActor actor, StateID id) : base(actor, id)
        {

        }

        public override void Update()
        {
            base.Update();

            ProcessCollisionTarget(actor);
        }

        void ProcessCollisionTarget(UnitActor acter)
        {
            UnitActor minTarget = battleManager.unitList.FindMinTarget(actor, TargetType.Enemy, actor.AI.GetFindDistance());

            if (minTarget is null)
            {
                minTarget = battleManager.unitList.FindMinNPC(actor);
            }

            actor.AI.SetCollisionActor(minTarget); // 타겟 세팅
        }
    }
}