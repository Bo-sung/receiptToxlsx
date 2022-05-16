using System;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// 현재 받고 있는 버프 아이템 리스트
    /// </summary>
    public class BuffItemListModel : CharacterEntityModel
    {
        public interface IInputBuffValue
        {
            int Cid { get; }
            int ItemId { get; }
            long CoolDown { get; }
            long Duration { get; }
        }

        private readonly ItemDataManager itemDataRepo;

        /// <summary>
        /// 사용중인 아이템 정보 리스트
        /// </summary>
        private readonly List<UseItemInfo> useItemList;

        /// <summary>
        /// 버프 정보 변경 시 호출
        /// </summary>
        public event Action OnUpdateBuff;

        public BuffItemListModel()
        {
            itemDataRepo = ItemDataManager.Instance;
            useItemList = new List<UseItemInfo>();
        }

        public override void AddEvent(UnitEntityType type)
        {
            if (Entity.Inventory != null)
            {
                Entity.Inventory.OnUpdateItem += UpdateEndCooldown;
            }
        }

        public override void RemoveEvent(UnitEntityType type)
        {
            if (Entity.Inventory != null)
            {
                Entity.Inventory.OnUpdateItem -= UpdateEndCooldown;
            }
        }

        public override void ResetData()
        {
            base.ResetData();

            useItemList.Clear();
        }

        /// <summary>
        /// 사용중인 아이템 정보
        /// </summary>
        internal void Initialize(IInputBuffValue[] buffs)
        {
            useItemList.Clear();
            foreach (var item in buffs.OrEmptyIfNull())
            {
                AddUsedBuffItem(item);
            }

            UpdateEndCooldown();
            OnUpdateBuff?.Invoke();
        }

        /// <summary>
        /// 사용중인 버프 아이템 업데이트
        /// </summary>
        internal void UpdateData(UpdateBuffPacket[] inputBuffs)
        {
            if (inputBuffs == null)
                return;

            foreach (var item in inputBuffs)
            {
                switch (item.dirtyType)
                {
                    case DirtyType.Insert:
                        AddUsedBuffItem(item.buffPacket);
                        break;

                    case DirtyType.Update:
                        UpdateUsedBuffItem(item.buffPacket);
                        break;

                    case DirtyType.Delete:
                        DeleteUsedBuffItem(item.buffPacket);
                        break;
                }
            }

            UpdateEndCooldown();
            OnUpdateBuff?.Invoke();
        }

        /// <summary>
        /// 현재 받고 있는 버프 효과
        /// </summary>
        public List<UseItemInfo> GetBuffItemInfoList()
        {
            return useItemList.FindAll(info => info.RemainDuration > 0f || info.IsInfinity);
        }

        /// <summary>
        /// 현재 받고 있는 버프 효과
        /// </summary>
        public UseItemInfo[] GetBuffItemInfos()
        {
            return GetBuffItemInfoList().ToArray();
        }

        /// <summary>
        /// 유효하지 않은 사용중인 아이템 정보 제거
        /// </summary>
        public void RemoveInvalidBuffItem()
        {
            useItemList.RemoveAll(IsInvalidItemInfo);
        }

        /// <summary>
        /// 사용중인 아이템 정보 추가
        /// </summary>
        private void AddUsedBuffItem(IInputBuffValue buff)
        {
            var useItem = new UseItemInfo();
            useItem.Initialize(buff);
            useItem.SetData(itemDataRepo.Get(buff.ItemId));
            useItemList.Add(useItem);
        }

        /// <summary>
        /// 사용중인 아이템 정보 업데이트
        /// </summary>
        private void UpdateUsedBuffItem(IInputBuffValue buff)
        {
            UseItemInfo oldBuff = useItemList.Find(a => a.CID == buff.Cid && a.ItemId == buff.ItemId);

            if (oldBuff == null)
            {
                AddUsedBuffItem(buff);
                return;
            }

            oldBuff.UpdateTime(buff.CoolDown, buff.Duration);
        }

        /// <summary>
        /// 사용중인 아이템 정보 제거
        /// </summary>
        private void DeleteUsedBuffItem(IInputBuffValue buff)
        {
            UseItemInfo oldBuff = useItemList.Find(a => a.CID == buff.Cid && a.ItemId == buff.ItemId);
            useItemList.Remove(oldBuff);
        }

        /// <summary>
        /// 쿨타임 정보 업데이트
        /// </summary>
        private void UpdateEndCooldown()
        {
            if (Entity.Inventory == null)
                return;

            for (int i = 0; i < useItemList.Count; i++)
            {
                Entity.Inventory.SetEndCooldown(useItemList[i].ItemId, useItemList[i].RemainCoolDown);
            }
        }

        /// <summary>
        /// 유효성 검사
        /// </summary>
        private bool IsInvalidItemInfo(UseItemInfo effect)
        {
            return !effect.IsValid();
        }
    }
}