using System.Collections.Generic;

namespace Ragnarok
{
    public class BookCardViewPresenter : ViewPresenter
    {
        private readonly ItemDataManager itemRepo;
        private readonly BookModel bookModel;
        private readonly UIBookCardView view;

        private List<BookStateDecoratedData> lastShowingList = new List<BookStateDecoratedData>();

        public BookCardViewPresenter(UIBookCardView view)
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

        public void ViewEventHandler(UIBookCardView.Event eventType, object data)
        {
            if (eventType == UIBookCardView.Event.OnClassTypeChanged)
            {
                (EquipmentClassType equipmentClassType, bool isShadow)? filter = null;
                if (data != null)
                {
                    filter = ((EquipmentClassType, bool))data;
                }

                lastShowingList.Clear();

                foreach (var each in itemRepo.EntireItems)
                {
                    bool isShadow = each.BookType == BookType.Card && each.duration == 1;
                    bool isExclude = filter.HasValue;
                    if (isExclude)
                    {
                        if (filter.Value.isShadow)
                        {
                            // 쉐도우 카드 포함 조건 체크
                            if (isShadow)
                            {
                                isExclude = false;
                            }
                            else
                            {
                                isExclude = true;
                            }
                        }
                        else
                        {
                            var classType = each.class_type.ToEnum<EquipmentClassType>();

                            switch (classType)
                            {
                                case EquipmentClassType.All:
                                    if (filter.Value.equipmentClassType == EquipmentClassType.All)
                                        isExclude = false;
                                    break;
                                default:
                                    if (classType.HasFlag(filter.Value.equipmentClassType))
                                        isExclude = false;
                                    break;
                            }
                        }
                    }

                    if (each.dic_id == 0 || each.dic_order == 0 || each.BookType != BookType.Card
                        || isExclude)
                        continue;

                    var info = new CardItemInfo();
                    info.SetData(each);

                    lastShowingList.Add(new BookStateDecoratedData()
                    {
                        data = info,
                        isRecorded = bookModel.IsRecorded(BookTabType.Card, each.dic_id)
                    });
                }

                view.ShowList(lastShowingList);
            }
            else if (eventType == UIBookCardView.Event.OnClickLevelUp)
            {
                RequestLevelUp();
            }
            else if (eventType == UIBookCardView.Event.OnClickSlot)
            {
                BookStateDecoratedData item = data as BookStateDecoratedData;
                UI.ShowItemInfo(item.GetData<ItemInfo>());
            }
            else if (eventType == UIBookCardView.Event.OnClickRewardDetail)
            {
                UI.Show<UIBookLevelPopup>().ShowBookLevel(BookTabType.Card);
            }
        }

        private async void RequestLevelUp()
        {
            bool result = await bookModel.RequestLevelUp(BookTabType.Card);

            if (result)
            {
                int curLevel = bookModel.GetTabLevel(BookTabType.Card);
                var rewardData = BookDataManager.Instance.GetBookRewardData(BookTabType.Card, curLevel);
                UI.Show<UIBookReward>(new UIBookReward.Input() { rewardData = rewardData });
            }
        }

        private void OnBookStateChange(BookTabType tabType)
        {
            if (tabType != BookTabType.Card)
                return;

            OnBookStateRefreshed();
        }

        private void OnBookStateRefreshed()
        {
            UpdateLevelView();
            foreach (var each in lastShowingList)
                each.isRecorded = bookModel.IsRecorded(BookTabType.Card, each.GetData<ItemInfo>().BookIndex);
            view.RefreshList();
        }

        private void UpdateLevelView()
        {
            int curLevel = bookModel.GetTabLevel(BookTabType.Card);
            int curRecordCount = bookModel.GetTabRecordCount(BookTabType.Card);
            var curRewardData = BookDataManager.Instance.GetBookRewardData(BookTabType.Card, curLevel);
            var nextRewardData = BookDataManager.Instance.GetBookRewardData(BookTabType.Card, curLevel + 1);

            view.SetCurrentLevelState(curRewardData, nextRewardData, curRecordCount);
        }
    }
}
