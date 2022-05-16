using UnityEngine;

namespace Ragnarok
{
    public class WorldBossEntity : BossMonsterEntity, IPoolObject<WorldBossEntity>
    {
        public override UnitEntityType type => UnitEntityType.WorldBoss;

        private IPoolDespawner<WorldBossEntity> despawner;

        public void Initialize(IPoolDespawner<WorldBossEntity> despawner)
        {
            this.despawner = despawner;
        }

        public override void Release()
        {
            base.Release();

            despawner.Despawn(this);
        }

        protected override UnitActor SpawnEntityActor()
        {
            UnitActor actor = unitActorPool.SpawnWorldBoss();
            actor.CachedTransform.localScale = Vector3.one * scale; // 스케일 적용
            return actor;
        }

        protected override void ApplyHP(int value, int blowCount, bool isNotDie)
        {
        }
    }
}