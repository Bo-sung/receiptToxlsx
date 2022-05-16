using UnityEngine;

namespace Ragnarok.AI
{
    public class InvestigateState : UnitFsmState
    {
        private const float MIN_TIME = 1f;
        private const float MAX_TIME = 3f;

        /// <summary>
        /// 조사 시간
        /// </summary>
        private float investigateTime;

        public InvestigateState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            investigateTime = Time.realtimeSinceStartup + Random.Range(MIN_TIME, MAX_TIME);

            // 이동 멈춤
            actor.Movement.Stop();
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

            // 회복 스킬 사용
            if (UseRecoverySkill(actor))
                return;

            // 가장 가까운 타겟 바라보기
            if (ProcessSawTarget(actor))
                return;

            // 조사 시간 종료
            if (investigateTime < Time.realtimeSinceStartup)
            {
                actor.AI.ChangeState(Transition.Finished); // 상태(조사) 종료
                return;
            }

            // TODO 자연스러운 움직임을 주려면 약간의 LookAt 및 Translate 을 준다
        }
    }
}