using UnityEngine;

namespace Ragnarok
{
    public class CharacterPoolManager : PoolManager<CharacterPoolManager>, ICharacterPool
    {
        ICharacterContainer characterContainer;

        protected override void Awake()
        {
            base.Awake();

            characterContainer = AssetManager.Instance;
        }

        protected override Transform GetOriginal(string key)
        {
            GameObject go = characterContainer.Get(key);

            if (go == null)
                throw new System.ArgumentException($"존재하지 않는 GameObject 입니다: {nameof(key)} = {key}");

            return go.transform;
        }

        protected override PoolObject Create(string key)
        {
            GameObject go = characterContainer.Get(key);

            if (go == null)
                throw new System.ArgumentException($"존재하지 않는 GameObject 입니다: {nameof(key)} = {key}");

            return Instantiate(go).AddMissingComponent<PoolObject>();
        }

        PoolObject ICharacterPool.Spawn(Job job, Gender gender)
        {
            string jobName = job.ToString();
            string prefabName = jobName.AddPostfix(gender);
            return Spawn(prefabName);
        }

        // TODO 추구 스폰 통합
        PoolObject ICharacterPool.Spawn(string prefabName)
        {
            return Spawn(prefabName);
        }

		PoolObject ICharacterPool.Spawn(int costumeIndex)
		{
			// TODO: 제대로 인덱싱하도록 수정
			switch (costumeIndex)
			{
				case 0: return Spawn("Bongdali");
				case 1: return Spawn("Bongdali2");
				case 2: return Spawn("Brown");
				case 3: return Spawn("Brown2");
			}
			return Spawn("Bongdali");
		}

        PoolObject ICharacterPool.SpawnShadow()
        {
            return Spawn("Guild_Shadow_Character");
        }

        PoolObject ICharacterPool.SpawnTimeSuit()
        {
            return Spawn("TimeSuit");
        }
    }
}