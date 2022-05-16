using AnimationOrTween;
using System;
using UnityEngine;

namespace Ragnarok
{
    public class MazeTreasure : PoolObject, IMazeDropItem
    {
        private int pointId;
        private MazeRewardType rewardType;
        int IMazeDropItem.PointId => pointId;
        MazeRewardType IMazeDropItem.RewardType => rewardType;

        public event Action<IMazeDropItem> OnDeSpawn;

        [SerializeField, Rename(displayName = "OnDespawn 시간")]
        float onDeSpawnTime = 0.5f;

        [SerializeField] SphereCollider sphereCollider;
        [SerializeField] Animator animator;
        [SerializeField] GameObject goActive, goDeactive;

        void IMazeDropItem.Set(int pointId, MazeRewardType rewardType, UIWidget target = null)
        {
            this.pointId = pointId;
            this.rewardType = rewardType;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            sphereCollider.enabled = true;
            animator.Update(0);
            animator.enabled = false;
        }

        public override void OnDespawn()
        {
            OnDeSpawn?.Invoke(this);
            base.OnDespawn();
        }

        void IMazeDropItem.StartEffect()
        {
            Hit();
            var ani = ActiveAnimation.Play(animator, "MazeTreasure", Direction.Forward, EnableCondition.EnableThenPlay, DisableCondition.DoNotDisable);
            EventDelegate.Add(ani.onFinished, Release, oneShot: true);
        }

        public void SetGoActive(bool isActive)
        {
            goActive.SetActive(isActive);
            goDeactive.SetActive(!isActive);
        }

        public void Hit()
        {
            sphereCollider.enabled = false;
        }
    }
}
