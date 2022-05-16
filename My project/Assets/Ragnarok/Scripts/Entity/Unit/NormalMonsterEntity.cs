namespace Ragnarok
{
    public class NormalMonsterEntity : MonsterEntity, IPoolObject<NormalMonsterEntity>
    {
        public override UnitEntityType type => UnitEntityType.NormalMonster;

        private IPoolDespawner<NormalMonsterEntity> despawner;

        public void Initialize(IPoolDespawner<NormalMonsterEntity> despawner)
        {
            this.despawner = despawner;
        }

        public override void Release()
        {
            base.Release();

            despawner.Despawn(this);
        }

        protected override DamagePacket.UnitKey GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.NormalMonster, Monster.MonsterID, Monster.MonsterLevel);
        }
    }
}