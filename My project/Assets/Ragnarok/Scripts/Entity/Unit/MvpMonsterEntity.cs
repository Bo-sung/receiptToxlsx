namespace Ragnarok
{
    public class MvpMonsterEntity : BossMonsterEntity, IPoolObject<MvpMonsterEntity>
    {
        public override UnitEntityType type => UnitEntityType.MvpMonster;

        private IPoolDespawner<MvpMonsterEntity> despawner;

        public void Initialize(IPoolDespawner<MvpMonsterEntity> despawner)
        {
            this.despawner = despawner;
        }

        public override void Release()
        {
            base.Release();

            despawner.Despawn(this);
        }
    }
}