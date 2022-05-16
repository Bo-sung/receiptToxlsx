using UnityEngine;

namespace Ragnarok
{
    public sealed class PlayerBotCreateQueue
    {
        private readonly BetterList<PlayerBotEntity> createQueue;
        private bool isFinished;

        public bool IsInProgress => !isFinished;

        public PlayerBotCreateQueue()
        {
            createQueue = new BetterList<PlayerBotEntity>();
        }

        public void Clear()
        {
            createQueue.Release();
            isFinished = false;
        }

        public void Enqueue(PlayerBotEntity[] entities)
        {
            if (entities == null)
                return;

            for (int i = 0; i < entities.Length; i++)
            {
                Enqueue(entities[i]);
            }
        }

        public void Enqueue(PlayerBotEntity entity)
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
        public PlayerBotEntity Dequeue()
        {
            PlayerBotEntity result = createQueue[0];
            createQueue.RemoveAt(0);
            return result;
        }

        /// <summary>
        /// 생성 큐 제거
        /// </summary>
        public bool Remove(PlayerBotEntity entity)
        {
            int index = IndexOf(entity.Character.Cid);
            if (index == -1)
                return false;

            createQueue.RemoveAt(index);
            return true;
        }

        private int IndexOf(int cid)
        {
            for (int i = 0; i < createQueue.size; i++)
            {
                if (createQueue[i].Character.Cid == cid)
                    return i;
            }

            return -1;
        }
    }
}