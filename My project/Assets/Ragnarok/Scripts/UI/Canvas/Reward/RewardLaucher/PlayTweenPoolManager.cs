using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class PlayTweenPoolManager : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] PlayTweenPoolObject poolObject;

        Transform myTransform;
        Stack<PlayTweenPoolObject> pooledStack;
        BetterList<PlayTweenPoolObject> spawnedList;

        public event System.Action OnDespawn;

        void Awake()
        {
            myTransform = transform;
            pooledStack = new Stack<PlayTweenPoolObject>();
            spawnedList = new BetterList<PlayTweenPoolObject>();
        }

        void Start()
        {
            poolObject.SetActive(false);
        }

        public void Clear()
        {
            while (pooledStack.Count > 0)
            {
                PlayTweenPoolObject obj = pooledStack.Pop();
                NGUITools.Destroy(obj.gameObject);
            }

            while (spawnedList.size > 0)
            {
                PlayTweenPoolObject obj = spawnedList.Pop();
                NGUITools.Destroy(obj.gameObject);
            }
        }

        public PlayTweenPoolObject Spawn()
        {
            PlayTweenPoolObject spawned = pooledStack.Count > 0 ? pooledStack.Pop() : Instantiate(poolObject, myTransform, worldPositionStays: false);
            spawnedList.Add(spawned);
            spawned.SetActive(true);

            spawned.ResetDelay(poolObject);
            spawned.OnFinish += Despawn;
            return spawned;
        }

        private void Despawn(PlayTweenPoolObject obj)
        {
            obj.OnFinish -= Despawn;

            obj.SetActive(false);
            pooledStack.Push(obj);
            spawnedList.Remove(obj);

            OnDespawn?.Invoke();
        }
    }
}