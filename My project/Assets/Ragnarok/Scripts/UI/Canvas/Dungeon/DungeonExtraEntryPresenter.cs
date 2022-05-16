namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIDungeonExtraEntry"/>
    /// </summary>
    public class DungeonExtraEntryPresenter : ViewPresenter
    {
        private readonly GoodsModel goodsModel;

        public event System.Action<long> OnUpdateCatCoin
        {
            add { goodsModel.OnUpdateCatCoin += value; }
            remove { goodsModel.OnUpdateCatCoin -= value; }
        }

        public DungeonExtraEntryPresenter()
        {
            goodsModel = Entity.player.Goods;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public long GetCatCoin()
        {
            return goodsModel.CatCoin;
        }
    }
}