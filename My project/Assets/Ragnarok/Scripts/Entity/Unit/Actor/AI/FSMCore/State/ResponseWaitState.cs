namespace Ragnarok.AI
{
    /// <summary>
    /// 응답 대기 상태
    /// </summary>
    public class ResponseWaitState : UnitFsmState
    {
        private const float FIND_DISTANCE = 2f;

        public ResponseWaitState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Update()
        {
            base.Update();

            ProcessCollisionTarget(actor);
        }

        void ProcessCollisionTarget(UnitActor actor)
        {
            UnitActor minTarget = BattleManager.Instance.unitList.FindMinTarget(actor, TargetType.Enemy, FIND_DISTANCE);

            actor.AI.SetCollisionActor(minTarget); // 타겟 세팅
        }
    }
}