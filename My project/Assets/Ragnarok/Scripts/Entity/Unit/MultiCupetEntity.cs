namespace Ragnarok
{
    public class MultiCupetEntity : CupetEntity, IPoolObject<MultiCupetEntity>
    {
        public override UnitEntityType type => UnitEntityType.MultiCupet;

        private IPoolDespawner<MultiCupetEntity> despawner;

        public void Initialize(IPoolDespawner<MultiCupetEntity> despawner)
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
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.Cupet, Cupet.CupetID, Cupet.Level);
        }
    }
}