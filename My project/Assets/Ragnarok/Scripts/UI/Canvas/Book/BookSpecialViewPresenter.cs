using System.Collections.Generic;

namespace Ragnarok
{

    public class BookSpecialViewPresenter : ViewPresenter
    {
        private readonly ItemDataManager itemRepo;
        private readonly BookModel bookModel;
        private readonly UIBookSpecialView view;

        private List<BookStateDecoratedData> lastShowingList = new List<BookStateDecoratedData>();

        public BookSpecialViewPresenter(UIBookSpecialView view)
        {
            itemRepo = ItemDataManager.Instance;
            bookModel = Entity.player.Book;
            this.view = view;
        }

        public void OnShow()
        {
            view.ResetView();
            UpdateLevelView();
        }

        public override void AddEvent()
        {
            bookModel.OnBookStateChange += OnBookStateChange;
            bookModel.OnBookStateRefreshed += OnBookStateRefreshed;
        }

        public override void RemoveEvent()
        {
            bookModel.OnBookStateChange -= OnBookStateChange;
            bookModel.OnBookStateRefreshed -= OnBookStateRefreshed;
        }

        public void ViewEventHandler(UIBookSpecialView.Event eventType, object data)
        {
            if (eventType == UIBookSpecialView.Event.OnSpecialTypeChanged)
            {
                CostumeType filter = (CostumeType)data;

                lastShowingList.Clear();

                foreach (var each in itemRepo.EntireItems)
                {
                    if (each.dic_id == 0 || each.dic_order == 0 || each.BookType != BookType.Special || !filter.HasFlag(each.class_type.ToEnum<CostumeType>()))
                        continue;

                    var info = new CostumeItemInfo();
                    info.SetData(each);

                    lastShowingList.Add(new BookStateDecoratedData()
                    {
                        data = info,
                        isRecorded = bookModel.IsRecorded(BookTabType.Special, each.dic_id)
                    });
                }

                view.ShowList(lastShowingList);
            }
            else if (eventType == UIBookSpecialView.Event.OnClickLevelUp)
            {
                RequestLevelUp();
            }
            else if (eventType == UIBookSpecialView.Event.OnClickSlot)
            {
                BookStateDecoratedData item = data as BookStateDecoratedData;
                UI.ShowItemInfo(item.GetData<ItemInfo>());
            }
            else if (eventType == UIBookSpecialView.Event.OnClickRewardDetail)
            {
                UI.Show<UIBookLevelPopup>().ShowBookLevel(BookTabType.Special);
            }
        }

        private async void RequestLevelUp()
        {
            bool result = await bookModel.RequestLevelUp(BookTabType.Special);

            if (result)
            {
                int curLevel = bookModel.GetTabLevel(BookTabType.Special);
                var rewardData = BookDataManager.Instance.GetBookRewardData(BookTabType.Special, curLevel);
                UI.Show<UIBookReward>(new UIBookReward.Input() { rewardData = rewardData });
            }
        }

        private void OnBookStateChange(BookTabType tabType)
        {
            if (tabType != BookTabType.Special)
                return;

            OnBookStateRefreshed();
        }

        private void OnBookStateRefreshed()
        {
            UpdateLevelView();
            foreach (var each in lastShowingList)
                each.isRecorded = bookModel.IsRecorded(BookTabType.Special, each.GetData<ItemInfo>().BookIndex);
            view.RefreshList();
        }

        private void UpdateLevelView()
        {
            int curLevel = bookModel.GetTabLevel(BookTabType.Special);
            int curRecordCount = bookModel.GetTabRecordCount(BookTabType.Special);
            var curRewardData = BookDataManager.Instance.GetBookRewardData(BookTabType.Special, curLevel);
            var nextRewardData = BookDataManager.Instance.GetBookRewardData(BookTabType.Special, curLevel + 1);

            view.SetCurrentLevelState(curRewardData, nextRewardData, curRecordCount);
        }
    }
}
