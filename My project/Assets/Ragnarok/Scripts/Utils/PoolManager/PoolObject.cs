using MEC;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="EntityActor{TEntity}"/>
    /// <see cref="DroppedClickItem"/>
    /// <see cref="DroppedItem"/>
    /// <see cref="PickingEffect"/>
    /// <see cref="ProjectileEffect"/>
    /// <see cref="TargetingArrow"/>
    /// <see cref="TargetingLine"/>
    /// <see cref="UnitCircle"/>
    /// <see cref="HUDObject"/>
    /// <see cref="AutoReleasePoolObject"/>
    /// </summary>
    public class PoolObject : MonoBehaviour
    {
        private IPooledDespawner despawner;
        private string poolID;

        public GameObject CachedGameObject { get; private set; }
        public Transform CachedTransform { get; private set; }
        public bool IsPooled { get; protected set; }
        protected Camera mainCamera;

        public event System.Action<PoolObject> OnRelease;

        protected TrailRenderer[] trailParticles;

        protected virtual void Awake()
        {
            CachedGameObject = gameObject;
            CachedTransform = transform;
            mainCamera = Camera.main;

            trailParticles = GetComponentsInChildren<TrailRenderer>(includeInactive: true);
        }

        protected virtual void OnDestroy()
        {
            Timing.KillCoroutines(CachedGameObject);

            CachedTransform = null;
            CachedGameObject = null;
        }

        public virtual void OnCreate(IPooledDespawner despawner, string poolID)
        {
            this.despawner = despawner;
            this.poolID = poolID;
            IsPooled = false;
        }

        public virtual void OnSpawn()
        {
            IsPooled = false;
        }

        public virtual void OnDespawn()
        {
            OnRelease?.Invoke(this);

            // 잔존 트레일 제거.
            for (int i = 0; i < trailParticles.Length; ++i)
            {
                trailParticles[i].Clear();
            }

            IsPooled = true;
        }

        public void Release()
        {
            if (despawner == null)
                return;

            despawner.Despawn(poolID, this);
        }

        public void Show()
        {
            SetActive(true);
        }

        public void Hide()
        {
            SetActive(false);
        }

        public void SetActive(bool isActive)
        {
            CachedGameObject.SetActive(isActive);
        }
    }
}