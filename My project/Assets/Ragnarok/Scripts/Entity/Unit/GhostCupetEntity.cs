namespace Ragnarok
{
    public class GhostCupetEntity : CupetEntity, IPoolObject<GhostCupetEntity>, IInitializable<IMultiCupetInput>
    {
        public readonly static IMultiCupetInput DEFAULT = new DefaultMultiCupetInput();

        private class DefaultMultiCupetInput : IMultiCupetInput
        {
            int IMultiCupetInput.Id => 0;
            int IMultiCupetInput.Rank => 0;
            int IMultiCupetInput.Level => 0;
        }

        public override UnitEntityType type => UnitEntityType.GhostCupet;

        /// <summary>
        /// 레이어 값
        /// </summary>
        public override int Layer => Ragnarok.Layer.GHOST;

        private IPoolDespawner<GhostCupetEntity> despawner;

        public void Initialize(IPoolDespawner<GhostCupetEntity> despawner)
        {
            this.despawner = despawner;
        }

        public override void Release()
        {
            base.Release();

            despawner.Despawn(this);
        }

        public void Initialize(IMultiCupetInput input)
        {
            Cupet.Initialize(input.Id, input.Rank, input.Level);
        }

        public override UnitEntitySettings CreateUnitSettings()
        {
            UnitEntitySettings settings = base.CreateUnitSettings();

            if (settings != null)
            {
                settings.unitSettings.cognizanceDistance = -1; // 타겟 인지거리: 전체(-1f)
            }

            return settings;
        }

        /// <summary>
        /// Actor 생성
        /// </summary>
        protected override UnitActor SpawnEntityActor()
        {
            return unitActorPool.SpawnGhostCupet();
        }

        protected override DamagePacket.UnitKey GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.Cupet, Cupet.CupetID, Cupet.Level);
        }

        protected override bool IsNeedSaveDamagePacket()
        {
            return !IsEnemy;
        }
    }
}