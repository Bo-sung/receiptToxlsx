using System.Collections.Generic;

namespace Ragnarok
{
    public class BookLevelPopupPresenter : ViewPresenter
    {
        private UIBookLevelPopup view;

        public BookLevelPopupPresenter(UIBookLevelPopup view)
        {
            this.view = view;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void OnShow(BookTabType tabType)
        {
            IEnumerable<BookData> datas = BookDataManager.Instance.GetBookRewardDatas(tabType);

            if (datas == null)
                return;

            List<BookData> rewardDatas = new List<BookData>();
            rewardDatas.AddRange(datas);

            view.ShowList(rewardDatas, Entity.player.Book.GetTabLevel(tabType), Entity.player.Book.GetTabRecordCount(tabType));
        }
    }
}
