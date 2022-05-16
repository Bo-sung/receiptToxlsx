namespace Ragnarok
{
    public class TurretEntity : MonsterEntity, IPoolObject<TurretEntity>
    {
        public override UnitEntityType type => UnitEntityType.Turret;

        private IPoolDespawner<TurretEntity> despawner;

        public void Initialize(IPoolDespawner<TurretEntity> despawner)
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
            return unitActorPool.SpawnTurret();
        }

        protected override DamagePacket.UnitKey GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.NormalMonster, Monster.MonsterID, Monster.MonsterLevel);
        }
    }
}