namespace Ragnarok
{
    public interface IEntityActorElement<T>
        where T : UnitEntity
    {
        void OnReady(T entity);

        void OnRelease();

        void AddEvent();

        void RemoveEvent();
    }
}