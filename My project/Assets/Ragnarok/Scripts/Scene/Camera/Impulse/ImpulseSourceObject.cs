using Cinemachine;
using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class ImpulseSourceObject : PoolObject
    {
        CinemachineImpulseSource impulseSource;

        protected override void Awake()
        {
            base.Awake();

            impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        public void Fire()
        {
            if (!LocalValue.IsCameraShake)
                return;

            impulseSource.GenerateImpulse();
        }
    }
}