using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="GuildMazeRockList"/>
    /// [길드 미로] 거석 오브젝트
    /// </summary>
    public class GuildMazeRockInfo
    {
        /// <summary>
        /// 거석 상태 (진로 방해 큐브)
        /// </summary>
        public enum RockState
        {
            None = 0,       // 없음
            Ready = 1,      // 출현 대기
            Blocked = 2,    // 출현 (진로 방해)
            Blocked_Duplicated = 3, // = 출현 (Blocked로 전환해줘야함)
            Release,        // 제거
        }

        public NavPoolObjectWithReady Rock { get; private set; }
        public int Id { get; private set; } = -1;
        public RockState State { get; private set; }
        private Vector3 lastPosition;

        public bool IsInvalid => (Rock is null);

        public void Initialize(int rockId, RockState rockState, Vector3 pos)
        {
            this.lastPosition = pos;
            this.Id = rockId;

            if (Rock is null)
            {
                IBattlePool battlePool = BattlePoolManager.Instance;
                this.Rock = battlePool.SpawnGuildMazeRock();
            }

            SetState(rockState);
        }

        public void SetState(RockState state)
        {
            // 임시 변환 작업 .. (3번 State가 사라져야한다)
            if (state == RockState.Blocked_Duplicated)
                state = RockState.Blocked;

            this.State = state;

            Refresh();
        }

        public void Refresh()
        {
            if (IsInvalid)
                return;

            switch (State)
            {
                case RockState.None:
                    break;

                case RockState.Ready: // 출현 대기 상태
                    Rock.Warp(lastPosition);
                    Rock.ShowReady();
                    Rock.HideMain();
                    break;

                case RockState.Blocked:
                    Rock.Warp(lastPosition);
                    Rock.HideReady();
                    Rock.ShowMain();
                    break;

                case RockState.Release:
                    Release();
                    break;
            }
        }

        private void Release()
        {
            if (IsInvalid)
            {
                Debug.LogError($"Release NULL Rock#{Id}.");
                return;
            }

            Rock.Release();
            Rock = null;
        }
    }
}