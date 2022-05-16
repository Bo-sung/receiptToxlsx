namespace Ragnarok
{
    public class GhostPlayerEntity : CharacterEntity, IPoolObject<GhostPlayerEntity>
    {
        public override UnitEntityType type => UnitEntityType.GhostPlayer;

        /// <summary>
        /// 레이어 값
        /// </summary>
        public override int Layer => Ragnarok.Layer.GHOST;

        private IPoolDespawner<GhostPlayerEntity> despawner;
        private DamagePacket.UnitKey damageUnitKey;

        public SharingModel.CloneCharacterType CloneType { get; private set; }

        public void Initialize(IPoolDespawner<GhostPlayerEntity> despawner)
        {
            this.despawner = despawner;
        }

        public void Initialize(IMultiPlayerInput input)
        {
            Character.Initialize(input);
            Status.Initialize(input);
            Status.Initialize(input.IsExceptEquippedItems, input.BattleOptions, input.GuildBattleOptions);
            Inventory.Initialize(input.ItemStatusValue, input.WeaponItemId, input.ArmorItemId, input.WeaponChangedElement, input.WeaponElementLevel, input.ArmorChangedElement, input.ArmorElementLevel, input.GetEquippedItems);
            Skill.Initialize(input.IsExceptEquippedItems, input.Skills);
            Skill.Initialize(input.Slots);
            Guild.Initialize(input);
            Trade.Initialize(input);
        }

        public void SetDamageUnitKey(DamagePacket.UnitKey damageUnitKey)
        {
            this.damageUnitKey = damageUnitKey;
        }

        public override void Release()
        {
            base.Release();

            despawner.Despawn(this);
            SetCloneType(default);
        }

        public void SetCloneType(SharingModel.CloneCharacterType cloneType)
        {
            CloneType = cloneType;
        }

        protected override UnitActor SpawnEntityActor()
        {
            return unitActorPool.SpawnGhostPlayer();
        }

        protected override DamagePacket.UnitKey GetDamageUnitKey()
        {
            return damageUnitKey;
        }

        public override string GetThumbnailName()
        {
            var unit = GetDamageUnitKey();
            if (unit.type == DamagePacket.DamageUnitType.Agent)
                return Character.GetAgentIconName(unit.id, AgentIconType.CircleIcon);

            return base.GetThumbnailName();
        }
    }
}