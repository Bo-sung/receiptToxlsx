using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class BattleItemPool : BetterList<BattleItem>
    {
        private readonly Stack<BattleItem> pooledStack;
        private readonly BetterList<CreateTask> createQueue;

        public BattleItemPool()
        {
            pooledStack = new Stack<BattleItem>();
            createQueue = new BetterList<CreateTask>();
        }

        /// <summary>
        /// 데이터 제거
        /// </summary>
        public new void Clear()
        {
            while (size > 0)
            {
                Recycle(Pop());
            }

            pooledStack.Clear();
            createQueue.Clear();
        }

        public BattleItem Create(IBattleItemInput input)
        {
            BattleItem result = pooledStack.Count > 0 ? pooledStack.Pop() : new BattleItem();

            result.Initialize(input); // 초기화
            Add(result); // List에 관리

            return result;
        }

        /// <summary>
        /// 재활용
        /// </summary>
        public void Recycle(BattleItem input)
        {
            input.ResetData(); // 초기화
            pooledStack.Push(input); // Stack에 관리 (Pool)
        }

        /// <summary>
        /// 반환 - 고유 id 이용
        /// </summary>
        public BattleItem Find(int id)
        {
            for (int i = 0; i < size; i++)
            {
                if (buffer[i].Id == id)
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
        public IBattleItemInput Dequeue()
        {
            return createQueue.Pop();
        }

        /// <summary>
        /// 생성 큐에 추가
        /// </summary>
        public void EnqueueRange(IBattleItemInput[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                Enqueue(input[i]);
            }
        }

        /// <summary>
        /// 생성 큐에 추가
        /// </summary>
        public void Enqueue(IBattleItemInput input)
        {
            int id = input.Id;
            if (RemoveQueue(id))
            {
#if UNITY_EDITOR
                Debug.LogError($"이미 생성 스택이 존재: {nameof(id)} = {id}");
#endif
            }

            createQueue.Add(new CreateTask(input));
        }

        /// <summary>
        /// 생성 큐 업데이트 (중도 상태변환 처리)
        /// </summary>
        public bool UpdateQueueState(int id, byte state)
        {
            int index = GetIndexCreateTask(id);
            if (index == -1)
                return false;

            createQueue[index].SetState(state);
            return true;
        }

        /// <summary>
        /// 생성 큐에서 제거
        /// </summary>
        private bool RemoveQueue(int id)
        {
            int index = GetIndexCreateTask(id);
            if (index == -1)
                return false;

            createQueue.RemoveAt(index);
            return true;
        }

        private int GetIndexCreateTask(int id)
        {
            for (int i = 0; i < createQueue.size; i++)
            {
                if (createQueue[i].Id == id)
                    return i;
            }

            return -1;
        }

        private class CreateTask : IBattleItemInput
        {
            public int Id { get; private set; }
            public byte State { get; private set; }
            public short IndexX { get; private set; }
            public short IndexZ { get; private set; }

            public CreateTask(IBattleItemInput input)
            {
                Id = input.Id;
                State = input.State;
                IndexX = input.IndexX;
                IndexZ = input.IndexZ;
            }

            public void SetState(byte state)
            {
                State = state;
            }
        }
    }
}