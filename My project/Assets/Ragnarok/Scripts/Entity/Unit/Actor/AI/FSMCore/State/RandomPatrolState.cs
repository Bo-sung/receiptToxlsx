using UnityEngine;

namespace Ragnarok.AI
{
    public class RandomPatrolState : UnitFsmState
    {
        private const float DELAY_MIN = 2f;
        private const float DELAY_MAX = 6f;

        private const int MOVE_TRY_COUNT = 4;

        private const int RATE_LOOK_ENEMY = 6000; // 60.00 % 확률로 적군 쪽으로 이동
        private const int RATE_FOLLOW_ENEMY = 5000; // 50.00 % 확률로 적군 쫓아감

        /// <summary>
        /// 이동 가능 여부
        /// </summary>
        bool canMove;

        private RelativeRemainTime remainTime;

        public RandomPatrolState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            // 이동 가능 여부
            canMove = actor.Animator.CanPlayRun();
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

            if (remainTime.GetRemainTime() > 0f)
                return;

            remainTime = Random.Range(DELAY_MIN, DELAY_MAX);

            // 적군 위치로 순찰
            if (MathUtils.IsCheckPermyriad(RATE_LOOK_ENEMY))
            {
                UnitActor enemy = FindEnemy();

                if (enemy == null || enemy.Entity.IsDie)
                {
                    SetRandomDestination(MOVE_TRY_COUNT);
                }
                else
                {
                    // Chase 로 변경
                    if (MathUtils.IsCheckPermyriad(RATE_FOLLOW_ENEMY))
                    {
                        actor.AI.SetTarget(enemy); // 타겟 세팅
                        actor.AI.ChangeState(Transition.SawTarget);
                    }
                    else
                    {
                        actor.Movement.SetDestination(enemy.Entity.LastPosition);
                    }
                }
            }
            else
            {
                SetRandomDestination(MOVE_TRY_COUNT);
            }
        }

        private UnitActor FindEnemy()
        {
            return battleManager.unitList.FindMinTarget(actor, TargetType.Enemy, -1f);
        }

        private void SetRandomDestination(int input)
        {
            int count = Mathf.Max(1, input);
            for (int i = 0; i < count; i++)
            {
                Vector3 output = actor.AI.GetRandomPatrolPosition();
                if (actor.Movement.SetDestination(output))
                    return;
            }

            // 움직임 실패
        }
    }
}