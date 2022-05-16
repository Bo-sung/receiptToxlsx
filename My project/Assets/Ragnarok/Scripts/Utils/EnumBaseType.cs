using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ragnarok
{
    public abstract class EnumBaseType<T> where T : EnumBaseType<T>
    {
        protected static List<T> enumValues = new List<T>();

        public readonly int Key;
        public readonly string Value;

        public EnumBaseType(int key, string value)
        {
            Key = key;
            Value = value;
            enumValues.Add((T)this);
        }

        protected static ReadOnlyCollection<T> GetBaseValues()
        {
            return enumValues.AsReadOnly();
        }

        protected static T GetBaseByKey(int key)
        {
            foreach (T t in enumValues)
            {
                if (t.Key == key) return t;
            }
            UnityEngine.Debug.LogError(string.Format("Not find key: {0}, Check the static field: {1}", key, typeof(T)));
            return null;
        }

        public override string ToString()
        {
            return string.Concat(Key, ":", Value);
        }
    }

    public abstract class EnumBaseType<T, TKey, TValue>
        where T : EnumBaseType<T, TKey, TValue>
    {
        protected static List<T> enumValues = new List<T>();

        public readonly TKey Key;
        public readonly TValue Value;

        public EnumBaseType(TKey key, TValue value)
        {
            Key = key;
            Value = value;
            enumValues.Add((T)this);
        }

        protected static ReadOnlyCollection<T> GetBaseValues()
        {
            return enumValues.AsReadOnly();
        }

        protected static T GetBaseByKey(TKey key)
        {
            foreach (T t in enumValues)
            {
                if (t.Key.Equals(key)) return t;
            }
            UnityEngine.Debug.LogError(string.Format("Not find key: {0}, Check the static field: {1}", key, typeof(T)));
            return null;
        }
        protected static bool ContainsKey(TKey key)
        {
            foreach (T t in enumValues)
            {
                if (t.Key.Equals(key)) return true;
            }
            return false;
        }

        public override string ToString()
        {
            return string.Concat(Key, ":", Value);
        }
    }
}
