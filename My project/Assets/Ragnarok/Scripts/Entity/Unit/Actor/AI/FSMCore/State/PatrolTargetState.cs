﻿using UnityEngine;

namespace Ragnarok.AI
{
    public class PatrolTargetState : UnitFsmState
    {
        private const float DISTANCE = 3f;
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

        UnitEntityType type;

        public PatrolTargetState(UnitActor actor, StateID id, UnitEntityType type) : base(actor, id)
        {
            this.type = type;
        }

        public override void Begin()
        {
            base.Begin();

            // 이동 가능 여부
            canMove = actor.Animator.CanPlayRun();

            if (canMove)
                home = actor.CachedTransform.position;
        }

        public override void End()
        {
            base.End();

            stopTime = 0f;
        }

        public override void Update()
        {
            base.Update();

            // 가장 가까운 타겟 바라보기
            if (ProcessSawTarget(actor, type))
                return;

            // 회복 스킬 사용
            if (UseRecoverySkill(actor))
                return;

            // 이동 불가
            if (!canMove)
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

                    // 현재 위치에서 서성거리기
                    Vector3 pos = home + new Vector3(Random.Range(-DISTANCE, DISTANCE), 0f, Random.Range(-DISTANCE, DISTANCE));
                    actor.Movement.SetDestination(pos);
                }
            }
        }


    }
}