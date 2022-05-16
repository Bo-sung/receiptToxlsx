using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class BattleTrapPool : BetterList<BattleTrap>
    {
        private readonly BattleTrapType trapType;
        private readonly Stack<BattleTrap> pooledStack;
        private readonly BetterList<CreateTask> createQueue;

        public BattleTrapPool(BattleTrapType trapType)
        {
            this.trapType = trapType;

            pooledStack = new Stack<BattleTrap>();
            createQueue = new BetterList<CreateTask>();
        }

        /// <summary>
        /// 데이터 제거
        /// </summary>
        public new void Clear()
        {
            pooledStack.Clear();
            createQueue.Clear();
        }

        /// <summary>
        /// 모든 Trap 재활용
        /// </summary>
        public void Recycle()
        {
            while (size > 0)
            {
                Recycle(base[0]);
            }
        }

        public BattleTrap Create(IBattleTrapInput input)
        {
            BattleTrap result = pooledStack.Count > 0 ? pooledStack.Pop() : new BattleTrap(trapType);

            result.Initialize(input); // 초기화
            Add(result); // List에 관리

            return result;
        }

        /// <summary>
        /// 재활용
        /// </summary>
        public void Recycle(BattleTrap input)
        {
            input.Initialize(BattleTrap.DEFAULT); // 초기화
            input.ResetData(); // 초기화
            pooledStack.Push(input); // Stack에 관리 (Pool)

            Remove(input);
        }

        /// <summary>
        /// 반환 - 고유 id 이용
        /// </summary>
        public BattleTrap Find(int id)
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
        public IBattleTrapInput Dequeue()
        {
            return createQueue.Pop();
        }

        /// <summary>
        /// 생성 큐에 추가
        /// </summary>
        public void EnqueueRange(IBattleTrapInput[] input)
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
        public void Enqueue(IBattleTrapInput input)
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
        /// 생성 큐 업데이트 (Index 처리)
        /// </summary>
        public bool UpdateQueueIndex(int id, short indexX, short indexZ)
        {
            int index = GetIndexCreateTask(id);
            if (index == -1)
                return false;

            createQueue[index].SetIndex(indexX, indexZ);
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

        private class CreateTask : IBattleTrapInput
        {
            public int Id { get; private set; }
            public byte State { get; private set; }
            public short IndexX { get; private set; }
            public short IndexZ { get; private set; }

            public CreateTask(IBattleTrapInput input)
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

            public void SetIndex(short indexX, short indexZ)
            {
                IndexX = indexX;
                IndexZ = indexZ;
            }
        }
    }
}