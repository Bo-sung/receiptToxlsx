namespace Ragnarok
{
    public interface ICharacterPool
    {
        PoolObject Spawn(Job job, Gender gender);
        PoolObject Spawn(string prefabName);
        PoolObject Spawn(int costumeIndex);
        PoolObject SpawnShadow();
        PoolObject SpawnTimeSuit();
    }
}