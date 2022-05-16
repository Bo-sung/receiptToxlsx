using UnityEngine;

namespace Ragnarok
{
    public abstract class MazeObjectEntity
    {
        public readonly static IMazeCubeStateInfo DEFAULT = new DefaultMazeObjectInfo();

        private class DefaultMazeObjectInfo : IMazeCubeStateInfo
        {
            int IMazeCubeStateInfo.Index => -1;
            float IMazeCubeStateInfo.PosX => 0f;
            float IMazeCubeStateInfo.PosY => 0f;
            float IMazeCubeStateInfo.PosZ => 0f;
            MazeCubeState IMazeCubeStateInfo.State => default;
        }

        protected readonly IBattlePool battlePool;

        public int ServerIndex { get; private set; }
        public Vector3 Position { get; private set; }
        public MazeCubeState State { get; private set; }

        private IMazeDropItem mazeDropItem;

        protected abstract MazeRewardType MazeRewardType { get; }
        protected virtual MazeBattleType MazeBattleType => MazeBattleType.None;

        public MazeObjectEntity()
        {
            battlePool = BattlePoolManager.Instance;
        }

        public MazeObjectEntity(IMazeCubeStateInfo input)
        {
            Initialize(input);
        }

        public void Initialize(IMazeCubeStateInfo input)
        {
            ServerIndex = input.Index;
            SetPosition(input.PosX, input.PosY, input.PosZ);
            SetState(input.State);

            Despawn();
        }

        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }

        public void SetPosition(Vector3 pos)
        {
            Position = pos;
        }

        public void SetState(MazeCubeState state)
        {
            State = state;
        }

        public IMazeDropItem Spawn(Vector3 pos)
        {
            if (mazeDropItem == null)
            {
                mazeDropItem = battlePool.SpawnMazeDropItem(pos, MazeRewardType);
                mazeDropItem.OnDeSpawn += OnDespawnMazeDropItem;
            }

            return mazeDropItem;
        }

        public void Despawn()
        {
            if (mazeDropItem == null)
                return;

            mazeDropItem.Release();
        }

        public IMazeDropItem GetMazeObjectItem()
        {
            return mazeDropItem;
        }

        void OnDespawnMazeDropItem(IMazeDropItem item)
        {
            item.OnDeSpawn -= OnDespawnMazeDropItem;
            mazeDropItem = null;
        }
    }
}