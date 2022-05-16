using UnityEngine;
using System.Collections.Generic;

namespace Ragnarok
{
    public class ObjectPool<T>
        where T : new()
    {
        private readonly Stack<T> m_Stack;
        private readonly System.Action<T> m_ActionOnGet;
        private readonly System.Action<T> m_ActionOnRelease;

        public int CountAll { get; private set; }
        public int CountActive { get { return CountAll - CountInactive; } }
        public int CountInactive { get { return m_Stack.Count; } }

        public ObjectPool(System.Action<T> actionOnGet = null, System.Action<T> actionOnRelease = null)
        {
            m_Stack = new Stack<T>();
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
        }

        public T Get()
        {
            T element;

            if (m_Stack.Count == 0)
            {
                element = new T();
                CountAll++;
            }
            else
            {
                element = m_Stack.Pop();
            }

            m_ActionOnGet?.Invoke(element);

            return element;
        }

        public void Release(T element)
        {
            if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");

            m_ActionOnRelease?.Invoke(element);

            m_Stack.Push(element);
        }
    }
}