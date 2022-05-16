using UnityEngine;

namespace Ragnarok.AI
{
    public class DieState : UnitFsmState
    {
        public const float DISAPPEARE_TIME = 2f;
        float startDisappearTime;

        public DieState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            actor.Movement.Stop();
            actor.Animator.PlayDie();

            actor.AI.ResetTarget(); // 타겟 초기화 (Group Camera로 인한 움직임 방지)
            actor.AI.SetInputMove(isControl: false); // 이동 상태 취소.

            startDisappearTime = Time.realtimeSinceStartup;
        }

        public override void Update()
        {
            base.Update();

            if (actor.Entity.type == UnitEntityType.Player)
            {
                // 캐릭터의 경우에는 BattleManager 에서 Despawn 처리를 한다.
                return;
            }

            if (actor.Entity.type == UnitEntityType.Nexus)
            {
                // 넥서스는 죽은 채로 두고 BattleManager 에서 Despawn 처리를 한다.
                return;
            }

            if (actor.Entity.type == UnitEntityType.Guardian)
            {
                // 수호물은 죽은 채로 둔다.
                return;
            }

            if (Time.realtimeSinceStartup - startDisappearTime >= DISAPPEARE_TIME)
            {
                // [큐펫] 리스폰 하는 경우, Actor를 Despawn하지 않는다. (AI가 멈춤)
                if (actor.Entity.type == UnitEntityType.PlayerCupet || actor.Entity.type == UnitEntityType.MultiCupet || actor.Entity.type == UnitEntityType.GhostCupet)
                {
                    CupetEntity cupetEntity = actor.Entity as CupetEntity;
                    if (cupetEntity.IsCupetRespawnable)
                    {
                        actor.AI.ChangeState(Transition.Respawn);
                        return;
                    }
                }

                actor.Entity.DespawnActor();
            }
        }
    }
}