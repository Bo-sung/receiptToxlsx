namespace Ragnarok
{
    public abstract class EntityActor<TEntity> : PoolObject, IEntityActorElement<TEntity>
        where TEntity : UnitEntity
    {
        IEntityActorElement<TEntity>[] elements;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (elements != null)
            {
                foreach (var item in elements)
                {
                    item.RemoveEvent();
                }
            }
        }

        public override void OnCreate(IPooledDespawner despawner, string poolID)
        {
            base.OnCreate(despawner, poolID);

            elements = GetComponentsInChildren<IEntityActorElement<TEntity>>(includeInactive: true);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
        }

        public override void OnDespawn()
        {
            base.OnDespawn();

            foreach (var item in elements)
            {
                item.RemoveEvent();
                item.OnRelease();
            }
        }

        public void SetEntity(UnitEntity entity)
        {
            foreach (var item in elements)
            {
                item.OnReady(entity as TEntity);
                item.AddEvent();
            }
        }

        public abstract void OnReady(TEntity entity);

        public new abstract void OnRelease();

        public abstract void AddEvent();

        public abstract void RemoveEvent();
    }
}