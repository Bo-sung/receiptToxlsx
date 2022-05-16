using UnityEngine;

namespace Ragnarok
{
    public interface IMovement
    {
        event System.Action OnMoveStart; // 이동 시작
        event System.Action<UnitEntity> OnMoveStop; // 이동 끝
        event System.Action OnWarp; // 워프

        bool IsStopped { get; }
        bool enabled { set; }


        /// <summary>
        /// 다른 유닛과의 충돌 무시 여부
        /// </summary>
        void SetCollisionIgnoreUnit(bool isIgnoreUnit);

        /// <summary>
        /// 이동 멈춤
        /// </summary>
        void Stop();

        /// <summary>
        /// 특정 위치까지 이동
        /// </summary>
        bool SetDestination(Vector3 targetPos, bool useRemainThreshold);

        /// <summary>
        /// 해당 위치로 이동
        /// </summary>
        void Move(Vector3 motion);

        /// <summary>
        /// 특정 위치까지 바로 이동
        /// </summary>
        void Warp(Vector3 targetPos);

        /// <summary>
        /// 회전 변경
        /// </summary>
        void SetRotation(Quaternion rotation);

        /// <summary>
        /// 특정 위치까지 직선으로 갈 수 있는가? (돌진 가능 여부)
        /// </summary>
        bool IsLinear(Vector3 targetPos);

        /// <summary>
        /// 움직임 속도 반환
        /// </summary>
        float GetSpeed();

        /// <summary>
        /// 움직임 속도 세팅
        /// </summary>
        void SetSpeed(float speed);

        /// <summary>
        /// 도달 거리 제한 세팅
        /// </summary>
        void SetDistanceLimit(float distanceLimit);

        /// <summary>
        /// 이동 타겟 위치
        /// </summary>
        Vector3 GetTargetPos();
    }

    public interface IAdvancedMovement : IMovement
    {
        event System.Action OnKnockBackStart; // 넉백 시작
        event System.Action OnKnockBackStop; // 넉백 끝
        event System.Action OnRushStart; // 돌진 시작
        event System.Action OnRushStop; // 돌진 끝

        /// <summary>
		/// 넉백시키는 함수 (보스 등장 연출 등에 쓰임)
		/// </summary>
		/// <param name="normalizedVector">넉백될 방향 벡터(Normalized)</param>
        void KnockBack(Vector3 normalizedVector, float power);

        /// <summary>
        /// 특정 위치까지 돌진
        /// </summary>
        /// <param name="targetPosition">돌진 목표 위치</param>
        void Rush(Vector3 targetPosition, float rushTime);

        /// <summary>
        /// 공중에서 낙하.
        /// </summary>
        /// <param name="spawnPosition">스폰 시작 지점</param>
        /// <param name="arrivePosition">스폰 도착 지점 (낙하 지점)</param>
        /// <param name="fallingTime">떨어지는 데에 걸리는 시간</param>
        void Fall(Vector3 spawnPosition, Vector3 arrivePosition, float fallingTime);

        /// <summary>
        /// 지정 방향 쳐다보기.
        /// </summary>
        /// <param name="targetPosition">쳐다볼 대상이 있는 방향</param>
        void Look(Vector3 targetPosition);

        /// <summary>
        /// NavMeshMovement 전용
        /// </summary>
        void SetAvoidanceRadius(float radius);

        /// <summary>
        /// NavMeshMovement 전용
        /// </summary>
        void SetAvoidancePriority(int priority);

        /// <summary>
        /// NavMeshMovement 전용
        /// </summary>
        void SetObstacleAvoidanceType(UnityEngine.AI.ObstacleAvoidanceType type);

        /// <summary>
        /// NavMeshMovement 전용
        /// </summary>
        void SetMask(NavMeshArea area);

        /// <summary>
        /// NavMeshMovement 전용
        /// </summary>
        void AddMask(NavMeshArea area);

        /// <summary>
        /// NavMeshMovement 전용
        /// </summary>
        void RemoveMask(NavMeshArea area);
    }
}