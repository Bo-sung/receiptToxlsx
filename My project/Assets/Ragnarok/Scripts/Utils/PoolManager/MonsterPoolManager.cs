using UnityEngine;

namespace Ragnarok
{
    public class MonsterPoolManager : PoolManager<MonsterPoolManager>, IMonsterPool
    {
        IMonsterContainer monsterContainer;

        protected override void Awake()
        {
            base.Awake();

            monsterContainer = AssetManager.Instance;
        }

        protected override Transform GetOriginal(string key)
        {
            GameObject go = monsterContainer.Get(key);

            if (go == null)
                throw new System.ArgumentException($"존재하지 않는 GameObject 입니다: {nameof(key)} = {key}");

            return go.transform;
        }

        protected override PoolObject Create(string key)
        {
            GameObject go = monsterContainer.Get(key);

            if (go == null)
                throw new System.ArgumentException($"존재하지 않는 GameObject 입니다: {nameof(key)} = {key}");

            return Instantiate(go).AddMissingComponent<PoolObject>();
        }
    }
}