using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 길드 미로 거석 정보 리스트
    /// </summary>
    public class GuildMazeRockList : BetterList<GuildMazeRockInfo>
    {
        public int Size => size;

        public void Add(int id, int xIndex, int zIndex, GuildMazeRockInfo.RockState rockState)
        {
            float x = xIndex * 1.17f;
            float z = zIndex * 1.17f;
            Vector3 position = new Vector3(x, 0f, z);
            Add(id, position, rockState);
        }

        public void Add(int rockId, short xIndex, short zIndex, byte rockState)
        {
            GuildMazeRockInfo.RockState state = rockState.ToEnum<GuildMazeRockInfo.RockState>();
            Add(rockId, xIndex, zIndex, state);
        }

        public void Add(int rockId, short xIndex, short zIndex, GuildMazeRockInfo.RockState rockState)
        {
            Vector3 position = Constants.Map.GuildMaze.GetBlockPosition(xIndex, zIndex);
            Add(rockId, position, rockState);
        }

        private void Add(int rockId, Vector3 position, GuildMazeRockInfo.RockState rockState)
        {
            GuildMazeRockInfo rockInfo = Get(rockId) ?? new GuildMazeRockInfo();
            rockInfo.Initialize(rockId, rockState, position);

            Add(rockInfo);
        }

        public GuildMazeRockInfo Get(int rockId)
        {
            return this.Find(e => e.Id == rockId);
        }

        public void ReleaseAll()
        {
            while (Size > 0)
            {
                Release(this[0].Id);
            }
            Release();
        }

        public void Release(int rockId)
        {
            var rockInfo = Get(rockId);
            if (rockInfo is null)
            {
                Debug.LogError($"Release 실패. Rock#{rockId} 존재하지 않음.");
                return;
            }

            rockInfo.SetState(GuildMazeRockInfo.RockState.Release);
            Remove(rockInfo);
        }
    }
}