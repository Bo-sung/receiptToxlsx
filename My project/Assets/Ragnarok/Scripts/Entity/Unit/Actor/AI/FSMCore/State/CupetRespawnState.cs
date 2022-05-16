using UnityEngine;

namespace Ragnarok.AI
{
    /// <summary>
    /// 전투 대기 상태
    /// </summary>
    public class CupetRespawnState : UnitFsmState
    {
        float waitStartTime;
        CupetEntity cupetEntity;

        public CupetRespawnState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            cupetEntity = actor.Entity as CupetEntity;

            if (!cupetEntity.IsCupetRespawnable)
            {
                EndState();
                return;
            }

            actor.HideOnSky();
            waitStartTime = Time.realtimeSinceStartup;
        }

        public override void Update()
        {
            base.Update();

            if (Time.realtimeSinceStartup - waitStartTime >= cupetEntity.RespawnDelay)
            {
                actor.AI.WarpToHomePosition();
                actor.AI.ChangeState(Transition.Finished);

                EndState();
            }
        }

        void EndState()
        {
            UnitActor bindingActor = actor.AI.GetBindingActor();
            if (bindingActor)
            {
                actor.AI.SetHomePosition(bindingActor.Entity.LastPosition, isWarp: true);
            }

            actor.AI.ChangeState(Transition.Finished);
        }
    }
}