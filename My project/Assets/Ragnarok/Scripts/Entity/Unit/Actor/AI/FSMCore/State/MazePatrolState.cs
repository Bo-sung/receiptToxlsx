using UnityEngine;

namespace Ragnarok.AI
{
    public class MazePatrolState : UnitFsmState
    {
        private const float STOP_TIME = 1f;

        /// <summary>
        /// 이동 가능 여부
        /// </summary>
        bool canMove;

        /// <summary>
        /// 처음 위치
        /// </summary>
        Vector3 home;

        float stopTime;

        public MazePatrolState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            // [보너스미로 고스트, 시나리오 미로] 고스트인 경우, MazeGhostPatrol State로 전환
            MazeMonsterEntity mazeMonsterEntity = actor.Entity as MazeMonsterEntity;
            if (mazeMonsterEntity)
            {
                MazeMonsterEntity.MazeMonsterType type = mazeMonsterEntity.GetMazeMonsterType();
                if (type == MazeMonsterEntity.MazeMonsterType.Ghost || type == MazeMonsterEntity.MazeMonsterType.Boss)
                {
                    actor.AI.ChangeState(Transition.MoveAround);
                    return;
                }
            }

            // 이동 가능 여부
            canMove = actor.Animator.CanPlayRun();

            if (canMove)
                home = actor.CachedTransform.position;
        }

        public override void End()
        {
            base.End();

            stopTime = 0f;
            actor.EffectPlayer.HideName();
        }

        public override void Update()
        {
            base.Update();

            // 이동 불가
            if (!canMove)
                return;

            if (actor.Entity.IsDie)
            {
                actor.EffectPlayer.HideBattleHUD();
                return;
            }

            UnitActor player = FindPlayer(actor.AI.GetFindDistance());
            if (player)
            {
                actor.EffectPlayer.ShowBattleHUD();
                actor.Movement.Look(player.Entity.LastPosition);
                actor.Movement.Stop();
                return;
            }

            actor.EffectPlayer.HideBattleHUD();

            if (!actor.AI.IsPatrolPosition())
                return;

            // 움직임 종료
            if (actor.Movement.IsStopped)
            {
                // 멈춤 시작
                if (stopTime == 0f)
                {
                    stopTime = Time.realtimeSinceStartup + STOP_TIME + Random.Range(0f, STOP_TIME);
                    return;
                }

                // 멈춤 시간 체크
                if (stopTime < Time.realtimeSinceStartup)
                {
                    stopTime = 0f;
                    Vector3 des = actor.AI.GetRandomPatrolPosition();
                    actor.Movement.SetDestination(des);
                }
            }
        }

        private UnitActor FindPlayer(float cognizanceDistance)
        {
            return battleManager.unitList.FindMinTarget(actor, TargetType.Enemy, UnitEntityType.Player, cognizanceDistance);
        }
    }
}