using System;
using UnityEngine;

namespace Ragnarok
{
    public interface IMazeDropItem
    {
        int PointId { get; }
        MazeRewardType RewardType { get; }
        void Set(int pointId, MazeRewardType rewardType, UIWidget target = null);
        void Hit();
        void StartEffect();
        void Release();
        event Action<IMazeDropItem> OnDeSpawn;
    }

    public class MazeGold : DroppedItem, IMazeDropItem
    {
        private float spread_power = 0.15f;

        private int pointId;
        private MazeRewardType rewardType;
        int IMazeDropItem.PointId => pointId;
        MazeRewardType IMazeDropItem.RewardType => rewardType;

        public event Action<IMazeDropItem> OnDeSpawn;

        private UIWidget targetWidget;

        [SerializeField] public SphereCollider sphereCollider;

        bool isSpread;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void OnSpawn()
        {
            //base.OnSpawn();
            IsPooled = false;

            isSpread = false;
            sphereCollider.enabled = true;
            Show();
        }

        public override void OnDespawn()
        {
            OnDeSpawn?.Invoke(this);
            base.OnDespawn();
        }

        public void SetSpread(bool isSpread)
        {
            this.isSpread = isSpread;
        }

        void IMazeDropItem.Set(int pointId, MazeRewardType rewardType, UIWidget target = null)
        {
            this.pointId = pointId;
            this.rewardType = rewardType;
            targetWidget = target;
            ResetItem();
        }

        void IMazeDropItem.StartEffect()
        {
            base.OnSpawn();

            Hit();

            if (isSpread)
            {
                float randomDegree = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                velocity += Vector3.right * spread_power * Mathf.Cos(randomDegree);
                velocity += Vector3.forward * spread_power * Mathf.Sin(randomDegree);

                ChangeState(State.Jump);
            }
        }

        protected override Vector3 GetDestination()
        {
            if (targetWidget == null)
            {
                //return transform.position + Vector3.down * 5f; //Vector3.zero;
                return base.GetDestination();
            }

            Vector3 dest = UI.CurrentCamera.WorldToScreenPoint(targetWidget.cachedTransform.position);
            dest.z = cameraDistance;
            return mainCamera.ScreenToWorldPoint(dest) + offset;
        }

        public void Hit()
        {
            sphereCollider.enabled = false;
        }
    }
}
