namespace Ragnarok
{
    public sealed class GuardianEntity : MonsterEntity, IPoolObject<GuardianEntity>
    {
        public override UnitEntityType type => UnitEntityType.Guardian;

        private IPoolDespawner<GuardianEntity> despawner;

        public void Initialize(IPoolDespawner<GuardianEntity> despawner)
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
            return unitActorPool.SpawnGuardian();
        }

        protected override DamagePacket.UnitKey GetDamageUnitKey()
        {
            return default;
        }
    }
}