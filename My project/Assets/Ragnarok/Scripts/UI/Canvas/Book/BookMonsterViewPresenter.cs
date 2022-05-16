using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ragnarok
{
    public class BookMonsterViewPresenter : ViewPresenter
    {
        private readonly MonsterDataManager monsterRepo;
        private readonly BookDataManager bookRepo;
        private readonly InventoryModel inventoryModel;
        private readonly BookModel bookModel;
        private readonly UIBookMonsterView view;

        private List<BookStateDecoratedData> lastShowingList = new List<BookStateDecoratedData>();

        public BookMonsterViewPresenter(UIBookMonsterView view)
        {
            monsterRepo = MonsterDataManager.Instance;
            bookRepo = BookDataManager.Instance;
            inventoryModel = Entity.player.Inventory;
            bookModel = Entity.player.Book;
            this.view = view;
        }

        public void OnShow()
        {
            UpdateLevelView();
            if (lastShowingList.Count == 0)
            {
                foreach (var each in monsterRepo.EntireMonsters)
                {
                    if (each.dic_id == 0 || each.dic_order == 0)
                        continue;

                    lastShowingList.Add(new BookStateDecoratedData()
                    {
                        data = each,
                        isRecorded = bookModel.IsRecorded(BookTabType.Monster, each.dic_id)
                    });
                }

                view.ShowList(lastShowingList);
            }
            else
            {
                foreach (var each in lastShowingList)
                    each.isRecorded = bookModel.IsRecorded(BookTabType.Monster, each.GetData<MonsterData>().dic_id);
                view.RefreshList();
            }
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

        public void ViewEventHandler(UIBookMonsterView.Event eventType, object data)
        {
            if (eventType == UIBookMonsterView.Event.OnClickLevelUp)
            {
                RequestLevelUp();
            }
            else if (eventType == UIBookMonsterView.Event.OnClickSlot)
            {
                BookStateDecoratedData item = data as BookStateDecoratedData;
                UI.Show<UIMonsterDetail>().ShowMonster(item.GetData<MonsterData>().id, item.GetData<MonsterData>().dic_lv);
            }
            else if (eventType == UIBookMonsterView.Event.OnClickRewardDetail)
            {
                UI.Show<UIBookLevelPopup>().ShowBookLevel(BookTabType.Monster);
            }
        }

        private async void RequestLevelUp()
        {
            bool result = await bookModel.RequestLevelUp(BookTabType.Monster);

            if (result)
            {
                int curLevel = bookModel.GetTabLevel(BookTabType.Monster);
                var rewardData = BookDataManager.Instance.GetBookRewardData(BookTabType.Monster, curLevel);
                UI.Show<UIBookReward>(new UIBookReward.Input() { rewardData = rewardData });
            }
        }

        private void OnBookStateChange(BookTabType tabType)
        {
            if (tabType != BookTabType.Monster)
                return;

            OnBookStateRefreshed();
        }

        private void OnBookStateRefreshed()
        {
            UpdateLevelView();
            foreach (var each in lastShowingList)
                each.isRecorded = bookModel.IsRecorded(BookTabType.Monster, each.GetData<MonsterData>().dic_id);
            view.RefreshList();
        }

        private void UpdateLevelView()
        {
            int curLevel = bookModel.GetTabLevel(BookTabType.Monster);
            int curRecordCount = bookModel.GetTabRecordCount(BookTabType.Monster);
            var curRewardData = BookDataManager.Instance.GetBookRewardData(BookTabType.Monster, curLevel);
            var nextRewardData = BookDataManager.Instance.GetBookRewardData(BookTabType.Monster, curLevel + 1);

            view.SetCurrentLevelState(curRewardData, nextRewardData, curRecordCount);
        }
    }
}
