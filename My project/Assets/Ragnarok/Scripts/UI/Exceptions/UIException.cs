using UnityEngine;

namespace Ragnarok
{
    public abstract class UIException : System.Exception
    {
        private const string TAG = nameof(UIException);

        public UIException()
        {
#if UNITY_EDITOR
            Debug.Log($"[{TAG}] {GetType().Name}");
#endif
        }

        public UIException(string message) : base(message)
        {
#if UNITY_EDITOR
            Debug.Log($"[{TAG}] {GetType().Name}: {nameof(message)} = {message}");
#endif
        }

        public abstract void Execute();
    }
}