using Ragnarok.View;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class BoxRewardListPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly InventoryModel inventoryModel;

        // <!-- Repositories --!>
        private readonly ItemDataManager itemDataRepo;

        // <!-- Event --!>

        public BoxRewardListPresenter()
        {
            inventoryModel = Entity.player.Inventory;
            itemDataRepo = ItemDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public BoxRewardItemView.IInput GetItemViewInfo(int itemId)
        {
            return itemDataRepo.Get(itemId);
        }

        public List<BoxRewardGroup> GetBoxRewardGroup(int itemId)
        {
            return inventoryModel.GetBoxRewardList(itemId);
        }
    }
}