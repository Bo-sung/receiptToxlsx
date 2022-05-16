using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class Buffer<T> : BetterList<T>, IEnumerable<T>
    {
        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                Add(item);
            }

            ToArray();
        }

        public T[] GetBuffer(bool isAutoRelease)
        {
            if (size == 0)
                return System.Array.Empty<T>();

            T[] result = new T[size];

            T[] buffer = ToArray();
            buffer.CopyTo(result, 0);

            if (isAutoRelease)
                Release();

            return result;
        }

        public T Dequeue(bool isAutoRelease)
        {
            if (size == 0)
                return default;

            T result = base[0];

            if (isAutoRelease)
            {
                Release();
            }
            else
            {
                RemoveAt(0);
            }

            return result;
        }

        public T GetRandomPop(bool isAutoRelease)
        {
            if (size == 0)
                return default;

            int index = Random.Range(0, size);
            T result = base[index];

            if (isAutoRelease)
            {
                Release();
            }
            else
            {
                RemoveAt(index);
            }

            return result;
        }

        public T[] GetRandomPop(int randomCount, bool isAutoRelease)
        {
            // 랜덤하게 뽑을 필요가 음슴
            if (size <= randomCount)
                return GetBuffer(isAutoRelease);

            T[] result = new T[randomCount];

            for (int i = 0; i < randomCount; i++)
            {
                int index = Random.Range(0, size);
                result[i] = base[index]; // 해당 item 세팅
                RemoveAt(index); // 해당 item 제거
            }

            if (isAutoRelease)
                Release();

            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}