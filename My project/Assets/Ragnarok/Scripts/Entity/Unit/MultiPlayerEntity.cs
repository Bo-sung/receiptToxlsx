namespace Ragnarok
{
    public class MultiPlayerEntity : CharacterEntity, IPoolObject<MultiPlayerEntity>
    {
        public override UnitEntityType type => UnitEntityType.MultiPlayer;

        public override int Layer => State == UnitState.Maze ? Ragnarok.Layer.MAZE_OTHER_PLAYER : base.Layer;

        IPoolDespawner<MultiPlayerEntity> despawner;

        public void Initialize(IPoolDespawner<MultiPlayerEntity> despawner)
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
            switch (State)
            {
                case UnitState.Maze:
                    return unitActorPool.SpawnMazePlayer();

                case UnitState.GVG:
                    return unitActorPool.SpawnGVGPlayer();

                case UnitState.GVGMultiPlayer:
                    return unitActorPool.SpawnGVGMultiPlayer();
            }

            return base.SpawnEntityActor();
        }

        protected override DamagePacket.UnitKey GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.Character, Character.Cid, Character.JobLevel);
        }
    }

    [System.Obsolete]
    public class DummyMultiPlayerEntity : MultiPlayerEntity
    {
        public DummyMultiPlayerEntity() : base()
        {
        }
    }
}