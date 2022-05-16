using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 1. Entity Pooling
    /// 2. 생성되기 전 바뀐 행동을 담아서 처리
    /// </summary>
    public sealed class MonsterBotEntityPool : BetterList<MonsterBotEntity>
    {
        private readonly Stack<MonsterBotEntity> pooledStack;
        private readonly BetterList<CreateTask> createQueue;
        private readonly Dictionary<int, int> createQueueIndexDic; // key: serverIndex, value: index

        public MonsterBotEntityPool()
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
        public MonsterBotEntity Create(IMonsterBotInput input)
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
            entity.Initialize(MonsterBotEntity.DEFAULT); // 초기화
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
        public IMonsterBotInput Dequeue()
        {
            int removeIndex = createQueue.size - 1;
            IMonsterBotInput result = createQueue[removeIndex];
            createQueueIndexDic.Remove(result.Index);
            createQueue.RemoveAt(removeIndex);
            return result;
        }

        /// <summary>
        /// 생성 큐에 추가
        /// </summary>
        public void EnqueueRange(IMonsterBotInput[] input)
        {
            if (input == null)
                return;

            for (int i = 0; i < input.Length; i++)
            {
                Enqueue(input[i]);
            }
        }

        /// <summary>
        /// 생성 큐에 추가
        /// </summary>
        public void Enqueue(IMonsterBotInput input)
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
        /// 생성 큐 업데이트 (중도 움직임 처리)
        /// </summary>
        public bool UpdateQueueMove(int serverIndex, Vector3 pos)
        {
            if (createQueueIndexDic.ContainsKey(serverIndex))
            {
                int index = createQueueIndexDic[serverIndex];
                createQueue[index].SetPosition(pos);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 생성 큐 업데이트 (중도 움직임 처리)
        /// </summary>
        public bool UpdateQueueMove(int serverIndex, Vector3 pos, Vector3 targetPos)
        {
            if (createQueueIndexDic.ContainsKey(serverIndex))
            {
                int index = createQueueIndexDic[serverIndex];
                createQueue[index].SetTargetPosition(pos, targetPos);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 생성 큐 업데이트 (중도 hp 변경 처리)
        /// </summary>
        public bool UpdateQueueHp(int serverIndex, int hp)
        {
            if (createQueueIndexDic.ContainsKey(serverIndex))
            {
                int index = createQueueIndexDic[serverIndex];
                createQueue[index].SetCurHp(hp);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 생성 큐 업데이트 (상태이상)
        /// </summary>
        public bool UpdateQueueCrowdControl(int serverIndex, CrowdControlType crowdControl)
        {
            if (createQueueIndexDic.ContainsKey(serverIndex))
            {
                int index = createQueueIndexDic[serverIndex];
                createQueue[index].SetCrowdControl(crowdControl);
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

        private class CreateTask : IMonsterBotInput
        {
            private int index;
            private MonsterType monsterType;
            private int id;
            private byte state;
            private float posX;
            private float posY;
            private float posZ;
            private bool hasTargetPos;
            private float savedTargetingTime;
            private float targetPosX;
            private float targetPosY;
            private float targetPosZ;
            private bool hasMaxHp;
            private int maxHp;
            private bool hasCurHp;
            private int curHp;
            private int level;
            private float scale;
            private float? moveSpeed;
            private CrowdControlType crowdControlType;

            int IMonsterBotInput.Index => index;
            byte IMonsterBotInput.State => state;
            float IMonsterBotInput.PosX => posX;
            float IMonsterBotInput.PosY => posY;
            float IMonsterBotInput.PosZ => posZ;
            bool IMonsterBotInput.HasTargetPos => hasTargetPos;
            float IMonsterBotInput.SavedTargetingTime => savedTargetingTime;
            float IMonsterBotInput.TargetPosX => targetPosX;
            float IMonsterBotInput.TargetPosY => targetPosY;
            float IMonsterBotInput.TargetPosZ => targetPosZ;
            bool IMonsterBotInput.HasMaxHp => hasMaxHp;
            int IMonsterBotInput.MaxHp => maxHp;
            bool IMonsterBotInput.HasCurHp => hasCurHp;
            int IMonsterBotInput.CurHp => curHp;
            CrowdControlType IMonsterBotInput.CrowdControl => crowdControlType;

            MonsterType ISpawnMonster.Type => monsterType;
            int ISpawnMonster.Id => id;
            int ISpawnMonster.Level => level;
            float ISpawnMonster.Scale => scale;
            float? IMonsterBotInput.MoveSpeed => moveSpeed;

            public CreateTask(IMonsterBotInput input)
            {
                index = input.Index;
                state = input.State;
                posX = input.PosX;
                posY = input.PosY;
                posZ = input.PosZ;
                hasTargetPos = input.HasTargetPos;
                savedTargetingTime = input.SavedTargetingTime;
                targetPosX = input.TargetPosX;
                targetPosY = input.TargetPosY;
                targetPosZ = input.TargetPosZ;
                hasMaxHp = input.HasMaxHp;
                maxHp = input.MaxHp;
                hasCurHp = input.HasCurHp;
                curHp = input.CurHp;
                crowdControlType = input.CrowdControl;

                monsterType = input.Type;
                id = input.Id;
                level = input.Level;
                scale = input.Scale;
                moveSpeed = input.MoveSpeed;
            }

            public void SetState(byte state)
            {
                this.state = state;
            }

            public void SetPosition(Vector3 pos)
            {
                savedTargetingTime = 0f;

                posX = pos.x;
                posY = pos.y;
                posZ = pos.z;
            }

            public void SetTargetPosition(Vector3 pos, Vector3 targetPos)
            {
                savedTargetingTime = Time.realtimeSinceStartup;

                posX = pos.x;
                posY = pos.y;
                posZ = pos.z;

                targetPosX = targetPos.x;
                targetPosY = targetPos.y;
                targetPosZ = targetPos.z;
            }

            public void SetCurHp(int hp)
            {
                curHp = hp;
            }

            public void SetCrowdControl(CrowdControlType crowdControlType)
            {
                this.crowdControlType = crowdControlType;
            }
        }
    }
}