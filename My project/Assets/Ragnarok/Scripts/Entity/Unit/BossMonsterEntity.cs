using UnityEngine;

namespace Ragnarok
{
    public class BossMonsterEntity : MonsterEntity, IPoolObject<BossMonsterEntity>
    {
        public override UnitEntityType type => UnitEntityType.BossMonster;

        private IPoolDespawner<BossMonsterEntity> despawner;

        public void Initialize(IPoolDespawner<BossMonsterEntity> despawner)
        {
            this.despawner = despawner;
        }

        public override void Release()
        {
            base.Release();

            despawner.Despawn(this);
        }

        /// <summary>
        /// Actor 생성
        /// </summary>
        protected override UnitActor SpawnEntityActor()
        {
            UnitActor actor = unitActorPool.SpawnBossMonster();
            actor.CachedTransform.localScale = Vector3.one * scale; // 스케일 적용
            return actor;
        }

        protected override DamagePacket.UnitKey GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.BossMonster, Monster.MonsterID, Monster.MonsterLevel);
        }

        protected override MonsterType GetMonsterType()
        {
            return MonsterType.Boss;
        }
    }
}