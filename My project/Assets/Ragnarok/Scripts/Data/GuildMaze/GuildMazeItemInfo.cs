using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="GuildMazeItemList"/>
    /// [길드 미로] 아이템 오브젝트
    /// </summary>
    public class GuildMazeItemInfo
    {
        public enum ItemState
        {
            None = 0,       // 없음
            Idle = 1,       // 맵에 있음
            Release = 2,    // 제거
        }

        private Vector3 position;
        public int Id { get; private set; }
        public ItemState State { get; private set; }
        public NavPoolObject Item { get; private set; }

        public bool IsInvalid => (Item is null);

        public void Initialize(int id, ItemState state, Vector3 pos)
        {
            this.Id = id;
            this.position = pos;

            if (Item is null)
            {
                IBattlePool battlePool = BattlePoolManager.Instance;
                Item = battlePool.SpawnGuildMazeItem();
            }

            SetState(state);
        }

        public void SetState(ItemState state)
        {
            this.State = state;

            Refresh();
        }

        public void Refresh()
        {
            if (IsInvalid)
                return;

            switch (State)
            {
                case ItemState.None:
                    Item.Hide();
                    break;

                case ItemState.Idle:
                    Item.Show();
                    Item.Warp(position);
                    break;

                case ItemState.Release:
                    Release();
                    break;
            }
        }

        private void Release()
        {
            if (IsInvalid)
            {
                Debug.LogError($"Release NULL Item#{Id}.");
                return;
            }

            Item.Release();
            Item = null;
        }
    }
}