using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 이펙트 또는 파티클의 플레이가 종료되면 자동으로 Release
    /// </summary>
    public class AutoReleasePoolObject : PoolObject
    {
        ParticleSystem[] particles;
        Animation[] animations;

        protected override void Awake()
        {
            base.Awake();

            animations = GetComponentsInChildren<Animation>(includeInactive: true);
            particles = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
        }

        protected virtual void Update()
        {
            if (!IsPlaying())
                Release();
        }

        private bool IsPlaying()
        {
            if (animations != null)
            {
                foreach (var item in animations)
                {
                    if (item.isPlaying)
                        return true;
                }
            }

            if (particles != null)
            {
                foreach (var item in particles)
                {
                    if (item.isPlaying)
                        return true;
                }
            }

            return false;
        }
    }
}