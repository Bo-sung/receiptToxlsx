using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ragnarok
{
    public abstract class GameObjectSingleton<T> : MonoBehaviour where T : Component
    {
        /// <summary>
        /// The instance.
        /// </summary>
        private static T instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        instance = obj.AddComponent<T>();
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Use this for initialization.
        /// </summary>
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                SceneManager.sceneLoaded += OnSceneLoaded;

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.buildIndex == 1)
                OnTitle();
        }

        protected abstract void OnTitle();
    }
}