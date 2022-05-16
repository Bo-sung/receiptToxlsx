using System.Collections.Generic;

namespace Ragnarok
{
    public class BookOnBuffViewPresenter : ViewPresenter
    {
        private readonly ItemDataManager itemRepo;
        private readonly BookModel bookModel;
        private readonly UIBookOnBuffView view;

        private List<BookStateDecoratedData> lastShowingList = new List<BookStateDecoratedData>();

        public BookOnBuffViewPresenter(UIBookOnBuffView view)
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

        public void ViewEventHandler(UIBookOnBuffView.Event eventType, object data)
        {
            if (eventType == UIBookOnBuffView.Event.OnSpecialTypeChanged)
            {
                CostumeType filter = (CostumeType)data;

                lastShowingList.Clear();

                foreach (var each in itemRepo.EntireItems)
                {
                    if (each.dic_id == 0 || each.dic_order == 0 || each.BookType != BookType.OnBuff || !filter.HasFlag(each.class_type.ToEnum<CostumeType>()))
                        continue;

                    var info = new CostumeItemInfo();
                    info.SetData(each);

                    lastShowingList.Add(new BookStateDecoratedData()
                    {
                        data = info,
                        isRecorded = bookModel.IsRecorded(BookTabType.OnBuff, each.dic_id)
                    });
                }

                view.ShowList(lastShowingList);
            }
            else if (eventType == UIBookOnBuffView.Event.OnClickLevelUp)
            {
                RequestLevelUp();
            }
            else if (eventType == UIBookOnBuffView.Event.OnClickSlot)
            {
                BookStateDecoratedData item = data as BookStateDecoratedData;
                UI.ShowItemInfo(item.GetData<ItemInfo>());
            }
            else if (eventType == UIBookOnBuffView.Event.OnClickRewardDetail)
            {
                UI.Show<UIBookLevelPopup>().ShowBookLevel(BookTabType.OnBuff);
            }
        }

        private async void RequestLevelUp()
        {
            bool result = await bookModel.RequestLevelUp(BookTabType.OnBuff);

            if (result)
            {
                int curLevel = bookModel.GetTabLevel(BookTabType.OnBuff);
                var rewardData = BookDataManager.Instance.GetBookRewardData(BookTabType.OnBuff, curLevel);
                UI.Show<UIBookReward>(new UIBookReward.Input() { rewardData = rewardData });
            }
        }

        private void OnBookStateChange(BookTabType tabType)
        {
            if (tabType != BookTabType.OnBuff)
                return;

            OnBookStateRefreshed();
        }

        private void OnBookStateRefreshed()
        {
            UpdateLevelView();
            foreach (var each in lastShowingList)
                each.isRecorded = bookModel.IsRecorded(BookTabType.OnBuff, each.GetData<ItemInfo>().BookIndex);
            view.RefreshList();
        }

        private void UpdateLevelView()
        {
            int curLevel = bookModel.GetTabLevel(BookTabType.OnBuff);
            int curRecordCount = bookModel.GetTabRecordCount(BookTabType.OnBuff);
            var curRewardData = BookDataManager.Instance.GetBookRewardData(BookTabType.OnBuff, curLevel);
            var nextRewardData = BookDataManager.Instance.GetBookRewardData(BookTabType.OnBuff, curLevel + 1);

            view.SetCurrentLevelState(curRewardData, nextRewardData, curRecordCount);
        }
    }
}
