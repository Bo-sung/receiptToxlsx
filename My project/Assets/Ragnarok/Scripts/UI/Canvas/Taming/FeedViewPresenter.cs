namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIFeedView"/>
    /// </summary>
    public class FeedViewPresenter : ViewPresenter
    {
        public interface IView
        {
            void SetRewardData(RewardData rewardData);
            void SetItemCount(int itemCount);
        }

        /******************** Models ********************/
        InventoryModel invenModel;

        /******************** Repositories ********************/

        /******************** Event ********************/

        private int itemId;

        private readonly IView view;

        public FeedViewPresenter(IView view)
        {
            this.view = view;

            invenModel = Entity.player.Inventory;

            itemId = 0;
        }

        public override void AddEvent()
        {
            invenModel.OnUpdateItem += OnUpdateItem;
        }

        public override void RemoveEvent()
        {
            invenModel.OnUpdateItem -= OnUpdateItem;
        }

        public void SetItem(int itemId)
        {
            this.itemId = itemId;
            OnUpdateItem();
        }

        private void OnUpdateItem()
        {
            if (itemId == 0)
                return;

            var rewardData = new RewardData(RewardType.Item, itemId, 1);
            view.SetRewardData(rewardData);

            int itemCount = invenModel.GetItemCount(itemId);
            view.SetItemCount(itemCount);
        }
    }
}