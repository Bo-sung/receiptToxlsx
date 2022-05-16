using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class ResponseAwaiterQueue
    {
        private readonly Queue<TaskAwaiter> connectQueue; // 서버 응답 (연결 전용)
        private readonly Dictionary<string, Queue<ResponseAwaiter>> dic; // 서버 응답 (확장 전용)

        public ResponseAwaiterQueue()
        {
            connectQueue = new Queue<TaskAwaiter>();
            dic = new Dictionary<string, Queue<ResponseAwaiter>>(System.StringComparer.Ordinal);
        }

        /// <summary>
        /// 응답 해제 (단순 Clear 가 아닌 비어있는 Exception 호출)
        /// </summary>
        public void Clear()
        {
            ReleaseConnectTask();
            ReleaseExtensionTask();
        }

        /// <summary>
        /// 연결 관련 Task 해제
        /// </summary>
        public void ReleaseConnectTask()
        {
            while (connectQueue.Count > 0)
            {
                TaskAwaiter awaiter = connectQueue.Dequeue();
                awaiter.Complete(new EmptyNetworkException());
            }
        }

        /// <summary>
        /// 확장 응답 Task 해제
        /// </summary>
        public void ReleaseExtensionTask()
        {
            foreach (var item in dic.Values)
            {
                while (item.Count > 0)
                {
                    ResponseAwaiter awaiter = item.Dequeue();
                    awaiter.Complete(new EmptyNetworkException());
                }
            }

            dic.Clear();
        }

        public void Enqueue(TaskAwaiter item)
        {
            connectQueue.Enqueue(item);
        }

        public TaskAwaiter Dequeue()
        {
#if UNITY_EDITOR
            if (connectQueue.Count == 0)
            {
                Debug.LogError("[ResponseAwaiterQueue] 비어있는 큐 Dequeue 시도");
            }
#endif

            return connectQueue.Dequeue();
        }

        public void Enqueue(string key, ResponseAwaiter item)
        {
            if (!dic.ContainsKey(key))
                dic.Add(key, new Queue<ResponseAwaiter>());

            dic[key].Enqueue(item);
        }

        public ResponseAwaiter Dequeue(string key)
        {
            if (dic.ContainsKey(key))
            {
#if UNITY_EDITOR
                if (dic[key].Count == 0)
                {
                    Debug.LogError($"[ResponseAwaiterQueue] 비어있는 큐 Dequeue 시도: {nameof(key)} = {key}");
                }
#endif
                return dic[key].Dequeue();
            }

#if UNITY_EDITOR
            Debug.LogError($"[ResponseAwaiterQueue] 존재하지 않은 큐 Dequeue 시도: {nameof(key)} = {key}");
#endif
            return null;
        }
    } 
}