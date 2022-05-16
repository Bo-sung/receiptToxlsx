namespace Ragnarok
{
    public class ChangeElementViewPresenter : ViewPresenter
    {
        private readonly InventoryModel inventoryModel;
        private readonly QuestModel questModel;

        /// <summary>
        /// 장비 속성 변경 성공 이벤트
        /// </summary>
        public event System.Action<bool> OnChangeElement
        {
            add { inventoryModel.OnChangeElement += value; }
            remove { inventoryModel.OnChangeElement -= value; }
        }

        public ChangeElementViewPresenter()
        {
            inventoryModel = Entity.player.Inventory;
            questModel = Entity.player.Quest;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void RequestGiveElemental(ItemInfo targetItem, ItemInfo material)
        {
            inventoryModel.RequestItemElementChange(targetItem, material).WrapNetworkErrors();
        }

        public bool IsContentsOpen()
        {
            return questModel.IsOpenContent(ContentType.ChangeElement, false);
        }
    }
}
