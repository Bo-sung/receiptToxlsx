namespace Ragnarok
{
    public interface IPoolObject<T> : IInitializable<IPoolDespawner<T>>
    {
        void Release();
    }
}