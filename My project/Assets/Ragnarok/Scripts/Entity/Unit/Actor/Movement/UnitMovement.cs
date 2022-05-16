using UnityEngine;
using UnityEngine.AI;

namespace Ragnarok
{
    public class UnitMovement : MonoBehaviour, IEntityActorElement<UnitEntity>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            Physics.IgnoreLayerCollision(Layer.PLAYER, Layer.ENEMY);
            Physics.IgnoreLayerCollision(Layer.CUPET, Layer.ENEMY);
            Physics.IgnoreLayerCollision(Layer.ENEMY, Layer.ENEMY);
            Physics.IgnoreLayerCollision(Layer.ENEMY, Layer.DEFAULT);
            Physics.IgnoreLayerCollision(Layer.PLAYER, Layer.CUPET);
            Physics.IgnoreLayerCollision(Layer.CUPET, Layer.CUPET);
            Physics.IgnoreLayerCollision(Layer.PLAYER, Layer.PLAYER);
            Physics.IgnoreLayerCollision(Layer.MAZE_OTHER_PLAYER, Layer.MAZE_ENEMY);
            Physics.IgnoreLayerCollision(Layer.MAZE_OTHER_PLAYER, Layer.ENEMY);
            Physics.IgnoreLayerCollision(Layer.MAZE_OTHER_PLAYER, Layer.MAZE_OTHER_PLAYER);
            Physics.IgnoreLayerCollision(Layer.MAZE_OTHER_PLAYER, Layer.PLAYER);
            Physics.IgnoreLayerCollision(Layer.PLAYER, Layer.GHOST);
            Physics.IgnoreLayerCollision(Layer.ENEMY, Layer.GHOST);
            Physics.IgnoreLayerCollision(Layer.GHOST, Layer.GHOST);
            Physics.IgnoreLayerCollision(Layer.MAZE_ENEMY, Layer.CUPET);
            Physics.IgnoreLayerCollision(Layer.PLAYER, Layer.WAYPOINT);
            Physics.IgnoreLayerCollision(Layer.CUPET, Layer.WAYPOINT);
            Physics.IgnoreLayerCollision(Layer.MAZE_OTHER_PLAYER, Layer.WAYPOINT);
        }

        public const float POSITION_Y = 2.47f;

        /// <summary>
        /// Movement 모드
        /// </summary>
        public enum Mode
        {
            [System.Obsolete("NavMesh만 쓰임")]
            Controller,
            NavMesh,
        }

        /// <summary>
        /// 현재의 Movement 모드
        /// </summary>
        Mode mode;
        IAdvancedMovement controllerMovement;
        IAdvancedMovement navMeshMovement;

        /// <summary>
        /// 현재 모드에 맞는 movement를 반환
        /// </summary>
        IAdvancedMovement CurrentMovement
        {
            get
            {
                switch (mode)
                {
                    case Mode.Controller:
                        return controllerMovement;

                    case Mode.NavMesh:
                        return navMeshMovement;
                }

                return default;
            }
        }

        public event System.Action OnMoveStart
        {
            add
            {
                controllerMovement.OnMoveStart += value;
                navMeshMovement.OnMoveStart += value;
            }
            remove
            {
                controllerMovement.OnMoveStart -= value;
                navMeshMovement.OnMoveStart -= value;
            }
        }

        public event System.Action<UnitEntity> OnMoveStop
        {
            add
            {
                controllerMovement.OnMoveStop += value;
                navMeshMovement.OnMoveStop += value;
            }
            remove
            {
                controllerMovement.OnMoveStop -= value;
                navMeshMovement.OnMoveStop -= value;
            }
        }

        public event System.Action OnWarp
        {
            add
            {
                controllerMovement.OnWarp += value;
                navMeshMovement.OnWarp += value;
            }
            remove
            {
                controllerMovement.OnWarp -= value;
                navMeshMovement.OnWarp -= value;
            }
        }

        public event System.Action<Vector3> OnMove;

        public event System.Action<GameObject> OnPortal;
        public event System.Action<GameObject> OnPortalOut;
        [System.Obsolete("prototype")]
        public event System.Action<GameObject, UnitEntity> OnTempPortal;
        public event System.Action<IMazeDropItem> OnMazeDropItem;
        [System.Obsolete("prototype")]
        public event System.Action<IMazeDropItem, UnitEntity> OnTempMazeDropItem;
        public event System.Action<WayPointZone, UnitEntity> OnWayPoint;

        public event System.Action OnRushBegin;
        public event System.Action OnRushEnd;
        public event System.Action OnKnockBackBegin;
        public event System.Action OnKnockBackEnd;

        public bool IsStopped => CurrentMovement.IsStopped;

        float savedDefaultSpeed;
        bool isPause;

        private UnitEntity unitEntity;
        public UnitEntity UnitEntity => unitEntity;
        private float defaultSpeed = Constants.Battle.DEFAULT_MOVE_SPEED;

        private bool isLookFixed;

        private NavMeshAgent agent;
        public NavMeshAgent Agent
        {
            get
            {
                if (agent is null)
                    agent = gameObject.AddMissingComponent<NavMeshMovement>().Agent;
                return agent;
            }
        }

        public new bool enabled
        {
            get
            {
                return base.enabled;
            }
            set
            {
                base.enabled = value;
                CurrentMovement.enabled = value;
            }
        }

        protected virtual void Awake()
        {
            controllerMovement = gameObject.AddMissingComponent<ControllerMovement>();
            var navMeshComponent = gameObject.AddMissingComponent<NavMeshMovement>();
            navMeshMovement = navMeshComponent;
            agent = navMeshComponent.Agent;

            SetMode(Mode.NavMesh); // 기본 : NavMesh
        }

        [System.Obsolete("NavMesh Only")]
        public void SetMode(Mode newMode)
        {
            mode = newMode;

            navMeshMovement.enabled = mode == Mode.NavMesh;
        }

        public virtual void OnReady(UnitEntity entity)
        {
            unitEntity = entity;
            SetSpeedRate(unitEntity.MoveSpeedRate);
            SetDistanceLimit(Constants.Battle.SMALL_REMAINING_DISTANCE_LIMIT);

            controllerMovement.OnRushStart += OnRushStart;
            navMeshMovement.OnRushStart += OnRushStart;
            controllerMovement.OnRushStop += OnRushStop;
            navMeshMovement.OnRushStop += OnRushStop;

            controllerMovement.OnKnockBackStart += OnKnockBackStart;
            navMeshMovement.OnKnockBackStart += OnKnockBackStart;
            controllerMovement.OnKnockBackStop += OnKnockBackStop;
            navMeshMovement.OnKnockBackStop += OnKnockBackStop;

            Resume();
        }

        public virtual void OnRelease()
        {
            unitEntity = null;

            controllerMovement.OnRushStart -= OnRushStart;
            navMeshMovement.OnRushStart -= OnRushStart;
            controllerMovement.OnRushStop -= OnRushStop;
            navMeshMovement.OnRushStop -= OnRushStop;

            controllerMovement.OnKnockBackStart -= OnKnockBackStart;
            navMeshMovement.OnKnockBackStart -= OnKnockBackStart;
            controllerMovement.OnKnockBackStop -= OnKnockBackStop;
            navMeshMovement.OnKnockBackStop -= OnKnockBackStop;
        }

        public virtual void AddEvent()
        {
            if (unitEntity)
            {
                unitEntity.OnChangeMoveSpeedRate += SetSpeedRate;
            }
        }

        public virtual void RemoveEvent()
        {
            if (unitEntity)
            {
                unitEntity.OnChangeMoveSpeedRate -= SetSpeedRate;
            }
        }

        void OnRushStart()
        {
            UnitActor actor = unitEntity.GetActor();
            actor?.EffectPlayer.PlayRushEffect();

            OnRushBegin?.Invoke();
        }

        void OnRushStop()
        {
            OnRushEnd?.Invoke();
        }

        void OnKnockBackStart()
        {
            OnKnockBackBegin?.Invoke();
        }

        void OnKnockBackStop()
        {
            OnKnockBackEnd?.Invoke();
        }

        public void Look(Vector3 pos)
        {
            CurrentMovement.Look(pos);
        }

        public void Stop()
        {
            CurrentMovement.Stop();
        }

        public Vector3 GetLastPosition()
        {
            return unitEntity.LastPosition;
        }

        public bool SetDestination(Vector3 targetPos)
        {
            return SetDestination(targetPos, useRemainThreshold: true);
        }

        public bool ForceSetDestination(Vector3 targetPos)
        {
            return ForceSetDestination(targetPos, useRemainThreshold: true);
        }

        public bool ForceSetDestination(Vector3 targetPos, bool useRemainThreshold)
        {
            return CurrentMovement.SetDestination(targetPos, useRemainThreshold);
        }

        public bool SetDestination(Vector3 targetPos, bool useRemainThreshold, bool fixedYPos = true)
        {
            Vector3 pos = targetPos;
            if (fixedYPos)
            {
                pos.y = POSITION_Y; // y값 고정
            }
            return CurrentMovement.SetDestination(pos, useRemainThreshold);
        }

        public void Move(Vector3 motion)
        {
            CurrentMovement.Move(motion);
            OnMove?.Invoke(GetLastPosition());
        }

        public void Warp(Vector3 targetPos)
        {
            // 이동 제한
            Vector3 pos = targetPos;
            pos.y = POSITION_Y; // y값 고정
            CurrentMovement.Warp(pos);
        }

        public void ForceWarp(Vector3 targetPos)
        {
            // 이동 제한
            CurrentMovement.Warp(targetPos);
        }

        public void SetRotation(Quaternion rotation)
        {
            CurrentMovement.SetRotation(rotation);
        }

        public bool IsLinear(Vector3 targetPos)
        {
            return CurrentMovement.IsLinear(targetPos);
        }

        public void SetDefaultSpeed(float defaultSpeed)
        {
            if (isPause)
            {
                savedDefaultSpeed = defaultSpeed;
                return;
            }

            this.defaultSpeed = defaultSpeed;

            float moveSpeedRate = unitEntity ? unitEntity.MoveSpeedRate : 1f;
            SetSpeedRate(moveSpeedRate);
        }

        public void Pause()
        {
            if (isPause)
                return;

            savedDefaultSpeed = defaultSpeed;
            SetDefaultSpeed(0f);
            isPause = true;
        }

        public void Resume()
        {
            if (!isPause)
                return;

            isPause = false;
            SetDefaultSpeed(savedDefaultSpeed);
            savedDefaultSpeed = 0f;
        }

        public float GetDefaultSpeed()
        {
            return defaultSpeed;
        }

        public Mode GetMode()
        {
            return mode;
        }

        private void SetSpeedRate(float speedRate)
        {
            CurrentMovement.SetSpeed(defaultSpeed * speedRate);
        }

        public float GetSpeed()
        {
            return CurrentMovement.GetSpeed();
        }

        public void SetDistanceLimit(float distanceLimit)
        {
            CurrentMovement.SetDistanceLimit(distanceLimit);
        }

        public virtual void KnockBack(Vector3 normalizedVector, float power)
        {
            if (isLookFixed)
                return;

            CurrentMovement.KnockBack(normalizedVector, power);
        }

        public virtual void Rush(Vector3 targetPosition, float rushTime)
        {
            if (isLookFixed)
                return;

            CurrentMovement.Rush(targetPosition, rushTime);
        }

        /// <summary>
        /// 공중에서 낙하.
        /// </summary>
        /// <param name="spawnPosition">스폰 시작 지점</param>
        /// <param name="arrivePosition">스폰 도착 지점 (낙하 지점)</param>
        /// <param name="fallingTime">떨어지는 데에 걸리는 시간</param>
        public void Fall(Vector3 spawnPosition, Vector3 arrivePosition, float fallingTime)
        {
            CurrentMovement.Fall(spawnPosition, arrivePosition, fallingTime);
        }

        public Vector3 GetTargetPos()
        {
            return CurrentMovement.GetTargetPos();
        }

        /// <summary>
        /// 다른 유닛과의 충돌 무시 여부
        /// </summary>
        public void SetCollisionIgnoreUnit(bool isIgnoreUnit)
        {
            CurrentMovement.SetCollisionIgnoreUnit(isIgnoreUnit);
        }

        public void SetAvoidanceRadius(float radius)
        {
            CurrentMovement.SetAvoidanceRadius(radius);
        }

        public void SetAvoidancePriority(int priority)
        {
            CurrentMovement.SetAvoidancePriority(priority);
        }

        public void SetObstacleAvoidanceType(ObstacleAvoidanceType type)
        {
            CurrentMovement.SetObstacleAvoidanceType(type);
        }

        public void SetMask(NavMeshArea area)
        {
            CurrentMovement.SetMask(area);
        }

        public void AddMask(NavMeshArea area)
        {
            CurrentMovement.AddMask(area);
        }

        public void RemoveMask(NavMeshArea area)
        {
            CurrentMovement.RemoveMask(area);
        }

        public void SetLookFixed()
        {
            isLookFixed = true;
        }

        public void ResetLookFixed()
        {
            isLookFixed = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            // 포탈 충돌
            if (other.CompareTag("Portal"))
            {
                OnPortal?.Invoke(other.gameObject);
                OnTempPortal?.Invoke(other.gameObject, unitEntity);
            }

            if (other.CompareTag("MazeDropItem"))
            {
                IMazeDropItem mazeDropItem = other.GetComponent<IMazeDropItem>();
                OnMazeDropItem?.Invoke(mazeDropItem);
                OnTempMazeDropItem?.Invoke(mazeDropItem, unitEntity);
            }

            if (other.CompareTag("WayPoint"))
            {
                OnWayPoint?.Invoke(other.GetComponent<WayPointZone>(), unitEntity);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Portal"))
            {
                OnPortal?.Invoke(null);
                OnTempPortal?.Invoke(null, unitEntity);
                OnPortalOut?.Invoke(other.gameObject);
            }

            if (other.CompareTag("MazeDropItem"))
            {
                OnMazeDropItem?.Invoke(null);
                OnTempMazeDropItem?.Invoke(null, unitEntity);
            }
        }
    }
}