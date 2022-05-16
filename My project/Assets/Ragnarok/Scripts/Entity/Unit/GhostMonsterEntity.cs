namespace Ragnarok
{
    public class GhostMonsterEntity : MonsterEntity, IPoolObject<GhostMonsterEntity>
    {
        public override UnitEntityType type => UnitEntityType.GhostMonster;

        /// <summary>
        /// 레이어 값
        /// </summary>
        public override int Layer => Ragnarok.Layer.GHOST;

        private IPoolDespawner<GhostMonsterEntity> despawner;

        public void Initialize(IPoolDespawner<GhostMonsterEntity> despawner)
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
            return default;
        }
    }
}