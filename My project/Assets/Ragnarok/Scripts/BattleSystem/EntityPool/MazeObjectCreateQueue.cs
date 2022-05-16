using UnityEngine;

namespace Ragnarok
{
    public sealed class MazeObjectCreateQueue
    {
        private readonly BetterList<MazeObjectEntity> createQueue;
        private bool isFinished;

        public bool IsInProgress => !isFinished;

        public MazeObjectCreateQueue()
        {
            createQueue = new BetterList<MazeObjectEntity>();
        }

        public void Clear()
        {
            createQueue.Release();
            isFinished = false;
        }

        public void Enqueue(MazeObjectEntity[] entities)
        {
            if (entities == null)
                return;

            for (int i = 0; i < entities.Length; i++)
            {
                Enqueue(entities[i]);
            }
        }

        public void Enqueue(MazeObjectEntity entity)
        {
            if (Remove(entity))
            {
                Debug.LogError("이미 대기열에 추가되어있는 플레이어 봇");
            }

            createQueue.Add(entity);
        }

        public bool HasQueue()
        {
            return createQueue.size > 0;
        }

        public void Ready()
        {
            isFinished = false;
        }

        public void Finish()
        {
            isFinished = true;
        }

        /// <summary>
        /// 생성 큐 꺼내기
        /// </summary>
        public MazeObjectEntity Dequeue()
        {
            MazeObjectEntity result = createQueue[0];
            createQueue.RemoveAt(0);
            return result;
        }

        /// <summary>
        /// 생성 큐 제거
        /// </summary>
        public bool Remove(MazeObjectEntity entity)
        {
            int index = IndexOf(entity.ServerIndex);
            if (index == -1)
                return false;

            createQueue.RemoveAt(index);
            return true;
        }

        private int IndexOf(int index)
        {
            for (int i = 0; i < createQueue.size; i++)
            {
                if (createQueue[i].ServerIndex == index)
                    return i;
            }

            return -1;
        }
    }
}