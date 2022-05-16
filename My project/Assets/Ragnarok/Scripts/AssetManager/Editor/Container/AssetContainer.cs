using UnityEngine;
using System.Collections.Generic;

namespace Ragnarok
{
    public interface IAssetContainer
    {
        void Ready();
        void Clear();
    }

    public abstract class AssetContainer<TKey, TValue> : ScriptableObject, IAssetContainer
        where TValue : Object
    {
        [SerializeField]
        [Tooltip("어셋번들에 포함시킬 어셋 배열")]
        protected TValue[] array;

        Dictionary<TKey, TValue> dic;

        public virtual void Ready()
        {
            dic = new Dictionary<TKey, TValue>(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    Debug.LogError($"name = {name}, type = {typeof(TValue)}, i = {i}");
                    continue;
                }

                TKey key = ConvertKey(array[i]);
                if (dic.ContainsKey(key))
                {
                    Debug.LogError($"name = {name}, type = {typeof(TValue)}, key = {key}, i = {i}");
                    continue;
                }

                dic.Add(ConvertKey(array[i]), array[i]);
            }
        }

        public virtual void Clear()
        {
            dic.Clear();
        }

        public virtual TValue Get(TKey key, bool isLog = true)
        {
            if (!dic.ContainsKey(key))
            {
                if (isLog)
                    Debug.Log($"존재하지 않는 key: name = {name}, type = {typeof(TValue)}, {nameof(key)} = {key}");

                return null;
            }

            return dic[key];
        }

        public TValue[] GetArray()
        {
            return array;
        }

        protected abstract TKey ConvertKey(TValue t);
        protected abstract IEqualityComparer<TKey> GetComparer();

#if UNITY_EDITOR
        protected void CopyAllItems()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (TValue item in array)
            {
                sb.Append(ConvertKey(item)).AppendLine();
            }

            // Copy To Clipboard
            GUIUtility.systemCopyBuffer = sb.ToString();
            Debug.Log(sb);
        }

        protected void RemoveEmptyItem()
        {
            List<int> emptyIndexList = new List<int>();
            for (int i = array.Length - 1; i >= 0; i--)
            {
                if (array[i] == null)
                    emptyIndexList.Add(i);
            }
            foreach (var item in emptyIndexList)
            {
                UnityEditor.ArrayUtility.RemoveAt(ref array, item);
            }
            NGUITools.SetDirty(this);
        }
#endif
    }

    //[CreateAssetMenu(fileName = "Container", menuName = "AssetBundle/Container/{NAME}")]
    public abstract class StringAssetContainer<T> : AssetContainer<string, T>
        where T : Object
    {
        protected override IEqualityComparer<string> GetComparer()
        {
            return System.StringComparer.Ordinal;
        }
    }

    public abstract class IntAssetContainer<T> : AssetContainer<int, T>
        where T : Object
    {
        protected override IEqualityComparer<int> GetComparer()
        {
            return IntEqualityComparer.Default;
        }
    }
}