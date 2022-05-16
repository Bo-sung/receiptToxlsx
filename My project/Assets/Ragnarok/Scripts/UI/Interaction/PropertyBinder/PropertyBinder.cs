using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    [ExecuteInEditMode]
    public abstract class PropertyBinder<T> : MonoBehaviour, IEqualityComparer<T>
    {
        [SerializeField]
        bool isReverseInputValue;

        public T Value
        {
            get
            {
                return Get();
            }
            set
            {
                T input = isReverseInputValue ? ToReverseValue(value) : value;
                
                if (Equals(input, Value))
                    return;

#if UNITY_EDITOR
                Debug.Log($"[{GetType()}] PropertyBinder에 의해 변경: {nameof(input)} = {input}", this);
#endif
                Set(input);
            }
        }

        protected virtual T ToReverseValue(T value)
        {
            return value;
        }

        protected abstract T Get();

        protected abstract void Set(T value);

        public abstract bool Equals(T x, T y);

        public abstract int GetHashCode(T obj);
    }
}