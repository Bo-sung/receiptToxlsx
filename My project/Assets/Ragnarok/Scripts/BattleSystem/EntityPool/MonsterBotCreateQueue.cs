using UnityEngine;

namespace Ragnarok
{
    public sealed class MonsterBotCreateQueue
    {
        private readonly BetterList<MonsterBotEntity> createQueue;
        private bool isFinished;

        public bool IsInProgress => !isFinished;

        public MonsterBotCreateQueue()
        {
            createQueue = new BetterList<MonsterBotEntity>();
        }

        public void Clear()
        {
            createQueue.Clear();
            isFinished = false;
        }

        public void Enqueue(MonsterBotEntity[] entities)
        {
            if (entities == null)
                return;

            for (int i = 0; i < entities.Length; i++)
            {
                Enqueue(entities[i]);
            }
        }

        public void Enqueue(MonsterBotEntity entity)
        {
            if (Remove(entity))
            {
                Debug.LogError("이미 대기열에 추가되어있는 몬스터 봇");
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
        public MonsterBotEntity Dequeue()
        {
            MonsterBotEntity result = createQueue[0];
            createQueue.RemoveAt(0);
            return result;
        }

        /// <summary>
        /// 생성 큐 제거
        /// </summary>
        public bool Remove(MonsterBotEntity entity)
        {
            int index = IndexOf(entity.BotServerIndex);
            if (index == -1)
                return false;

            createQueue.RemoveAt(index);
            return true;
        }

        private int IndexOf(int serverIndex)
        {
            for (int i = 0; i < createQueue.size; i++)
            {
                if (createQueue[i].BotServerIndex == serverIndex)
                    return i;
            }

            return -1;
        }
    }
}