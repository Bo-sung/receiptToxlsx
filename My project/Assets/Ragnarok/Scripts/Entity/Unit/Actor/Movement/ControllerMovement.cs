using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(CharacterController))]
    [System.Obsolete("NavMeshMovement만 쓰임")]
    public class ControllerMovement : AdvancedMovement
    {
        /// <summary>
        /// 도달 거리 제한 (움직이고 있을 때, 도착까지 남아있는 거리가 해당 값보다 작을 경우 멈춘다)
        /// </summary>
        const float REMAINING_DISTANCE_LIMIT = 1f;

        [SerializeField] float speed;
        [SerializeField] float distanceLimit;

        Transform myTransform;
        CharacterController controller;
        UnitMovement unitMovement;
        BattleManager battleManager;
        public override bool enabled
        {
            set
            {
                if (controller != null)
                    controller.enabled = value;
                base.enabled = value;
            }
        }

        private Vector3 targetPos;
        private bool useRemainThreshold;
        private bool hasDestination;

        // Fall 동작 관련 변수들
        private Vector3 fallingSpawnPosition;
        private Vector3 fallingArrivePosition;
        private bool isFalling;
        private float fallingTime;
        private float fallingElapsedTime;

        void Awake()
        {
            myTransform = transform;
            controller = GetComponent<CharacterController>();
            unitMovement = GetComponent<UnitMovement>();
            battleManager = BattleManager.Instance;
        }

        void Start()
        {
            controller.slopeLimit = 90f; // 경사 못 오름 (유닛끼리 겹침 현상 방지)
            controller.center = Vector3.up * (controller.height * 0.5f);
            controller.radius = 0.7f;
            controller.stepOffset = 0f; // 명시된 값보다 계단이 땅에 가까울 경우에만 캐릭터가 계단을 오릅니다. 이 값은 캐릭터 컨트롤러의 높이보다 커서는 안됩니다. 값이 더 클 경우 오류가 발생합니다.
            controller.minMoveDistance = 0f; // 캐릭터가 지정한 값보다 낮게 움직이려고 할 경우 아예 움직이지 않게 됩니다. 지터링을 줄이기 위해 이 옵션을 사용할 수 있습니다. 대부분의 경우 이 값은 0으로 두어야 합니다.
        }

        void Update()
        {
            if (isFalling)
            {
                UpdateFall();
                return;
            }

            if (IsStopped)
                return;

            if (!hasDestination)
                return;

            Vector3 direction = targetPos - myTransform.position;
            if (IsStopCheck(direction.sqrMagnitude))
            {
                Stop();
                return;
            }

            myTransform.rotation = Quaternion.LookRotation(direction);
            controller.Move(direction.normalized * speed * Time.deltaTime);

            // 눕는 현상 방지
            Vector3 pos = myTransform.position;
            pos.y = UnitMovement.POSITION_Y;
            myTransform.position = pos;
        }

        /// <summary>
        /// 낙하 Update.
        /// </summary>
        private void UpdateFall()
        {
            float normalizedTime = Mathf.Clamp01(fallingElapsedTime / fallingTime);

            //	낙하
            Vector3 bossPosition = Vector3.Lerp(fallingSpawnPosition, fallingArrivePosition, normalizedTime);
            Warp(bossPosition);

            // 낙하 시간 다 채웠으면 낙하 종료.
            if (normalizedTime == 1f)
            {
                isFalling = false;
                InvokeFallStop();
                return;
            }

            fallingElapsedTime += Time.deltaTime;
        }

        public override void Stop()
        {
            if (!IsStopped)
            {
                IsStopped = true;
                hasDestination = false;

                InvokeMoveStop(unitMovement.UnitEntity);
            }
        }

        public override bool SetDestination(Vector3 targetPos, bool useRemainThreshold)
        {
            hasDestination = true;

            this.targetPos = targetPos;
            this.useRemainThreshold = useRemainThreshold;

            if (IsStopped)
            {
                IsStopped = false;
                InvokeMoveStart();
            }

            return true;
        }

        [SerializeField] Vector3 test;

        [ContextMenu("Test")]
        private void Test()
        {
            myTransform.rotation = Quaternion.LookRotation(test);
        }

        public override void Move(Vector3 motion)
        {
            myTransform.rotation = Quaternion.LookRotation(motion);
            controller.Move(motion * speed * Time.deltaTime);

            if (IsStopped)
            {
                IsStopped = false;
                InvokeMoveStart();
            }
        }

        public override void Warp(Vector3 targetPos)
        {
            IsStopped = true;

            this.targetPos = targetPos;
            myTransform.position = targetPos;
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
            float powerScale = Constants.Battle.KNOCKBACK_POWER_SCALE;

            InvokeKnockBackStart();

            while (power > 0f)
            {
                float moveDistance = power * powerScale * Time.deltaTime;
                if (moveDistance > powerLimit)
                    moveDistance = powerLimit;
                Vector3 dest = myTransform.position + normalizedVector * moveDistance;
                //if (battleManager.map != null)
                //{
                //    dest.x = Mathf.Clamp(dest.x, battleManager.map.MoveLimitX.x, battleManager.map.MoveLimitX.y);
                //    dest.z = Mathf.Clamp(dest.z, battleManager.map.MoveLimitZ.x, battleManager.map.MoveLimitZ.y);
                //}
                dest -= myTransform.position;
                dest.y = UnitMovement.POSITION_Y;

                controller.Move(dest);

                // 눕는 현상 방지
                Vector3 pos = myTransform.position;
                pos.y = UnitMovement.POSITION_Y;
                myTransform.position = pos;

                power -= Constants.Battle.KnockBackDrag * powerScale * Time.deltaTime + dragWeight;
                dragWeight += Constants.Battle.DragWeightAdd * powerScale * Time.deltaTime;

                yield return Timing.WaitForOneFrame;
            }

            InvokeKnockBackStop();
        }

        protected override IEnumerator<float> PlayRush(Vector3 targetPosition, float rushTime = Constants.Battle.RushTime)
        {
            float elapsedTime = 0f;

            // 이건 지정 사거리만큼 돌진
            //float dist = Vector3.Distance(targetPosition, cachedTransform.position);
            //targetPosition = (targetPosition - cachedTransform.position) / dist * (rushDistance + REMAINING_DISTANCE_LIMIT) + cachedTransform.position;

            //  목표 위치가 맵을 벗어나지 않도록 보정
            targetPosition.y = Constants.Map.POSITION_Y;

            InvokeRushStart();

            while (true)
            {
                elapsedTime += Time.deltaTime;

                float progressDeg = Mathf.Clamp01(elapsedTime / rushTime);

                float lerp = GetNthOrderLerp(progressDeg, Constants.Battle.RushLerpFunctionOrder);

                Vector3 newPos = Vector3.Lerp(myTransform.position, targetPosition, lerp);
                newPos.y = UnitMovement.POSITION_Y;
                myTransform.LookAt(newPos);
                newPos -= myTransform.position;
                newPos.y = UnitMovement.POSITION_Y;

                controller.Move(newPos);

                // 눕는 현상 방지
                Vector3 pos = myTransform.position;
                pos.y = UnitMovement.POSITION_Y;
                myTransform.position = pos;

                if (progressDeg >= Constants.Battle.RushEndDeg)
                    break;

                if (Vector3.Distance(myTransform.position, targetPosition) <= distanceLimit)
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
            {
                t = Mathf.Lerp(t, 1f, progressDeg);
            }

            return t;
        }

        /// <summary>
        /// 타겟 좌표 반환
        /// </summary>
        public override Vector3 GetTargetPos()
        {
            return targetPos;
        }

        /// <summary>
        /// 다른 유닛과의 충돌 무시 여부
        /// </summary>
        /// <param name="isIgnoreUnit"></param>
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
            this.fallingSpawnPosition = spawnPosition;
            this.fallingTime = fallingTime;
            this.fallingArrivePosition = arrivePosition;

            fallingElapsedTime = 0f;
            isFalling = true;

            UpdateFall();

            InvokeFallStart();
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