using UnityEngine;
using System.Collections.Generic;
using MEC;

namespace Ragnarok
{
    /// <summary>
    /// 어셋번들 GameObject 위주
    /// </summary>
    public abstract class PoolManager<TKey> : GameObjectSingleton<TKey>, IPooledDespawner
        where TKey : Component
    {
        Dictionary<string, Stack<PoolObject>> poolDic;
        Dictionary<string, Transform> parentDic;
        List<PoolObject> spawnedList;

        protected override void Awake()
        {
            base.Awake();

            poolDic = new Dictionary<string, Stack<PoolObject>>(System.StringComparer.Ordinal);
            parentDic = new Dictionary<string, Transform>(System.StringComparer.Ordinal);
            spawnedList = new List<PoolObject>();
        }

        public Dictionary<string, Stack<PoolObject>> GetPooledDataDic()
        {
            return poolDic;
        }

        protected override void OnTitle()
        {
            AllRelease();
        }

        protected void Create(string key, int count)
        {
            Timing.RunCoroutine(Add(key, count));
        }

        public PoolObject Spawn(string key)
        {
            return Spawn(key, Vector3.zero);
        }

        public PoolObject Spawn(string key, Vector3 position)
        {
            return Spawn(key, position, Quaternion.identity);
        }

        public PoolObject Spawn(string key, Vector3 position, Quaternion rotation)
        {
            return Spawn(key, null, false, position, rotation);
        }

        public PoolObject Spawn(string key, Transform parent, bool worldPositionStays)
        {
            return Spawn(key, parent, worldPositionStays, Vector3.zero);
        }

        public PoolObject Spawn(string key, Transform parent, bool worldPositionStays, Vector3 position)
        {
            return Spawn(key, parent, worldPositionStays, position, Quaternion.identity);
        }

        public PoolObject Spawn(string key, Transform parent, bool worldPositionStays, Vector3 position, Quaternion rotation)
        {
            Stack<PoolObject> stack = GetStack(key);
            PoolObject poolObject = stack.Count > 0 ? stack.Pop() : CreatePoolObject(key);

            if (parent)
            {
                poolObject.CachedTransform.SetParent(parent, worldPositionStays);

                if (worldPositionStays)
                {
                    poolObject.CachedTransform.position = position;
                    poolObject.CachedTransform.rotation = rotation;
                }
                else
                {
                    poolObject.CachedTransform.localPosition = position;
                    poolObject.CachedTransform.localRotation = rotation;
                }
            }
            else
            {
                poolObject.CachedTransform.position = position;
                poolObject.CachedTransform.rotation = rotation;
            }

            poolObject.CachedGameObject.SetActive(true);
            poolObject.OnSpawn(); // OnSpawn

            spawnedList.Add(poolObject);

            return poolObject;
        }

        public void Despawn(string key, PoolObject poolObject)
        {
            if (poolObject == null)
            {
#if UNITY_EDITOR
                Debug.LogError("[Despawn] poolObject is Null");
#endif
                return;
            }

            if (poolObject.IsPooled)
                return;

            poolObject.OnDespawn(); // OnDespawn 까지는 처리 필수!

            if (IntroScene.IsBackToTitle)
                return;

            spawnedList.Remove(poolObject);
            poolObject.CachedGameObject.SetActive(false);

            if (!parentDic.ContainsKey(key))
                return;

            if (!poolObject.CachedTransform.IsChildOf(parentDic[key]))
                poolObject.CachedTransform.SetParent(parentDic[key]);

            // 원본이랑 똑같은 상태로 되돌려준다
            Transform original = GetOriginal(key);
            poolObject.CachedTransform.localPosition = original.localPosition;
            poolObject.CachedTransform.localRotation = original.localRotation;
            poolObject.CachedTransform.localScale = original.localScale;

            GetStack(key).Push(poolObject);
        }

        public void Clear(string key)
        {
            if (poolDic.ContainsKey(key))
            {
                poolDic[key].Clear();
                poolDic.Remove(key);
            }
        }

        /// <summary>
        /// 모든 Spawned Object를 Release 시킵니다
        /// </summary>
        public void AllRelease()
        {
            RemoveAllImmediate();
            //Timing.RunCoroutine(RemoveAll());
        }

        private Stack<PoolObject> GetStack(string key)
        {
            if (!poolDic.ContainsKey(key))
            {
                poolDic.Add(key, new Stack<PoolObject>());

                GameObject go = new GameObject(key);
                Transform tf = go.transform;
                parentDic.Add(key, tf);

                tf.SetParent(transform, worldPositionStays: true);
            }

            return poolDic[key];
        }

        private IEnumerator<float> Add(string key, int count)
        {
            Stack<PoolObject> stack = GetStack(key);

            while (stack.Count < count)
            {
                PoolObject poolObject = CreatePoolObject(key);
                Despawn(key, poolObject);
                yield return Timing.WaitForOneFrame;
            }
        }

        private PoolObject CreatePoolObject(string key)
        {
            PoolObject poolObject = Create(key);
            poolObject.OnCreate(this, key);

            poolObject.CachedTransform.SetParent(parentDic[key], worldPositionStays: false);
            return poolObject;
        }

        private IEnumerator<float> RemoveAll()
        {
            foreach (var item in spawnedList)
            {
                NGUITools.Destroy(item.CachedGameObject);
                yield return Timing.WaitForOneFrame;
            }

            foreach (var item in poolDic.Values)
            {
                while (item.Count > 0)
                {
                    NGUITools.Destroy(item.Pop().CachedGameObject);
                    yield return Timing.WaitForOneFrame;
                }
            }

            foreach (var item in parentDic.Values)
            {
                NGUITools.Destroy(item);
                yield return Timing.WaitForOneFrame;
            }

            poolDic.Clear();
            parentDic.Clear();
        }

        private void RemoveAllImmediate()
        {
            for (int i = spawnedList.Count - 1; i >= 0; i--)
            {
                PoolObject poolObject = spawnedList[i];
                poolObject.Release(); // Release 는 시켜주어야 한다

                NGUITools.Destroy(poolObject.CachedGameObject);
            }

            foreach (var item in poolDic.Values)
            {
                while (item.Count > 0)
                {
                    NGUITools.Destroy(item.Pop().CachedGameObject);
                }
            }

            foreach (var item in parentDic.Values)
            {
                NGUITools.Destroy(item);
            }

            poolDic.Clear();
            parentDic.Clear();
        }

        protected abstract Transform GetOriginal(string key);
        protected abstract PoolObject Create(string key);
    }
}