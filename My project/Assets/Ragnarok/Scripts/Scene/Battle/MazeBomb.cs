using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class MazeBomb : PoolObject, IMazeDropItem
    {
        private int pointId;
        private MazeRewardType rewardType;
        int IMazeDropItem.PointId => pointId;
        MazeRewardType IMazeDropItem.RewardType => rewardType;

        public event Action<IMazeDropItem> OnDeSpawn;

        [SerializeField, Rename(displayName = "OnDespawn 시간")]
        float onDeSpawnTime = 0.5f;

        [SerializeField] GameObject bomb;
        [SerializeField] GameObject effect;
        [SerializeField] SphereCollider sphereCollider;

        void IMazeDropItem.Set(int pointId, MazeRewardType rewardType, UIWidget target = null)
        {
            this.pointId = pointId;
            this.rewardType = rewardType;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            sphereCollider.enabled = true;
            bomb.SetActive(true);
            effect.SetActive(false);
        }

        public override void OnDespawn()
        {
            OnDeSpawn?.Invoke(this);
            base.OnDespawn();
        }

        void IMazeDropItem.StartEffect()
        {
            Timing.KillCoroutines(gameObject);
            Timing.RunCoroutine(YieldDespawn(), gameObject);
        }

        IEnumerator<float> YieldDespawn()
        {
            Hit();
            bomb.SetActive(false);
            effect.SetActive(true);
            yield return Timing.WaitForSeconds(onDeSpawnTime);
            Release();
        }

        public void Hit()
        {
            sphereCollider.enabled = false;
        }
    }
}