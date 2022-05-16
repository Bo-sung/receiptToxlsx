using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class MazeObjectPoolManager<T> : Singleton<MazeObjectPoolManager<T>>
        where T : MazeObjectEntity, new()
    {
        private readonly BetterList<T> entityList; // 사용중인 entity List (Player)
        private readonly Stack<T> entityPooledStack; // 사용대기중인(사용 중 x) entity Stack (Player)

        public MazeObjectPoolManager()
        {
            entityList = new BetterList<T>();
            entityPooledStack = new Stack<T>();
        }

        protected override void OnTitle()
        {
            Clear();
        }

        /// <summary>
        /// 데이터 삭제
        /// </summary>
        public void Clear()
        {
            entityList.Release();
            entityPooledStack.Clear();
        }

        /// <summary>
        /// 사용중인 모든 Entity 재활용
        /// </summary>
        public void Recycle()
        {
            while (entityList.size > 0)
            {
                Recycle(entityList[0]);
            }
        }

        /// <summary>
        /// 사용중인 Entity 회수
        /// </summary>
        public void Recycle(T entity)
        {
            if (entityList.Remove(entity))
            {
                entity.Initialize(MazeObjectEntity.DEFAULT); // 초기화
                entityPooledStack.Push(entity); // Stack에 관리 (Pool)
            }
        }

        /// <summary>
        /// 회수
        /// </summary>
        public void Despawn()
        {
            for (int i = 0; i < entityList.size; i++)
            {
                entityList[i].Despawn();
            }
        }

        /// <summary>
        /// 오브젝트 생성
        /// </summary>
        public void Create(IMazeCubeStateInfo[] inputs)
        {
            if (inputs == null)
                return;

            for (int i = 0; i < inputs.Length; i++)
            {
                Create(inputs[i]);
            }
        }

        /// <summary>
        /// 오브젝트 생성
        /// </summary>
        public T Create(IMazeCubeStateInfo input)
        {
            T entity = entityPooledStack.Count > 0 ? entityPooledStack.Pop() : new T();

            entity.Initialize(input);
            entityList.Add(entity); // List에 관리

            return entity;
        }

        /// <summary>
        /// 사용중인 오브젝트 반환
        /// </summary>
        public T[] GetMazeObjects()
        {
            return entityList.ToArray() ?? System.Array.Empty<T>();
        }

        /// <summary>
        /// 사용중인 오브젝트 반환 (key: index)
        /// </summary>
        public T Find(int index)
        {
            for (int i = 0; i < entityList.size; i++)
            {
                if (entityList[i].ServerIndex == index)
                    return entityList[i];
            }

            return null;
        }

        /// <summary>
        /// 오브젝트 상태 변화
        /// </summary>
        public T UpdatePlayerState(int cid, MazeCubeState state)
        {
            T finded = Find(cid);
            if (finded == null)
                return null;

            finded.SetState(state);
            return finded;
        }
    }
}