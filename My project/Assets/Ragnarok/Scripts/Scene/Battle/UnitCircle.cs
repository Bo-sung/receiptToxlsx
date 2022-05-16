using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(ParticleSystem))]
    public class UnitCircle : PoolObject
    {
        [SerializeField] ParticleSystem myParticleSystem;

        [SerializeField, Rename(displayName = "아군 색상")]
        Color alliesColor = Color.white;

        [SerializeField, Rename(displayName = "적군 색상")]
        Color enemyColor = Color.red;

        public override void OnCreate(IPooledDespawner despawner, string poolID)
        {
            base.OnCreate(despawner, poolID);
        }

        public void Initialize(bool isEnemy)
        {
            ParticleSystem.MainModule settings = myParticleSystem.main;
            settings.startColor = new ParticleSystem.MinMaxGradient(isEnemy ? enemyColor : alliesColor);
        }
    }
}