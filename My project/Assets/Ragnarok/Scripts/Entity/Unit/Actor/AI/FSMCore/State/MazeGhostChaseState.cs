using UnityEngine;

namespace Ragnarok.AI
{
    public class MazeGhostChaseState : UnitFsmState
    {
        private const float FIND_DISTANCE = 1f;
        private const float moveDelayMin = 2f;
        private const float moveDelayMax = 6f;

        UnitActor player;
        private RelativeRemainTime remainTime;

        Bounds moveLimit;

        public MazeGhostChaseState(UnitActor actor, StateID id) : base(actor, id)
        {
        }


        public override void Begin()
        {
            base.Begin();

            player = Entity.player.GetActor();

            BonusMazeMonsterEntity bonusMazeMonsterEntity = actor.Entity as BonusMazeMonsterEntity;
            if (bonusMazeMonsterEntity)
            {
                moveLimit = bonusMazeMonsterEntity.MoveBounds;
            }

            remainTime = Random.Range(moveDelayMin, moveDelayMax);
        }

        public override void Update()
        {
            base.Update();

            ProcessCollisionTarget(actor);

            if (remainTime.GetRemainTime() > 0f)
                return;

            remainTime = Random.Range(moveDelayMin, moveDelayMax);

            Vector3 dest;

            // 75.00 % 확률로 플레이어 따라옴
            if (MathUtils.IsCheckPermyriad(7500) && player)
            {
                dest = player.Entity.LastPosition;
            }
            else
            {
                dest = new Vector3(Random.Range(moveLimit.min.x, moveLimit.max.x), moveLimit.center.y, Random.Range(moveLimit.min.z, moveLimit.max.z));
            }

            actor.Movement.ForceSetDestination(dest);
        }

        public override void End()
        {
            base.End();

            actor.Movement.Stop();
        }

        void ProcessCollisionTarget(UnitActor actor)
        {
            UnitActor minTarget = BattleManager.Instance.unitList.FindMinTarget(actor, TargetType.Enemy, FIND_DISTANCE);

            actor.AI.SetCollisionActor(minTarget); // 타겟 세팅
        }

    }
}