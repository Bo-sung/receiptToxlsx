using System.Collections.Generic;

namespace Ragnarok
{
    public class BookEquipmentViewPresenter : ViewPresenter
    {
        private readonly ItemDataManager itemRepo;
        private readonly InventoryModel inventoryModel;
        private readonly BookModel bookModel;
        private readonly UIBookEquipmentView view;

        private List<BookStateDecoratedData> lastShowingList = new List<BookStateDecoratedData>();

        public BookEquipmentViewPresenter(UIBookEquipmentView view)
        {
            itemRepo = ItemDataManager.Instance;
            inventoryModel = Entity.player.Inventory;
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

        public void ViewEventHandler(UIBookEquipmentView.Event eventType, object data)
        {
            if (eventType == UIBookEquipmentView.Event.OnClassTypeChanged)
            {
                (EquipmentClassType equipmentClassType, bool isShadow) filter = ((EquipmentClassType, bool))data;

                lastShowingList.Clear();

                foreach (var each in itemRepo.EntireItems)
                {
                    bool isShadow = each.BookType == BookType.Equipment && each.duration == 1;

                    if (each.dic_id == 0 || each.BookType != BookType.Equipment || !filter.equipmentClassType.HasFlag(each.class_type.ToEnum<EquipmentClassType>()) || isShadow != filter.isShadow)
                        continue;

                    var info = new EquipmentItemInfo(inventoryModel);
                    info.SetData(each);

                    lastShowingList.Add(new BookStateDecoratedData()
                    {
                        data = info,
                        isRecorded = bookModel.IsRecorded(BookTabType.Equipment, info.BookIndex)
                    });
                }

                view.ShowList(lastShowingList);
            }
            else if (eventType == UIBookEquipmentView.Event.OnClickLevelUp)
            {
                RequestLevelUp();
            }
            else if (eventType == UIBookEquipmentView.Event.OnClickSlot)
            {
                BookStateDecoratedData item = data as BookStateDecoratedData;
                UI.ShowItemInfo(item.GetData<ItemInfo>());
            }
            else if (eventType == UIBookEquipmentView.Event.OnClickRewardDetail)
            {
                UI.Show<UIBookLevelPopup>().ShowBookLevel(BookTabType.Equipment);
            }
        }

        private async void RequestLevelUp()
        {
            bool result = await bookModel.RequestLevelUp(BookTabType.Equipment);

            if (result)
            {
                int curLevel = bookModel.GetTabLevel(BookTabType.Equipment);
                var rewardData = BookDataManager.Instance.GetBookRewardData(BookTabType.Equipment, curLevel);
                UI.Show<UIBookReward>(new UIBookReward.Input() { rewardData = rewardData });
            }
        }

        private void OnBookStateChange(BookTabType tabType)
        {
            if (tabType != BookTabType.Equipment)
                return;

            OnBookStateRefreshed();
        }

        private void OnBookStateRefreshed()
        {
            UpdateLevelView();
            foreach (var each in lastShowingList)
                each.isRecorded = bookModel.IsRecorded(BookTabType.Equipment, each.GetData<ItemInfo>().BookIndex);
            view.RefreshList();
        }

        private void UpdateLevelView()
        {
            int curLevel = bookModel.GetTabLevel(BookTabType.Equipment);
            int curRecordCount = bookModel.GetTabRecordCount(BookTabType.Equipment);
            var curRewardData = BookDataManager.Instance.GetBookRewardData(BookTabType.Equipment, curLevel);
            var nextRewardData = BookDataManager.Instance.GetBookRewardData(BookTabType.Equipment, curLevel + 1);

            view.SetCurrentLevelState(curRewardData, nextRewardData, curRecordCount);
        }
    }
}
