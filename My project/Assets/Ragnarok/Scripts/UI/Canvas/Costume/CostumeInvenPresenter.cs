using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    public class CostumeInvenPresenter : ViewPresenter, UICostumeInfoSlot.Impl
    {
        /******************** Models ********************/
        private readonly InventoryModel invenModel;

        /******************** Repositories ********************/

        /******************** Event ********************/      
        public event InventoryModel.CostumeEvent OnUpdateCostume
        {
            add { invenModel.OnUpdateCostume += value; }
            remove { invenModel.OnUpdateCostume -= value; }
        }        

        public CostumeInvenPresenter()
        {
            invenModel = Entity.player.Inventory;
        }

        public override void AddEvent()
        {            
        }

        public override void RemoveEvent()
        {            
        }

        public ItemInfo[] GetCostumeArray(ItemEquipmentSlotType slotType)
        {
            List<ItemInfo> result = invenModel.itemList.FindAll(a => a is CostumeItemInfo && a.SlotType == slotType);
            result.Sort(SortByCustom);
            return result.ToArray();
        }

        void UICostumeInfoSlot.Impl.OnSelect(ItemInfo item)
        {
            if (item == null)
                return;

            UI.Show<UICostumeInfo>().Set(item.ItemNo);
        }

        private int SortByCustom(ItemInfo x, ItemInfo y)
        {
            int result0 = y.IsEquipped.CompareTo(x.IsEquipped);
            int result1 = result0 == 0 ? y.ItemId.CompareTo(x.ItemId) : result0;
            return result1;
        }

        TweenAlpha UICostumeInfoSlot.Impl.TweenAlpha => default;
        bool UICostumeInfoSlot.Impl.IsDisassemble(long itemNo) => default;       
    }
}