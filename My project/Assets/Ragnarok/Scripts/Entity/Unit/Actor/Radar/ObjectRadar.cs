using UnityEngine;

namespace Ragnarok
{
    public class ObjectRadar : MonoBehaviour
    {
        private const int FIXED_UPDATE_TICK = UnitAI.UPDATE_TICK;

        /// <summary>
        /// 감지거리
        /// </summary>
        [SerializeField]
        float senseDistance = 3f;

        Transform myTransform;
        BattleUnitList unitList;

        private UnitActor target;
        private int fixedTick;

        public event System.Action<UnitActor> OnFocusTarget;
        public event System.Action<UnitActor> OnLostFocusTarget;

        void Awake()
        {
            myTransform = transform;
            unitList = BattleManager.Instance.unitList;
        }

        void FixedUpdate()
        {
            if (++fixedTick < FIXED_UPDATE_TICK)
                return;

            fixedTick = 0;

            if (target)
            {
                // 타겟이 죽었을 경우
                if (target.Entity.IsDie)
                    ResetTarget();

                if (senseDistance != -1f)
                {
                    float sqrDistance = senseDistance * senseDistance; // 거리 제곱
                    float dist = (myTransform.position - target.Entity.LastPosition).sqrMagnitude;

                    // 찾고자 하는 거리보다 멀어졌을 경우
                    if (dist > sqrDistance)
                        ResetTarget();
                }
            }

            if (target == null)
                SetTarget(FindMinTarget());
        }

        public void SetSenseDistance(float senseDistance)
        {
            this.senseDistance = senseDistance;
        }

        public void ResetTarget()
        {
            SetTarget(null);
        }

        private UnitActor FindMinTarget()
        {
            float sqrDistance = senseDistance == -1f ? -1f : (senseDistance * senseDistance); // 거리 제곱
            Vector3 pos = myTransform.position;

            float minDist = float.MaxValue;
            UnitActor minTarget = null;
            for (int i = 0; i < unitList.size; i++)
            {
                // 죽어있음
                if (unitList[i].IsDie)
                    continue;

                if (unitList[i].GetActor() == null)
                    continue;

                float dist = (pos - unitList[i].LastPosition).sqrMagnitude;

                // 찾고자 하는 거리보다 멀다
                if ((sqrDistance != -1) && (dist > sqrDistance))
                    continue;

                // 가장 가까운 것은 아니다
                if (dist > minDist)
                    continue;

                minDist = dist;
                minTarget = unitList[i].GetActor();
            }

            return minTarget;
        }

        private void SetTarget(UnitActor other)
        {
            if (GetInstanceID(target) == GetInstanceID(other))
                return;

            OnLostFocusTarget?.Invoke(target); // 기존 타겟 변경 이벤트 호출
            target = other; // 타겟 세팅
            OnFocusTarget?.Invoke(target); // 타겟 변경 이벤트 호출
        }

        private int GetInstanceID(UnitActor actor)
        {
            return actor == null ? 0 : actor.GetInstanceID();
        }
    }
}