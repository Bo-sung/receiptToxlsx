namespace Ragnarok
{
    public abstract class Singleton<T>
       where T : new()
    {
        private static readonly object obj = new object();

        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (obj)
                    {
                        instance = new T();
                    }
                }

                return instance;
            }
        }

        public Singleton()
        {
            SceneLoader.OnTitleSceneLoaded += OnTitle;
        }

        ~Singleton()
        {
            SceneLoader.OnTitleSceneLoaded -= OnTitle;
        }

        protected abstract void OnTitle();
    }
}