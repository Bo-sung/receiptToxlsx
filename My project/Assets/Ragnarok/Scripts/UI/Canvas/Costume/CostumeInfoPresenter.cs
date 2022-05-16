namespace Ragnarok
{
    public class CostumeInfoPresenter : ViewPresenter
    {
        /******************** Models ********************/
        private readonly InventoryModel inventoryModel;
        private readonly CharacterModel characterModel;

        /******************** Repositories ********************/

        /******************** Event ********************/
        public event InventoryModel.ItemUpdateEvent OnUpdateItem
        {
            add { inventoryModel.OnUpdateItem += value; }
            remove { inventoryModel.OnUpdateItem -= value; }
        }

        public event InventoryModel.CostumeEvent OnUpdateCostume
        {
            add { inventoryModel.OnUpdateCostume += value; }
            remove { inventoryModel.OnUpdateCostume -= value; }
        }

        public bool IsOwningCostume { get; private set; }

        private long costumeNo;
        private ItemInfo itemInfo;

        public CostumeInfoPresenter()
        {
            inventoryModel = Entity.player.Inventory;
            characterModel = Entity.player.Character;
        }

        public override void AddEvent()
        {            
        }

        public override void RemoveEvent()
        {            
        }

        public void SetCostumeNo(long costumeNo)
        {
            itemInfo = null;
            this.costumeNo = costumeNo;
            IsOwningCostume = true;
        }

        public void SetCostumeInfo(ItemInfo itemInfo)
        {
            costumeNo = 0;
            this.itemInfo = itemInfo;
            IsOwningCostume = false;
        }

        public ItemInfo GetCostume()
        {
            if (itemInfo != null)
                return itemInfo;
            else
                return inventoryModel.GetItemInfo(costumeNo);
        }

        /// <summary>
        /// 아이템 장착,교체,해제
        /// </summary>
        public void OnClickedBtnOk()
        {
            if (IsOwningCostume)
                inventoryModel.RequestCostumeEquip(GetCostume()).WrapNetworkErrors();
            else
                UI.Close<UICostumeInfo>();
        }

        public Gender GetGender()
        {
            return characterModel.Gender;
        }
    }
}