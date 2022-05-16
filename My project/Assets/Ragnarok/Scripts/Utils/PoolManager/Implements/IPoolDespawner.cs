namespace Ragnarok
{
    public interface IPoolDespawner<T>
    {
        void Despawn(T t);
    }
}