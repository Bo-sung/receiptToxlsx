namespace Ragnarok
{
    public abstract class UnitModel<T> : IUnitModel, IInitializable<T>, System.IDisposable
        where T : UnitEntity
    {
        protected T Entity { get; private set; }

        public virtual void Initialize(T entity)
        {
            Entity = entity;
            Entity.modelList.Add(this);
        }

        public virtual void Dispose()
        {
            Entity = null;
            Entity.modelList.Remove(this);
        }

        public virtual void Ready()
        {

        }

        public virtual void ResetData()
        {

        }

        public abstract void AddEvent(UnitEntityType type);

        public abstract void RemoveEvent(UnitEntityType type);

        [System.Obsolete("Initialize를 이용할 것")]
        public static TInfo Create<TInfo>(T entity)
            where TInfo : UnitModel<T>, new()
        {
            TInfo info = new TInfo() { Entity = entity };
            entity.modelList.Add(info); // Info 에 추가
            return info;
        }

        [System.Obsolete("사용하지 않음")]
        public TModel Clone<TModel>()
            where TModel : UnitModel<T>
        {
            return MemberwiseClone() as TModel;
        }

        public static implicit operator bool(UnitModel<T> model)
        {
            return model != null;
        }
    }
}