using UnityEngine;

namespace Ragnarok
{
    public class GuildMazeNexusInfo
    {
        /// <summary>
        /// 타워 상태
        /// </summary>
        public enum TowerState
        {
            Idle = 0,       // 생존
            Destroyed = 1,  // 파괴됨
        }

        // 초기화용 데이터
        public int SavedHp { get; private set; } // 체력 
        public Vector3 SavedPosition { get; private set; } // 포지션

        public NexusEntity Entity { get; private set; }
        public NexusActor Actor => Entity?.GetActor() as NexusActor;
        public int TeamIndex { get; private set; }
        public TowerState State; // 상태

        public bool IsInvalid => (Entity is null);

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(int teamIndex, int hp, byte state, short xIndex, short zIndex)
        {
            this.TeamIndex = teamIndex;
            this.SavedHp = hp;
            this.State = state.ToEnum<TowerState>();
            this.SavedPosition = Constants.Map.GuildMaze.GetBlockPosition(xIndex, zIndex);
        }

        public void SetEntity(NexusEntity nexusEntity)
        {
            this.Entity = nexusEntity;
        }
    }
}