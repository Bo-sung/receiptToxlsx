using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIBuffInfo"/>
    /// </summary>
    public sealed class BuffInfoPresenter : ViewPresenter, ConsumableInfoSlot.Impl
    {
        public interface IView
        {
            void Refresh();
        }

        private readonly IView view;
        private readonly BuffItemListModel buffItemListModel;
        private readonly EventBuffModel eventBuffModel;
        private readonly InventoryModel inventoryModel;
        private readonly QuestModel questModel;

        public BuffInfoPresenter(IView view)
        {
            this.view = view;

            buffItemListModel = Entity.player.BuffItemList;
            eventBuffModel = Entity.player.EventBuff;
            inventoryModel = Entity.player.Inventory;
            questModel = Entity.player.Quest;
        }

        public override void AddEvent()
        {
            buffItemListModel.OnUpdateBuff += view.Refresh;
            eventBuffModel.OnUpdateEventBuff += view.Refresh;
            inventoryModel.OnUpdateItem += view.Refresh;
        }

        public override void RemoveEvent()
        {
            buffItemListModel.OnUpdateBuff -= view.Refresh;
            eventBuffModel.OnUpdateEventBuff -= view.Refresh;
            inventoryModel.OnUpdateItem -= view.Refresh;
        }

        public UIApplyBuffContent.IBuffInfo[] GetBuffInfos()
        {
            List<UIApplyBuffContent.IBuffInfo> infos = new List<UIApplyBuffContent.IBuffInfo>();

            // 축복 아이템 버프
            foreach (BlessBuffItemInfo buff in Entity.player.battleBuffItemInfo.blessBuffItemList)
            {
                infos.Add(buff);
            }

            // 이벤트 버프
            foreach (EventBuffInfo buff in Entity.player.battleBuffItemInfo.eventBuffList)
            {
                infos.Add(buff);
            }

            // 아이템 버프
            foreach (BuffItemInfo buff in Entity.player.battleBuffItemInfo.buffItemList)
            {
                infos.Add(buff);
            }

            return infos.ToArray();
        }

        /// <summary>
        /// 신규 컨텐츠 플래그 제거
        /// </summary>
        public void RemoveNewOpenContent_Buff()
        {
            questModel.RemoveNewOpenContent(ContentType.Buff); // // 신규 컨텐츠 플래그 제거 (버프)
        }

        public ConsumableInfoSlot.Info[] GetConsumableSlotInfos()
        {
            BetterList<ItemInfo> list = new BetterList<ItemInfo>();
            int jobChangeTicketId = BasisItem.JobChangeTicket.GetID();
            int transcendenceDisasembleId = BasisItem.TranscendenceDisasemble.GetID();
            foreach (var item in inventoryModel.itemList)
            {
                if (item.GetType() != typeof(ConsumableItemInfo))
                    continue;

                // 특정 물약 제외, 확성기, 성전환물약
                if (item.ConsumableItemType != ConsumableItemType.None)
                    continue;

                // 특정 아이템 제외 (직업변경권, 휴대용용광로)
                if (item.ItemId == jobChangeTicketId || item.ItemId == transcendenceDisasembleId)
                    continue;

                list.Add(item);
            }

            list.Sort(SortByCustom);

            var infos = new ConsumableInfoSlot.Info[list.size];
            for (int i = 0; i < list.size; i++)
            {
                infos[i] = new ConsumableInfoSlot.Info(list[i], this);
            }
            return infos;
        }

        int SortByCustom(ItemInfo x, ItemInfo y)
        {
            return y.ItemId.CompareTo(x.ItemId);
        }

        void ConsumableInfoSlot.Impl.OnSelect(ItemInfo item)
        {
            UI.Show<UIConsumableInfo>(item);
        }
    }
}