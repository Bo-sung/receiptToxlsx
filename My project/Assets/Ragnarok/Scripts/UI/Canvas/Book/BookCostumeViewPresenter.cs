using System.Collections.Generic;

namespace Ragnarok
{
    public class BookCostumeViewPresenter : ViewPresenter
    {
        private readonly ItemDataManager itemRepo;
        private readonly BookModel bookModel;
        private readonly UIBookCostumeView view;

        private List<BookStateDecoratedData> lastShowingList = new List<BookStateDecoratedData>();

        public BookCostumeViewPresenter(UIBookCostumeView view)
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

        public void ViewEventHandler(UIBookCostumeView.Event eventType, object data)
        {
            if (eventType == UIBookCostumeView.Event.OnCostumeTypeChanged)
            {
                CostumeType filter = (CostumeType)data;

                lastShowingList.Clear();

                foreach (var each in itemRepo.EntireItems)
                {
                    if (each.dic_id == 0 || each.dic_order == 0 || each.BookType != BookType.Costume || !filter.HasFlag(each.class_type.ToEnum<CostumeType>()))
                        continue;

                    var info = new CostumeItemInfo();
                    info.SetData(each);

                    lastShowingList.Add(new BookStateDecoratedData()
                    {
                        data = info,
                        isRecorded = bookModel.IsRecorded(BookTabType.Costume, each.dic_id)
                    });
                }

                view.ShowList(lastShowingList);
            }
            else if (eventType == UIBookCostumeView.Event.OnClickLevelUp)
            {
                RequestLevelUp();
            }
            else if (eventType == UIBookCostumeView.Event.OnClickSlot)
            {
                BookStateDecoratedData item = data as BookStateDecoratedData;
                UI.ShowItemInfo(item.GetData<ItemInfo>());
            }
            else if (eventType == UIBookCostumeView.Event.OnClickRewardDetail)
            {
                UI.Show<UIBookLevelPopup>().ShowBookLevel(BookTabType.Costume);
            }
        }

        private async void RequestLevelUp()
        {
            bool result = await bookModel.RequestLevelUp(BookTabType.Costume);

            if (result)
            {
                int curLevel = bookModel.GetTabLevel(BookTabType.Costume);
                var rewardData = BookDataManager.Instance.GetBookRewardData(BookTabType.Costume, curLevel);
                UI.Show<UIBookReward>(new UIBookReward.Input() { rewardData = rewardData });
            }
        }

        private void OnBookStateChange(BookTabType tabType)
        {
            if (tabType != BookTabType.Costume)
                return;

            OnBookStateRefreshed();
        }

        private void OnBookStateRefreshed()
        {
            UpdateLevelView();
            foreach (var each in lastShowingList)
                each.isRecorded = bookModel.IsRecorded(BookTabType.Costume, each.GetData<ItemInfo>().BookIndex);
            view.RefreshList();
        }

        private void UpdateLevelView()
        {
            int curLevel = bookModel.GetTabLevel(BookTabType.Costume);
            int curRecordCount = bookModel.GetTabRecordCount(BookTabType.Costume);
            var curRewardData = BookDataManager.Instance.GetBookRewardData(BookTabType.Costume, curLevel);
            var nextRewardData = BookDataManager.Instance.GetBookRewardData(BookTabType.Costume, curLevel + 1);

            view.SetCurrentLevelState(curRewardData, nextRewardData, curRecordCount);
        }
    }
}
