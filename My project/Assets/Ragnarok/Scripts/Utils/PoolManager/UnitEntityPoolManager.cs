using System.Collections.Generic;

namespace Ragnarok
{
    public class UnitEntityPoolManager<T> : IPoolDespawner<T>
        where T : UnitEntity, IPoolObject<T>, new()
    {
        private readonly Stack<T> pooledStack;
        private readonly BetterList<T> spawnedList;

        public UnitEntityPoolManager()
        {
            pooledStack = new Stack<T>();
            spawnedList = new BetterList<T>();

            SceneLoader.OnTitleSceneLoaded += Clear;
        }

        ~UnitEntityPoolManager()
        {
            SceneLoader.OnTitleSceneLoaded -= Clear;
        }

        public void Clear()
        {
            while (HasPooledObject())
            {
                T unitEntity = pooledStack.Pop();
                unitEntity.Dispose();
            }

            while (spawnedList.size > 0)
            {
                T unitEntity = spawnedList.Pop();
                unitEntity.Dispose();
            }
        }

        public T Spawn()
        {
            T unitEntity = HasPooledObject() ? pooledStack.Pop() : Create();
            spawnedList.Add(unitEntity);

            unitEntity.Initialize();

            return unitEntity;
        }

        public void Despawn(T unitEntity)
        {
            unitEntity.ResetData();
            unitEntity.Dispose();

            pooledStack.Push(unitEntity);
            spawnedList.Remove(unitEntity);
        }

        public bool HasPooledObject()
        {
            return pooledStack.Count > 0;
        }

        private T Create()
        {
            T unitEntity = new T();
            unitEntity.Initialize(this);
            return unitEntity;
        }
    }
}