using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class MakeSelectPartPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
        }

        private readonly IView view;
        private readonly MakeModel makeModel;
        private readonly InventoryModel inventoryModel;

        public MakeSelectPartPresenter(IView view)
        {
            this.view = view;
            makeModel = Entity.player.Make;
            inventoryModel = Entity.player.Inventory;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public bool IsInfoMode = false;

        public MaterialInfo Info => makeModel.SelectMaterialInfo;

        public ItemInfo[] GetItemInfos()
        {
            return inventoryModel.itemList.FindAll(a => a.ItemId == Info.ItemId).ToArray();
        }

        public int SelcectItemCount => makeModel.selectItemDict.Count;

        public void SelectItemInfo(ItemInfo info)
        {
            makeModel.SelectItemInfo(info);
            view.Refresh();
        }

        public bool IsSelect(ItemInfo info)
        {
           return makeModel.IsSelect(info);
        }

       public void UIMakeRefresh()
        {
            makeModel.SelectMakeInfo.Refresh();
        }

        public int GetSelectItemCount(int slotIndex)
        {
            return makeModel.GetSelectItemCount(slotIndex);
        }

        public void ToggelInfoMode()
        {
            IsInfoMode = !IsInfoMode;
            view.Refresh();
        }

        public void ResetInfoMode()
        {
            IsInfoMode = false;
        }

        public async Task UnEquip(ItemInfo info)
        {
            if(info.IsEquipped)
                await inventoryModel.RequestItemEquip(info);

            if(info.IsLock)
                await inventoryModel.RequestItemLock(info);

            return;
        }
    }
}
