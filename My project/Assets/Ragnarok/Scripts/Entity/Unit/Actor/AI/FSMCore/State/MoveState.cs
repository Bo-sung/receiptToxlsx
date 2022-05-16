using UnityEngine;

namespace Ragnarok.AI
{
    public class MoveState : UnitFsmState
    {
        private const float STOP_TIME = 0.4f;

        /// <summary>
        /// 멈춤 시간
        /// </summary>
        private float stopTime;

        public MoveState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void End()
        {
            base.End();

            stopTime = 0f;
        }

        public override void Update()
        {
            base.Update();

            // 외부 스킬 입력 존재
            if (ProcessInputSkill())
                return;

            // 외부 이동 입력 존재
            if (ProcessInputMove())
                return;

            // 이동 중
            if (!actor.Movement.IsStopped)
                return;

            // 멈춤 시작
            if (stopTime == 0f)
            {
                stopTime = Time.realtimeSinceStartup + STOP_TIME;
                return;
            }

            // 멈춤 시간 체크
            if (stopTime < Time.realtimeSinceStartup)
            {
                actor.AI.ChangeState(Transition.Finished); // 상태(이동) 종료
                return;
            }
        }
    }
}