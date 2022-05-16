namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIPartsInfo"/>
    /// </summary>
    public sealed class PartsInfoPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
        }

        private readonly IView view;
        private readonly InventoryModel inventoryModel;
        private readonly BoxDataManager boxDataRepo;

        public ItemInfo info { get; private set; }

        public PartsInfoPresenter(IView view)
        {
            this.view = view;

            inventoryModel = Entity.player.Inventory;
            boxDataRepo = BoxDataManager.Instance;
        }

        public override void AddEvent()
        {

        }

        public override void RemoveEvent()
        {
            RemoveInfoEvent();
        }

        public void SelectInfo(ItemInfo info)
        {
            RemoveInfoEvent();

            this.info = info;
            inventoryModel.ShowBoxItemRewardLog(info);

            AddInfoEvent();
            view.Refresh();
        }

        private void AddInfoEvent()
        {
            if (info != null)
                info.OnUpdateEvent += view.Refresh;
        }

        private void RemoveInfoEvent()
        {
            if (info != null)
            {
                info.OnUpdateEvent -= view.Refresh;
                info = null;
            }
        }

        public int GetItemCount()
        {
            return info == null ? 0 : inventoryModel.GetItemCount(info.ItemId);
        }

        public BoxType GetBoxType()
        {
            if (info == null)
                return BoxType.None;

            if (info.ItemType != ItemType.Box)
                return BoxType.None;

            BoxData data = boxDataRepo.Get(info.EventId);

            if (data == null)
                return BoxType.None;

            return data.box_type.ToEnum<BoxType>();
        }
    }
}