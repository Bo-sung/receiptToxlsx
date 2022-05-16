using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 큐브조각 생성하기 전의 상태를 담아서 처리
    /// </summary>
    public sealed class MazeCubeStatePool
    {
        public class Info
        {
            public int serverIndex;
            public Vector3 position;
            public MazeCubeState state;
            private IMazeDropItem dropItem;

            public void SetMazeDropItem(IMazeDropItem obj)
            {
                ReleaseMazeDropItem();
                AddPoolObject(obj);
            }

            public void ReleaseMazeDropItem()
            {
                if (dropItem == null)
                    return;

                dropItem.Release();
            }

            public void StartEffect()
            {
                if (dropItem == null)
                    return;

                dropItem.StartEffect();
            }

            public void SetPosition(Vector3 pos)
            {
                position = pos;
            }

            private void AddPoolObject(IMazeDropItem obj)
            {
                obj.OnDeSpawn += RemovePoolObject;
                dropItem = obj;
            }

            private void RemovePoolObject(IMazeDropItem obj)
            {
                dropItem = null;
                obj.OnDeSpawn -= RemovePoolObject;
            }
        }

        private readonly Dictionary<int, Info> dic;
        private readonly BetterList<CreateTask> createQueue;
        private readonly Dictionary<int, int> createQueueIndexDic; // key: serverIndex, value: index

        public MazeCubeStatePool()
        {
            dic = new Dictionary<int, Info>(IntEqualityComparer.Default);
            createQueue = new BetterList<CreateTask>();
            createQueueIndexDic = new Dictionary<int, int>(IntEqualityComparer.Default);
        }

        /// <summary>
        /// 데이터 제거
        /// </summary>
        public void Clear()
        {
            foreach (var item in dic.Values)
            {
                item.ReleaseMazeDropItem();
            }

            dic.Clear();
            createQueue.Clear();
            createQueueIndexDic.Clear();
        }

        /// <summary>
        /// 오브젝트 생성
        /// </summary>
        public Info Create(IMazeCubeStateInfo input)
        {
            int serverIndex = input.Index;
            if (!dic.ContainsKey(serverIndex))
                dic.Add(serverIndex, new Info());

            return dic[serverIndex];
        }

        /// <summary>
        /// Object 반환 - 서버 미로퀘스트조각 고유 Index 이용
        /// </summary>
        public Info Find(int serverIndex)
        {
            return dic.ContainsKey(serverIndex) ? dic[serverIndex] : null;
        }

        /// <summary>
        /// 생성 큐 존재
        /// </summary>
        public bool HasQueue()
        {
            return createQueue.size > 0;
        }

        /// <summary>
        /// 생성 큐 꺼내기
        /// </summary>
        public IMazeCubeStateInfo Dequeue()
        {
            int removeIndex = createQueue.size - 1;
            IMazeCubeStateInfo result = createQueue[removeIndex];
            createQueueIndexDic.Remove(result.Index);
            createQueue.RemoveAt(removeIndex);
            return result;
        }

        /// <summary>
        /// 생성 큐에 추가
        /// </summary>
        public void EnqueueRange(IMazeCubeStateInfo[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                Enqueue(input[i]);
            }
        }

        /// <summary>
        /// 생성 큐에 추가
        /// </summary>
        public void Enqueue(IMazeCubeStateInfo input)
        {
            int serverIndex = input.Index;
            if (RemoveQueue(serverIndex))
            {
#if UNITY_EDITOR
                Debug.LogError($"이미 생성 스택이 존재: {nameof(serverIndex)} = {serverIndex}");
#endif
            }

            createQueueIndexDic.Add(serverIndex, createQueue.size); // 추가
            createQueue.Add(new CreateTask(input));
        }

        /// <summary>
        /// 생성 큐 업데이트 (중도 상태변경 처리)
        /// </summary>
        public bool UpdateQueueState(int serverIndex, MazeCubeState state)
        {
            if (createQueueIndexDic.ContainsKey(serverIndex))
            {
                int index = createQueueIndexDic[serverIndex];
                createQueue[index].SetState(state);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 생성 큐에서 제거
        /// </summary>
        private bool RemoveQueue(int serverIndex)
        {
            if (createQueueIndexDic.ContainsKey(serverIndex))
            {
                int index = createQueueIndexDic[serverIndex];
                createQueue.RemoveAt(index); // 생성 stack 제거
                createQueueIndexDic.Remove(serverIndex); // index 제거
                return true;
            }

            return false;
        }

        private class CreateTask : IMazeCubeStateInfo
        {
            private int index;
            private float posX;
            private float posY;
            private float posZ;
            private MazeCubeState state;

            int IMazeCubeStateInfo.Index => index;
            float IMazeCubeStateInfo.PosX => posX;
            float IMazeCubeStateInfo.PosY => posY;
            float IMazeCubeStateInfo.PosZ => posZ;
            MazeCubeState IMazeCubeStateInfo.State => state;

            public CreateTask(IMazeCubeStateInfo input)
            {
                index = input.Index;
                posX = input.PosX;
                posY = input.PosY;
                posZ = input.PosZ;
                state = input.State;
            }

            public void SetState(MazeCubeState state)
            {
                this.state = state;
            }
        }
    }
}