namespace Ragnarok
{
    public class NexusEntity : MonsterEntity, IPoolObject<NexusEntity>
    {
        public override UnitEntityType type => UnitEntityType.Nexus;

        /// <summary>
        /// 팀 인덱스
        /// </summary>
        public int TeamIndex { get; set; }

        /// <summary>
        /// 고정 최대 체력
        /// </summary>
        public int FixedMaxHp { get; set; }

        private IPoolDespawner<NexusEntity> despawner;

        public void Initialize(IPoolDespawner<NexusEntity> despawner)
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
            return unitActorPool.SpawnNexus();
        }

        protected override DamagePacket.UnitKey GetDamageUnitKey()
        {
            return default;
        }
    }
}