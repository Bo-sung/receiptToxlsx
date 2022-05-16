using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    [System.Obsolete("NavMeshMovement만 쓰임")]
    public class TransformMovement : AdvancedMovement
    {
        /// <summary>
        /// 도달 거리 제한 (움직이고 있을 때, 도착까지 남아있는 거리가 해당 값보다 작을 경우 멈춘다)
        /// </summary>
        const float REMAINING_DISTANCE_LIMIT = 1f;

        Transform myTransform;
        UnitMovement unitMovement;

        [SerializeField] float smoothTime;
        [SerializeField] float speed;
        [SerializeField] float distanceLimit;

        private Vector3 targetPos;
        private bool useRemainThreshold;

        private Vector3 velocity;

        private void Awake()
        {
            myTransform = transform;
            unitMovement = GetComponent<UnitMovement>();
        }

        void Start()
        {
            smoothTime = 0.1f;
            speed = 5f;
        }

        void Update()
        {
            if (IsStopped)
                return;

            Vector3 direction = targetPos - myTransform.position;
            if (IsStopCheck(direction.sqrMagnitude))
            {
                Stop();
                return;
            }

            myTransform.rotation = Quaternion.LookRotation(direction);
            myTransform.position = Vector3.SmoothDamp(myTransform.position, targetPos, ref velocity, smoothTime, speed);

            // 눕는 현상 방지
            Vector3 pos = myTransform.position;
            pos.y = UnitMovement.POSITION_Y;
            myTransform.position = pos;
        }

        public override void Stop()
        {
            IsStopped = true;
            InvokeMoveStop(unitMovement.UnitEntity);
        }

        public override bool SetDestination(Vector3 targetPos, bool useRemainThreshold)
        {
            IsStopped = false;

            this.targetPos = targetPos;
            this.useRemainThreshold = useRemainThreshold;

            InvokeMoveStart();
            return true;
        }

        public override void Move(Vector3 motion)
        {
        }

        public override void Warp(Vector3 targetPos)
        {
            IsStopped = true;

            this.targetPos = targetPos;
            myTransform.position = targetPos;
            myTransform.rotation = Quaternion.identity;
            InvokeWarpEvent();
        }

        public override void SetRotation(Quaternion rotation)
        {
            myTransform.localRotation = rotation;
        }

        public override bool IsLinear(Vector3 targetPos)
        {
            return true;
        }

        public override void SetSpeed(float speed)
        {
            this.speed = speed;
        }

        public override float GetSpeed()
        {
            return speed;
        }

        public override void SetDistanceLimit(float distanceLimit)
        {
            this.distanceLimit = distanceLimit;
        }

        private bool IsStopCheck(float sqrMagnitude)
        {
            if (useRemainThreshold)
                return sqrMagnitude < REMAINING_DISTANCE_LIMIT * REMAINING_DISTANCE_LIMIT;

            //return Mathf.Approximately(sqrMagnitude, 0f);
            return sqrMagnitude < distanceLimit * distanceLimit;
        }

        protected override IEnumerator<float> PlayKnockBack(Vector3 normalizedVector, float power)
        {
            float powerLimit = power;
            float dragWeight = 0f;

            InvokeKnockBackStart();

            while (power > 0f)
            {
                float moveDistance = power * 30f * Time.deltaTime;
                if (moveDistance > powerLimit)
                    moveDistance = powerLimit;
                Vector3 dest = myTransform.position + normalizedVector * moveDistance;
                //dest.x = Mathf.Clamp(dest.x, Constants.Map.MAP_LIMIT_X.x, Constants.Map.MAP_LIMIT_X.y);
                //dest.z = Mathf.Clamp(dest.z, Constants.Map.MAP_LIMIT_Z.x, Constants.Map.MAP_LIMIT_Z.y);

                myTransform.position = dest;

                power -= Constants.Battle.KnockBackDrag * 30f * Time.deltaTime + dragWeight;
                dragWeight += Constants.Battle.DragWeightAdd * 30f * Time.deltaTime;
                yield return Timing.WaitForOneFrame;
            }

            InvokeKnockBackStop();
        }

        protected override IEnumerator<float> PlayRush(Vector3 targetPosition, float rushTime = Constants.Battle.RushTime)
        {
            //  목표 위치가 맵을 벗어나지 않도록 보정
            //targetPosition.x = Mathf.Clamp(targetPosition.x, Constants.Map.MAP_LIMIT_X.x, Constants.Map.MAP_LIMIT_X.y);
            targetPosition.y = Constants.Map.POSITION_Y;
            //targetPosition.z = Mathf.Clamp(targetPosition.z, Constants.Map.MAP_LIMIT_Z.x, Constants.Map.MAP_LIMIT_Z.y);

            float elapsedTime = 0f;

            InvokeRushStart();

            while (true)
            {
                elapsedTime += Time.deltaTime;

                float progressDeg = Mathf.Clamp01(elapsedTime / rushTime);

                float lerp = GetNthOrderLerp(progressDeg, Constants.Battle.RushLerpFunctionOrder);

                Vector3 newPos = Vector3.Lerp(myTransform.position, targetPosition, lerp);
                myTransform.LookAt(newPos);
                myTransform.position = newPos;

                if (progressDeg >= Constants.Battle.RushEndDeg)
                    break;

                yield return Timing.WaitForOneFrame;
            }

            InvokeRushStop();
        }

        /// <summary>
        /// n차 역함수를 그리는 Lerp결과를 반환
        /// </summary>
        /// <param name="progressDeg">0~1 진행도</param>
        /// <param name="NthOrder">역함수의 차수. 1 = 1차역함수</param>
        private float GetNthOrderLerp(float progressDeg, int NthOrder = 1)
        {
            float t = 0f;
            for (int i = 0; i < NthOrder; ++i)
                t = Mathf.Lerp(t, 1f, progressDeg);
            return t;
        }

        /// <summary>
        /// 타겟 좌표 반환
        /// </summary>
        /// <returns></returns>
        public override Vector3 GetTargetPos()
        {
            return this.targetPos;
        }

        /// <summary>
        /// 다른 유닛과의 충돌 무시 여부
        /// </summary>
        public override void SetCollisionIgnoreUnit(bool isIgnoreUnit)
        {
            /// TODO: 거래소에서만 쓰일 것 같아서 여긴 보류 ..
        }

        /// <summary>
        /// 공중에서 낙하.
        /// </summary>
        /// <param name="spawnPosition">스폰 시작 지점</param>
        /// <param name="arrivePosition">스폰 도착 지점 (낙하 지점)</param>
        /// <param name="fallingTime">떨어지는 데에 걸리는 시간</param>
        public override void Fall(Vector3 spawnPosition, Vector3 arrivePosition, float fallingTime)
        {
            /// TODO: ControllerMovement 참고.
        }

        /// <summary>
        /// 지정 방향 쳐다보기.
        /// </summary>
        /// <param name="targetPosition">쳐다볼 대상이 있는 방향</param>
        public override void Look(Vector3 targetPosition)
        {
            myTransform.LookAt(targetPosition);
            // 위아래 각도는 고정.
            myTransform.localEulerAngles = Vector3.up * myTransform.localEulerAngles.y;
        }
    }
}