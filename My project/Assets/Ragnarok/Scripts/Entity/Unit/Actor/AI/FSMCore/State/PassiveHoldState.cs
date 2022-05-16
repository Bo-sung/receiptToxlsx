using Ragnarok.AI;
using UnityEngine;

namespace Ragnarok
{
    public class PassiveHoldState : UnitFsmState
    {
        public PassiveHoldState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            actor.AI.ResetTarget();
        }

        public override void Update()
        {
            base.Update();

            // 죽음 상태 전환
            if (PreChangeState(Transition.Dead))
                return;
        }

        public override void End()
        {
            base.End();
        }
    }
}