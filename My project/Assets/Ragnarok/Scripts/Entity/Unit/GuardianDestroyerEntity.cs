namespace Ragnarok
{
    public class GuardianDestroyerEntity : MonsterEntity, IPoolObject<GuardianDestroyerEntity>
    {
        public override UnitEntityType type => UnitEntityType.GuardianDestroyer;

        private IPoolDespawner<GuardianDestroyerEntity> despawner;

        public void Initialize(IPoolDespawner<GuardianDestroyerEntity> despawner)
        {
            this.despawner = despawner;
        }

        public override void Release()
        {
            base.Release();

            despawner.Despawn(this);
        }

        public override UnitEntitySettings CreateUnitSettings()
        {
            UnitEntitySettings settings = base.CreateUnitSettings();
            settings.unitSettings.cognizanceDistance = -1f; // 타겟 인지거리: 전체(-1f)
            return settings;
        }

        protected override UnitActor SpawnEntityActor()
        {
            return unitActorPool.SpawnGuardianDestroyer();
        }

        protected override DamagePacket.UnitKey GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.NormalMonster, Monster.MonsterID, Monster.MonsterLevel);
        }
    }
}