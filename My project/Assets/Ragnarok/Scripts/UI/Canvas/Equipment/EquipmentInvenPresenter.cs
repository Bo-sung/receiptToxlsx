using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIEquipmentInven"/>
    /// </summary>
    public sealed class EquipmentInvenPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
        }

        private readonly IView view;
        private readonly InventoryModel invenModel;

        public EquipmentInvenPresenter(IView view)
        {
            this.view = view;
            invenModel = Entity.player.Inventory;
        }

        public override void AddEvent()
        {
            invenModel.OnUpdateItem += view.Refresh;
        }

        public override void RemoveEvent()
        {
            invenModel.OnUpdateItem -= view.Refresh;
        }

        public ItemInfo[] GetEquipmentArray(ItemEquipmentSlotType slotType)
        {          
            List<ItemInfo> result = invenModel.itemList.FindAll(a => a is EquipmentItemInfo && a.SlotType == slotType);
            result.Sort(SortByCustom);
            return result.ToArray();
        }

        public EquipmentItemInfo GetStrongestEquipment(ItemEquipmentSlotType slotType)
        {
            return invenModel.GetStrongestEquipmentInSlotType(slotType);
        }


        private int SortByCustom(ItemInfo x, ItemInfo y)
        {
            int result0 = y.IsEquipped.CompareTo(x.IsEquipped);
            int result1 = result0 == 0 ? y.Rating.CompareTo(x.Rating) : result0;
            int result2 = result1 == 0 ? y.Tier.CompareTo(x.Tier) : result1;
            int result3 = result2 == 0 ? y.Smelt.CompareTo(x.Smelt) : result2;
            int result4 = result3 == 0 ? x.ItemId.CompareTo(y.ItemId) : result3;
            return result4;
        }        
    }
}
