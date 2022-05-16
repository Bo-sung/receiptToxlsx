using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public abstract class AdvancedMovement : MonoBehaviour, IAdvancedMovement
    {
        public virtual new bool enabled
        {
            set
            {
                base.enabled = value;
            }
        }

        public virtual bool IsStopped { get; protected set; }

        public virtual bool IsRush { get; protected set; }

        public event System.Action OnKnockBackStart;
        public event System.Action OnKnockBackStop;
        public event System.Action OnRushStart;
        public event System.Action OnRushStop;
        public event System.Action OnMoveStart;
        public event System.Action<UnitEntity> OnMoveStop;
        public event System.Action OnWarp;

        public static event System.Action<Transform> OnFallStart;
        public static event System.Action<Transform> OnFallStop;

        public abstract Vector3 GetTargetPos();
        public abstract bool IsLinear(Vector3 targetPos);

        public abstract void Move(Vector3 motion);

        /// <summary>
        /// 넉백
        /// </summary>
        public void KnockBack(Vector3 normalizedVector, float power)
        {
            Timing.RunCoroutine(PlayKnockBack(normalizedVector, power), gameObject);
        }
        protected abstract IEnumerator<float> PlayKnockBack(Vector3 normalizedVector, float power);

        /// <summary>
        /// 특정 위치까지 돌진
        /// </summary>
        public void Rush(Vector3 targetPosition, float rushTime)
        {
            Timing.RunCoroutine(PlayRush(targetPosition, rushTime), gameObject);
        }

        protected abstract IEnumerator<float> PlayRush(Vector3 targetPosition, float rushTime = Constants.Battle.RushTime);

        public abstract void Fall(Vector3 spawnPosition, Vector3 arrivePosition, float fallingTime);
        public abstract void Look(Vector3 targetPosition);

        public abstract void SetCollisionIgnoreUnit(bool isIgnoreUnit);
        public abstract bool SetDestination(Vector3 targetPos, bool useRemainThreshold);
        public abstract void SetSpeed(float speed);
        public abstract float GetSpeed();
        public abstract void SetDistanceLimit(float distanceLimit);
        public abstract void Stop();
        public abstract void Warp(Vector3 targetPos);
        public abstract void SetRotation(Quaternion rotation);

        /// <summary>
        /// NavMeshMovement 전용
        /// </summary>
        public virtual void SetAvoidanceRadius(float radius)
        { 
        }

        /// <summary>
        /// NavMeshMovement 전용
        /// </summary>
        public virtual void SetAvoidancePriority(int priority)
        { 
        }

        /// <summary>
        /// NavMeshMovement 전용
        /// </summary>
        public virtual void SetObstacleAvoidanceType(UnityEngine.AI.ObstacleAvoidanceType type)
        {
        }

        /// <summary>
        /// NavMeshMovement 전용
        /// </summary>
        public virtual void SetMask(NavMeshArea area)
        {
        }

        /// <summary>
        /// NavMeshMovement 전용
        /// </summary>
        public virtual void AddMask(NavMeshArea area)
        {
        }

        /// <summary>
        /// NavMeshMovement 전용
        /// </summary>
        public virtual void RemoveMask(NavMeshArea area)
        {
        }

        protected void InvokeKnockBackStart() { OnKnockBackStart?.Invoke(); }
        protected void InvokeKnockBackStop() { OnKnockBackStop?.Invoke(); }
        protected void InvokeRushStart() { OnRushStart?.Invoke(); }
        protected void InvokeRushStop() { OnRushStop?.Invoke(); }
        protected void InvokeMoveStart() { OnMoveStart?.Invoke(); }
        protected void InvokeMoveStop(UnitEntity unitEntity) { OnMoveStop?.Invoke(unitEntity); }
        protected void InvokeWarpEvent() { OnWarp?.Invoke(); }
        protected void InvokeFallStart() { OnFallStart?.Invoke(transform); }
        protected void InvokeFallStop() { OnFallStop?.Invoke(transform); }
    }
}