using System;

namespace Ragnarok
{
    public class LifeCycle
    {
        public event Action OnDispose;

        public void Dispose()
        {
            OnDispose?.Invoke();
        }
    }
}
