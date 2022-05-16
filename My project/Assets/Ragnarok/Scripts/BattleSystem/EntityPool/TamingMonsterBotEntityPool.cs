using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class TamingMonsterBotEntityPool : BetterList<MonsterBotEntity>
    {
        private readonly Stack<MonsterBotEntity> pooledStack;
        private readonly BetterList<CreateTask> createQueue;
        private readonly Dictionary<int, int> createQueueIndexDic; // key: serverIndex, value: index

        public TamingMonsterBotEntityPool()
        {
            pooledStack = new Stack<MonsterBotEntity>();
            createQueue = new BetterList<CreateTask>();
            createQueueIndexDic = new Dictionary<int, int>(IntEqualityComparer.Default);
        }

        /// <summary>
        /// 데이터 제거
        /// </summary>
        public new void Clear()
        {
            base.Clear();

            pooledStack.Clear();
            createQueue.Clear();
            createQueueIndexDic.Clear();
        }

        /// <summary>
        /// Entity 생성
        /// </summary>
        public MonsterBotEntity Create(ITamingMonsterPotInput input)
        {
            MonsterBotEntity entity = pooledStack.Count > 0 ? pooledStack.Pop() : MonsterEntity.Factory.CreateMonsterBot();

            entity.Initialize(input); // 초기화
            Add(entity); // List에 관리

            return entity;
        }

        /// <summary>
        /// 모든 Entity 재활용
        /// </summary>
        public void Recycle()
        {
            while (size > 0)
            {
                Recycle(Pop());
            }
        }

        /// <summary>
        /// Entity 재활용
        /// </summary>
        public void Recycle(MonsterBotEntity entity)
        {
            entity.Initialize(MonsterBotEntity.TAMING); // 초기화
            entity.ResetData();
            pooledStack.Push(entity); // Stack에 관리 (Pool)
        }

        /// <summary>
        /// Entity 반환 - 서버 몬스터 고유 Index 이용
        /// </summary>
        public MonsterBotEntity Find(int serverIndex)
        {
            for (int i = 0; i < size; i++)
            {
                if (buffer[i].BotServerIndex == serverIndex)
                    return buffer[i];
            }

            return null;
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
        public ITamingMonsterPotInput Dequeue()
        {
            int removeIndex = createQueue.size - 1;
            ITamingMonsterPotInput result = createQueue[removeIndex];
            createQueueIndexDic.Remove(result.Index);
            createQueue.RemoveAt(removeIndex);
            return result;
        }

        /// <summary>
        /// 생성 큐에 추가
        /// </summary>
        public void EnqueueRange(ITamingMonsterPotInput[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                Enqueue(input[i]);
            }
        }

        /// <summary>
        /// 생성 큐에 추가
        /// </summary>
        public void Enqueue(ITamingMonsterPotInput input)
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
        public bool UpdateQueueState(int serverIndex, byte state)
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
        /// 생성 큐 없데이트 (중도 제거 처리)
        /// </summary>
        /// <param name="serverIndex"></param>
        /// <returns></returns>
        public bool UpdateQueueDelete(int serverIndex)
        {
            return RemoveQueue(serverIndex);
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

        private class CreateTask : ITamingMonsterPotInput
        {
            private int index;
            private MonsterType monsterType;
            private int id;
            private byte state;
            private int level;
            private float scale;

            MonsterType ISpawnMonster.Type => monsterType;
            int ISpawnMonster.Id => id;
            int ISpawnMonster.Level => level;
            float ISpawnMonster.Scale => scale;

            int ITamingMonsterPotInput.Index => index;
            byte ITamingMonsterPotInput.State => state;

            public CreateTask(ITamingMonsterPotInput input)
            {
                index = input.Index;
                monsterType = input.Type;
                id = input.Id;
                state = input.State;
                level = input.Level;
                scale = input.Scale;
            }

            public void SetState(byte state)
            {
                this.state = state;
            }         
        }
    }
}
