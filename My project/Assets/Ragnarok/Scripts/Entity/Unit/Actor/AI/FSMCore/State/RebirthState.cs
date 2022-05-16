namespace Ragnarok.AI
{
    /// <summary>
    /// 부활
    /// </summary>
    public class RebirthState : UnitFsmState
    {
        public RebirthState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            actor.Entity.Rebirth();
        }

        public override void Update()
        {
            base.Update();

            actor.AI.ChangeState(Transition.Finished);
        }
    }
}