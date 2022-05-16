using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ragnarok
{
    public class BookPresenter : ViewPresenter
    {
        private readonly GoodsModel goodsModel;
        private readonly BookModel bookModel;
        private readonly UIBook view;

        public event Action OnUpdateGoodsZeny;
        public event Action OnUpdateGoodsCatCoin;

        public BookPresenter(UIBook view)
        {
            goodsModel = Entity.player.Goods;
            bookModel = Entity.player.Book;
            this.view = view;
        }

        public void OnShow()
        {
            UpdateNotice(BookTabType.Equipment);
            UpdateNotice(BookTabType.Card);
            UpdateNotice(BookTabType.Monster);
            UpdateNotice(BookTabType.Costume);
            UpdateNotice(BookTabType.Special);
        }

        public override void AddEvent()
        {
            bookModel.OnBookStateChange += OnBookStateChange;
            goodsModel.OnUpdateZeny += InvokeUpdateGoodsZeny;
            goodsModel.OnUpdateCatCoin += InvokeUpdateGoodsCatCoin;
        }

        public override void RemoveEvent()
        {
            bookModel.OnBookStateChange -= OnBookStateChange;
            goodsModel.OnUpdateZeny -= InvokeUpdateGoodsZeny;
            goodsModel.OnUpdateCatCoin -= InvokeUpdateGoodsCatCoin;
        }

        void InvokeUpdateGoodsZeny(long value)
        {
            OnUpdateGoodsZeny?.Invoke();
        }

        void InvokeUpdateGoodsCatCoin(long value)
        {
            OnUpdateGoodsCatCoin?.Invoke();
        }

        private void OnBookStateChange(BookTabType obj)
        {
            UpdateNotice(obj);
        }

        private void UpdateNotice(BookTabType tabType)
        {
            BookData data = BookDataManager.Instance.GetBookRewardData(tabType, bookModel.GetTabLevel(tabType) + 1);
            view.SetNotice(tabType, data != null && bookModel.GetTabRecordCount(tabType) >= data.score);
        }

        public long GetZeny()
        {
            return goodsModel.Zeny;
        }

        public long GetCatCoin()
        {
            return goodsModel.CatCoin;
        }
    }
}
