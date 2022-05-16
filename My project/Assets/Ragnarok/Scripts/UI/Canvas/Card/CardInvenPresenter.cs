namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICardInven"/>
    /// </summary>
    public sealed class CardInvenPresenter : ViewPresenter
    {
        public interface IView
        {
            void UpdateView();           
        }

        private readonly IView view;
        private readonly InventoryModel invenModel;

        public CardInvenPresenter(IView view)
        {
            this.view = view;
            invenModel = Entity.player.Inventory;
        }

        public override void AddEvent()
        {
            invenModel.OnUpdateItem += view.UpdateView;
        }

        public override void RemoveEvent()
        {
            invenModel.OnUpdateItem -= view.UpdateView;
        }

        public ItemInfo[] GetCardItemInfos(EquipmentClassType ClassType, bool isShadow)
        {
            //var result = from pair in invenModel.GetCardItemInfos()
            //             where !pair.IsEquipped && pair.ClassType.HasFlag(ClassType)
            //             orderby pair.Level descending, pair.ItemID ascending
            //             select pair;

            ItemInfo[] result = invenModel.itemList.FindAll(a => a is CardItemInfo && !a.IsEquipped && a.ClassType.HasFlag(ClassType) && a.IsShadow == isShadow).ToArray();
            System.Array.Sort(result, SortByCustom);
            return result;
        }

        private int SortByCustom(ItemInfo x, ItemInfo y)
        {
            int result1 = x.Smelt.CompareTo(y.Smelt);
            int result2 = result1 == 0 ? y.ItemId.CompareTo(x.ItemId) : result1;
            return result2;
        }
    }
}