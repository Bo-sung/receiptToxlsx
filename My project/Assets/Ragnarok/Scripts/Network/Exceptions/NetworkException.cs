using UnityEngine;

namespace Ragnarok
{
    public abstract class NetworkException : System.Exception
    {
        private const string TAG = nameof(NetworkException);

        public NetworkException()
        {
#if UNITY_EDITOR
            Debug.Log($"[{TAG}] {GetType().Name}"); 
#endif
        }

        public NetworkException(string message) : base(message)
        {
#if UNITY_EDITOR
            Debug.Log($"[{TAG}] {GetType().Name}: {nameof(message)} = {message}"); 
#endif
        }

        public abstract void Execute();
    }
}