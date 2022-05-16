using MEC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Ragnarok
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshMovement : AdvancedMovement
    {
        /// <summary>
        /// 도달 거리 제한 (움직이고 있을 때, 도착까지 남아있는 거리가 해당 값보다 작을 경우 멈춘다)
        /// </summary>
        const float REMAINING_DISTANCE_LIMIT = 1f;

        /// <summary>
        /// Path 찾기 실패 시 Y축 보정 값
        /// </summary>
        const float PATH_FINDING_Y_CORRECTION_INTERVAL = 4.75f;

        /// <summary>
        /// Path 찾기 실패 시 Y축 보정 시도 횟수
        /// </summary>
        const int PATH_FINDING_Y_CORRECTION_ITERATION = 8;

        [SerializeField] float distanceLimit;

        Transform myTransform;
        UnitMovement unitMovement;
        NavMeshAgent agent;
        public NavMeshAgent Agent => agent;

        public override bool enabled
        {
            set
            {
                if (agent != null)
                    agent.enabled = value;
                base.enabled = value;
            }
        }

        [Tooltip("갈 수 없는 곳 Touch 무시")]
        public bool ignoreTouchNotWalkable = false; // 이게 true면 장애물을 넘어갈 수 없음.

        //public override bool IsStopped => !agent.isOnNavMesh || agent.isStopped;

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
            agent = GetComponent<NavMeshAgent>();
            unitMovement = GetComponent<UnitMovement>();

            // NavMeshAgent 기본 세팅
            SetMask(NavMeshArea.Walkable); // NevMesh Area 설정
            SetSpeed(5f);
            agent.angularSpeed = 500f;
            agent.acceleration = 100f;
            agent.autoBraking = false;
            agent.updateRotation = true;
            
            SetObstacleAvoidanceType(ObstacleAvoidanceType.NoObstacleAvoidance);
        }

        void Update()
        {
            if (isFalling)
            {
                UpdateFall();
                return;
            }

            // NavMesh 위에 있지 않을 경우
            if (!agent.isOnNavMesh)
                return;

            // 움직이는 상태가 아닐 경우
            if (IsStopped)
                return;

            if (!hasDestination)
                return;

            // 목적지 계산 중
            if (agent.pathPending)
                return;

            // 돌진기 스킬 중
            if (IsRush)
                return;

            // 남은 거리가 제한 거리보다 작을 경우
            if (IsStopCheck(agent.remainingDistance))
                Stop();
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

        /// <summary>
        /// 이동 멈춤
        /// </summary>
        public override void Stop()
        {
            // Stop Move
            if (!IsStopped)
            {
                IsStopped = true;
                agent.isStopped = true;
                hasDestination = false;
                agent.ResetPath(); // 목적지 초기화

                InvokeMoveStop(unitMovement.UnitEntity);
            }
        }

        /// <summary>
        /// 특정 위치까지 이동
        /// </summary>
        public override bool SetDestination(Vector3 targetPos, bool useRemainThreshold)
        {
            // 갈 수 없는 곳 터치 무시 체크
            if (ignoreTouchNotWalkable)
            {
                if (agent.Raycast(targetPos, out NavMeshHit hit))
                    return false;
            }

            // NavMesh 위에 있지 않을 경우
            if (!agent.isOnNavMesh)
                return false;

            bool isSuccess = FindPath(targetPos); // ToTarget

            if (isSuccess)
            {
                hasDestination = true;
                this.useRemainThreshold = useRemainThreshold;

                // Start Move
                if (IsStopped)
                {
                    IsStopped = false;
                    agent.isStopped = false;

                    InvokeMoveStart();
                }
            }

            return isSuccess;
        }

        /// <summary>
        /// 경로 찾기 (못 찾을 경우 추가 탐색)
        /// </summary>
        private bool FindPath(Vector3 targetPos)
        {
            Vector3? savedTargetPos = null;
            float minDist = float.MaxValue;

            bool isSuccess;
            isSuccess = agent.SetDestination(targetPos); // ToTarget
            if (isSuccess)
            {
                float dist = GetSqrMagnitudeXZ(agent.pathEndPosition, targetPos);
                if (dist < 0.1f)
                    return true;
                savedTargetPos = targetPos;
                minDist = dist;
            }

            // 추가 탐색 시도
            for (int i = 0; i < PATH_FINDING_Y_CORRECTION_ITERATION; ++i)
            {
                Vector3 upTargetPos = targetPos + Vector3.up * PATH_FINDING_Y_CORRECTION_INTERVAL * (i + 1);
                isSuccess = agent.SetDestination(upTargetPos);
                if (isSuccess)
                {
                    float dist = GetSqrMagnitudeXZ(agent.pathEndPosition, upTargetPos);
                    if (dist < 0.1f)
                        return true;

                    if (dist < minDist)
                    {
                        savedTargetPos = upTargetPos;
                        minDist = dist;
                    }
                }


                Vector3 downTargetPos = targetPos + Vector3.down * PATH_FINDING_Y_CORRECTION_INTERVAL * (i + 1);
                isSuccess = agent.SetDestination(downTargetPos);
                if (isSuccess)
                {
                    float dist = GetSqrMagnitudeXZ(agent.pathEndPosition, downTargetPos);
                    if (dist < 0.1f)
                        return true;

                    if (dist < minDist)
                    {
                        savedTargetPos = downTargetPos;
                        minDist = dist;
                    }
                }
            }

            if (savedTargetPos is null)
            {
                Debug.Log("Find Path Failed. " + targetPos);
                return false;
            }

            agent.SetDestination(savedTargetPos.Value);
            return true;
        }

        /// <summary>
        /// XZ축만을 비교했을 때의 두 벡터 사이의 sqrMagnitude
        /// </summary>
        private float GetSqrMagnitudeXZ(Vector3 A, Vector3 B)
        {
            A.y = 0f;
            B.y = 0f;
            return (A - B).sqrMagnitude;
        }

        public override void Move(Vector3 motion)
        {
            if (IsStopped)
            {
                IsStopped = false;
                agent.isStopped = false;
                hasDestination = false;
                agent.ResetPath(); // 목적지 초기화

                InvokeMoveStart();
            }

            myTransform.rotation = Quaternion.LookRotation(motion);
            agent.Move(motion * agent.speed * Time.deltaTime);
        }

        /// <summary>
        /// 특정 위치까지 바로 이동
        /// </summary>
        public override void Warp(Vector3 targetPos)
        {
            //InvokeMoveStop(); // 길드미로에서 애니메이션이 계속 반복되어서 일단 해제 .. 

            IsStopped = true;
            hasDestination = false;

            // NavMesh 위에 있을 경우
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath(); // 목적지 초기화
            }

            agent.Warp(targetPos); // Warp

            InvokeWarpEvent();
        }

        public override void SetRotation(Quaternion rotation)
        {
            myTransform.localRotation = rotation;
        }

        /// <summary>
        /// 특정 위치까지 직선으로 갈 수 있는가? (돌진 가능 여부)
        /// </summary>
        public override bool IsLinear(Vector3 targetPos)
        {
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(targetPos, path))
                return path.corners.Length == 2;

            return false;
        }

        public override void SetSpeed(float speed)
        {
            agent.speed = speed;
        }

        public override float GetSpeed()
        {
            return agent.speed;
        }

        public override void SetDistanceLimit(float distanceLimit)
        {
            this.distanceLimit = distanceLimit;
        }

        private bool IsStopCheck(float remainingDistance)
        {
            if (useRemainThreshold)
                return remainingDistance < REMAINING_DISTANCE_LIMIT;

            //return Mathf.Approximately(remainingDistance, 0f);
            return remainingDistance < distanceLimit;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            // NavMesh 위에 있지 않을 경우
            if (!agent.isOnNavMesh)
                return;

            if (IsStopped)
                return;

            Gizmos.color = Color.blue;
            for (int i = 0; i < agent.path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(agent.path.corners[i], agent.path.corners[i + 1]);
            }
            Gizmos.color = Color.white;
        }
#endif
         
        protected override IEnumerator<float> PlayKnockBack(Vector3 normalizedVector, float power)
        {
            float powerLimit = power;
            float powerScale = Constants.Battle.KNOCKBACK_POWER_SCALE;
            float dragWeight = 0f;

            InvokeKnockBackStart();

            //agent.updatePosition = false;

            while (power > 0f)
            {
                float moveDistance = power * powerScale * Time.deltaTime;
                if (moveDistance > powerLimit)
                    moveDistance = powerLimit;
                Vector3 dest = myTransform.position + normalizedVector * moveDistance;
                //dest.x = Mathf.Clamp(dest.x, Constants.Map.MAP_LIMIT_X.x, Constants.Map.MAP_LIMIT_X.y);
                //dest.z = Mathf.Clamp(dest.z, Constants.Map.MAP_LIMIT_Z.x, Constants.Map.MAP_LIMIT_Z.y);

                //SetSpeed(moveDistance*9999999);
                //agent.SetDestination(dest); // ToTarget
                //Move(dest, false);
                //Warp(dest);
                agent.nextPosition = dest;

                power -= Constants.Battle.KnockBackDrag * powerScale * Time.deltaTime + dragWeight;
                dragWeight += Constants.Battle.DragWeightAdd * powerScale * Time.deltaTime;
                yield return Timing.WaitForOneFrame;
            }

            InvokeKnockBackStop();
        }

        protected override IEnumerator<float> PlayRush(Vector3 targetPosition, float rushTime = Constants.Battle.RushTime)
        {
            //  목표 위치가 맵을 벗어나지 않도록 보정
            //targetPosition.y =  //Constants.Map.POSITION_Y;

            float elapsedTime = 0f;

            IsRush = true;
            InvokeRushStart();

            //agent.updatePosition = false;

            while (true)
            {
                elapsedTime += Time.deltaTime;

                float progressDeg = Mathf.Clamp01(elapsedTime / rushTime);

                float lerp = GetNthOrderLerp(progressDeg, Constants.Battle.RushLerpFunctionOrder);

                Vector3 newPos = Vector3.Lerp(myTransform.position, targetPosition, lerp);
                myTransform.LookAt(newPos);
                //Warp(newPos);
                agent.nextPosition = newPos;

                if (progressDeg >= Constants.Battle.RushEndDeg)
                    break;

                yield return Timing.WaitForOneFrame;
            }

            IsRush = false;
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
        /// <returns></returns>
        public override Vector3 GetTargetPos()
        {
            return agent.destination;
        }

        /// <summary>
        /// 다른 유닛과의 충돌 무시 여부
        /// </summary>
        public override void SetCollisionIgnoreUnit(bool isIgnoreUnit)
        {
            if (agent == null)
                return;

            if (isIgnoreUnit)
            {
                agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
                agent.avoidancePriority = Constants.Trade.TRADESHOP_DEFAULT_AVOIDANCE_PRIORITY;
            }
            else
            {
                agent.avoidancePriority = Constants.Trade.TRADESHOP_OBSTACLE_AVOIDANCE_PRIORITY;
                agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
            }
        }

        /// <summary>
        /// 공중에서 낙하.
        /// </summary>
        /// <param name="spawnPosition">스폰 시작 지점</param>
        /// <param name="arrivePosition">스폰 도착 지점 (낙하 지점)</param>
        /// <param name="fallingTime">떨어지는 데에 걸리는 시간</param>
        public override void Fall(Vector3 spawnPosition, Vector3 arrivePosition, float fallingTime)
        {
            fallingSpawnPosition = spawnPosition;
            this.fallingTime = fallingTime;
            fallingArrivePosition = arrivePosition;

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

        public override void SetAvoidanceRadius(float radius)
        {
            agent.radius = radius;
        }

        public override void SetAvoidancePriority(int priority)
        {
            agent.avoidancePriority = priority;
        }

        public override void SetObstacleAvoidanceType(ObstacleAvoidanceType type)
        {
            agent.obstacleAvoidanceType = type;
        }

        public override void SetMask(NavMeshArea area)
        {
            agent.areaMask = area.ToIntValue();
        }

        public override void AddMask(NavMeshArea area)
        {
            AreaMask mask = agent.areaMask;
            agent.areaMask = mask.Add(area);
        }

        public override void RemoveMask(NavMeshArea area)
        {
            AreaMask mask = agent.areaMask;
            agent.areaMask = mask.Remove(area);
        }
    }
}