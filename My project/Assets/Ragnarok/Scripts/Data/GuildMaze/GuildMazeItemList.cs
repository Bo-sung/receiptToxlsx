using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 길드 미로 아이템 정보 리스트
    /// </summary>
    public class GuildMazeItemList : BetterList<GuildMazeItemInfo>
    {
        public int Size => size;

        public void Add(int id, int xIndex, int zIndex, GuildMazeItemInfo.ItemState rockState)
        {
            float x = xIndex * 1.17f;
            float z = zIndex * 1.17f;
            Vector3 position = new Vector3(x, 0f, z);
            Add(id, position, rockState);
        }

        public void Add(int itemId, short xIndex, short zIndex, byte itemState)
        {
            GuildMazeItemInfo.ItemState state = itemState.ToEnum<GuildMazeItemInfo.ItemState>();
            Add(itemId, xIndex, zIndex, state);
        }

        public void Add(int itemId, short xIndex, short zIndex, GuildMazeItemInfo.ItemState itemState)
        {
            Vector3 position = Constants.Map.GuildMaze.GetBlockPosition(xIndex, zIndex);
            Add(itemId, position, itemState);
        }

        private void Add(int itemId, Vector3 pos, GuildMazeItemInfo.ItemState itemState)
        {
            GuildMazeItemInfo itemInfo = Get(itemId) ?? new GuildMazeItemInfo();
            itemInfo.Initialize(itemId, itemState, pos);

            Add(itemInfo);
        }

        public GuildMazeItemInfo Get(int itemId)
        {
            return this.Find(e => e.Id == itemId);
        }

        public void ReleaseAll()
        {
            while (Size > 0)
            {
                Release(this[0].Id);
            }
            Release();
        }

        public void Release(int itemId)
        {
            var itemInfo = Get(itemId);
            if (itemInfo is null)
            {
                Debug.LogError($"Release 실패. Item#{itemId} 존재하지 않음.");
                return;
            }

            itemInfo.SetState(GuildMazeItemInfo.ItemState.Release);
            Remove(itemInfo);
        }
    }
}