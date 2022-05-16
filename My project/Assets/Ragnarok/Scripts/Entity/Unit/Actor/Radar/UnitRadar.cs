using UnityEngine;

namespace Ragnarok
{
    public class UnitRadar : MonoBehaviour, IEntityActorElement<UnitEntity>
    {
        private const int FIXED_UPDATE_TICK = UnitAI.UPDATE_TICK;

        /// <summary>
        /// 감지거리
        /// </summary>
        [SerializeField]
        float senseDistance = 3f;

        /// <summary>
        /// 감지대상
        /// </summary>
        [SerializeField]
        TargetType senseTargetType = TargetType.Enemy;

        UnitActor actor;
        BattleUnitList unitList;

        private UnitEntity entity;

        [SerializeField]
        private UnitActor target;
        private int fixedTick;

        public event System.Action<UnitActor> OnTarget;
        public event System.Action<UnitActor> OnLostTarget;
        public event System.Action<UnitActor, UnitActor> OnTargetActor;

        void Awake()
        {
            actor = GetComponent<UnitActor>();
            unitList = BattleManager.Instance.unitList;
        }

        void Update()
        {
            if (entity == null)
                return;

            if (++fixedTick < FIXED_UPDATE_TICK)
                return;

            fixedTick = 0;

            if (target)
            {
                // 타겟이 죽었을 경우
                if (target.Entity.IsDie)
                {
                    ResetTarget();
                }
                else if (!actor.IsLookTarget(target, senseTargetType)) // 타겟 타입에 해당되지 않음
                {
                    ResetTarget();
                }
                else if (senseDistance != -1f)
                {
                    float sqrDistance = senseDistance * senseDistance; // 거리 제곱
                    float dist = (entity.LastPosition - target.Entity.LastPosition).sqrMagnitude;

                    // 찾고자 하는 거리보다 멀어졌을 경우
                    if (dist > sqrDistance)
                        ResetTarget();
                }
            }

            if (target == null)
                SetTarget(unitList.FindMinTarget(actor, senseTargetType, senseDistance));
        }

        public virtual void OnReady(UnitEntity entity)
        {
            this.entity = entity;
        }

        public virtual void OnRelease()
        {
            entity = null;
            ResetTarget();
        }

        public virtual void AddEvent()
        {
        }

        public virtual void RemoveEvent()
        {

        }

        public void SetSenseDistance(float senseDistance)
        {
            this.senseDistance = senseDistance;
        }

        public void SetSenseTargetType(TargetType senseTargetType)
        {
            this.senseTargetType = senseTargetType;
        }

        public void ResetTarget()
        {
            SetTarget(null);
        }

        private void SetTarget(UnitActor other)
        {
            if (GetInstanceID(target) == GetInstanceID(other))
                return;

            OnLostTarget?.Invoke(target); // 기존 타겟 변경 이벤트 호출
            target = other; // 타겟 세팅
            OnTarget?.Invoke(target); // 타겟 변경 이벤트 호출
            OnTargetActor?.Invoke(target, actor);
        }

        private int GetInstanceID(UnitActor actor)
        {
            return actor == null ? 0 : actor.GetInstanceID();
        }
    }
}