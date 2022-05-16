using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public abstract class MultiKeyEnumBaseType<T, K1, K2>
        where T : MultiKeyEnumBaseType<T, K1, K2>
    {
        protected static Dictionary<K1, Dictionary<K2, T>> dict = new Dictionary<K1, Dictionary<K2, T>>();

        public readonly K1 key1;
        public readonly K2 key2;

        public MultiKeyEnumBaseType(K1 key1, K2 key2)
        {
            this.key1 = key1;
            this.key2 = key2;

            if (!dict.ContainsKey(key1))
                dict[key1] = new Dictionary<K2, T>();
            dict[key1][key2] = (T)this;
        }

        protected static T GetByKey(K1 key1, K2 key2)
        {
            if (!dict.ContainsKey(key1) || !dict[key1].ContainsKey(key2))
            {
                Debug.LogError(string.Format("Not find key: {0},{1}, Check the static field: {2}", key1, key2, typeof(T)));
                return default;
            }
            return dict[key1][key2];
        }
    }
}
