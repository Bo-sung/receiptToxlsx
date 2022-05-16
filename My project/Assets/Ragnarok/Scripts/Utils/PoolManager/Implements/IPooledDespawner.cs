namespace Ragnarok
{
    public interface IPooledDespawner
    {
        void Despawn(string key, PoolObject poolObject);
    }
}