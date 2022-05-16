using UnityEngine;

namespace Ragnarok.AI
{
    public class FollowBindingTargetState : UnitFsmState
    {
        /// <summary>
        /// 바인딩 된 타겟과의 최대 거리
        /// </summary>
        private const float MAX_DISTANCE_TO_BINDING_TARGET = 2f;

        /// <summary>
        /// 대기 시간
        /// </summary>
        private const float STOP_TIME = 2f;

        private bool isMoveToBindingTarget;
        private float stopTime;

        public FollowBindingTargetState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            //MoveToBindingTarget(actor);
        }

        public override void End()
        {
            base.End();

            stopTime = 0f;
            isMoveToBindingTarget = false;
        }

        public override void Update()
        {
            base.Update();

            // 회복 스킬 사용
            if (UseRecoverySkill(actor))
                return;

            // 바인딩 타겟 변경 확인
            if (ProcessChangeBindingTarget(actor))
                return;

            // 가장 가까운 타겟 바라보기
            if (ProcessSawTarget(actor))
                return;

            // 바인딩 된 타겟이 움직일 경우
            if (IsMoveBindingTarget(actor))
            {
                MoveToBindingTarget(actor); // 바인딩 된 타겟으로 이동
                return;
            }

            // 바인딩 된 타겟으로 한 이동한 적이 없을 때
            if (!isMoveToBindingTarget)
            {
                // 멈춤 시작
                if (stopTime == 0f)
                {
                    stopTime = Time.realtimeSinceStartup + STOP_TIME;
                    return;
                }

                // 멈춤 시간 체크
                if (stopTime > Time.realtimeSinceStartup)
                    return;

                MoveToBindingTarget(actor); // 바인딩 된 타겟으로 이동
                return;
            }

            // 이동 없음
            if (isMoveToBindingTarget && actor.Movement.IsStopped)
            {
                actor.AI.ChangeState(Transition.Finished); // 상태(이동) 종료
                return;
            }
        }

        /// <summary>
        /// 바인딩 된 타겟으로 이동
        /// </summary>
        private void MoveToBindingTarget(UnitActor actor)
        {
            isMoveToBindingTarget = true;
            UnitActor bindingActor = actor.AI.GetBindingActor();

            // 바인딩 된 타겟이 없거나, 죽었을 경우
            if (bindingActor == null || bindingActor.Entity.IsDie)
                return;

            Vector3 offset = Vector3.zero;

            if (actor.Entity is CupetEntity cupetEntity)
            {
                offset = GetSquareOffset(0);
            }
            else if (actor.Entity is CharacterEntity)
            {
                offset = GetRoundOffset(actor.GetInstanceID()); // 뒤에 쫓아가기
                //offset = GetFollowOffset(actor, bindingActor); // 뒤에 쫓아가기
            }

            // 바인딩 된 타겟으로 이동
            actor.Movement.SetDestination(bindingActor.Entity.LastPosition + offset, useRemainThreshold: false);
        }

        private Vector3 GetRoundOffset(int pos)
        {
            float rad360 = Mathf.PI * 2;
            float addRad = rad360 / Constants.Size.CUPET_SLOT_SIZE; // 추가 각도
            float x = Mathf.Sin(addRad * pos) * MAX_DISTANCE_TO_BINDING_TARGET;
            float z = Mathf.Cos(addRad * pos) * MAX_DISTANCE_TO_BINDING_TARGET;
            return new Vector3(x, 0, z);
        }

        protected virtual Vector3 GetSquareOffset(int pos)
        {
            switch (pos)
            {
                case 1: return new Vector3(-MAX_DISTANCE_TO_BINDING_TARGET, 0, -MAX_DISTANCE_TO_BINDING_TARGET);
                case 2: return new Vector3(-MAX_DISTANCE_TO_BINDING_TARGET, 0, 0);
                case 3: return new Vector3(-MAX_DISTANCE_TO_BINDING_TARGET, 0, MAX_DISTANCE_TO_BINDING_TARGET);

                case 4: return new Vector3(0, 0, -MAX_DISTANCE_TO_BINDING_TARGET);
                case 5: return new Vector3(0, 0, MAX_DISTANCE_TO_BINDING_TARGET);

                case 6: return new Vector3(MAX_DISTANCE_TO_BINDING_TARGET, 0, -MAX_DISTANCE_TO_BINDING_TARGET);
                case 7: return new Vector3(MAX_DISTANCE_TO_BINDING_TARGET, 0, 0);
                case 8: return new Vector3(MAX_DISTANCE_TO_BINDING_TARGET, 0, MAX_DISTANCE_TO_BINDING_TARGET);
            }

            return Vector3.zero;
        }

        protected virtual Vector3 GetFollowOffset(UnitActor actor, UnitActor bindingActor)
        {
            Vector3 distance = actor.Entity.LastPosition - bindingActor.Entity.LastPosition;
            if (distance.magnitude < MAX_DISTANCE_TO_BINDING_TARGET * MAX_DISTANCE_TO_BINDING_TARGET)
                return distance;

            return distance.normalized * MAX_DISTANCE_TO_BINDING_TARGET;
        }
    }
}