using UnityEngine;

namespace Ragnarok.AI
{
    public class FollowEnemyState : UnitFsmState
    {
        private const float DELAY_MIN = 1f;
        private const float DELAY_MAX = 2f;

        /// <summary>
        /// 이동 가능 여부
        /// </summary>
        bool canMove;

        private RelativeRemainTime remainTime;

        public FollowEnemyState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            // 이동 가능 여부
            canMove = actor.Animator.CanPlayRun();

            remainTime = Random.Range(DELAY_MIN, DELAY_MAX);
        }

        public override void End()
        {
            base.End();

            remainTime = 0f;
        }

        public override void Update()
        {
            base.Update();

            if (actor.AI.IsPause)
            {
                actor.Movement.Stop();
                return;
            }

            // 이동 불가
            if (!canMove)
                return;

            // 타겟이 사라지거나 죽었을 경우
            UnitActor target = actor.AI.Target;
            if (target == null || target.Entity.IsDie || target.IsPooled)
            {
                actor.AI.ResetTarget(); // 타겟 초기화
                actor.AI.ChangeState(Transition.LostTarget); // 타겟 놓침
                return;
            }

            if (remainTime.GetRemainTime() == 0f)
            {
                actor.AI.ResetTarget(); // 타겟 초기화
                actor.AI.ChangeState(Transition.Finished);
                return;
            }

            actor.Movement.SetDestination(target.Entity.LastPosition);
        }       
    }
}