using UnityEngine;
using System.Collections.Generic;

namespace Ragnarok
{

    public class UIBookSpecialView : MonoBehaviour, IUIBookView
    {
        public enum Event { OnSpecialTypeChanged, OnClickSlot, OnClickLevelUp, OnClickRewardDetail }

        [SerializeField] UIBookListPanel listPanel;
        [SerializeField] UIBookLevelInfoPanel levelInfoPanel;

        private BookSpecialViewPresenter presenter;

        public void OnInit()
        {
            presenter = new BookSpecialViewPresenter(this);
        }

        public void OnClose()
        {
            presenter = null;
        }

        public void OnShow()
        {
            gameObject.SetActive(true);
            listPanel.gameObject.SetActive(true);

            levelInfoPanel.LocalizeText(LocalizeKey._40234.ToText(), LocalizeKey._40237.ToText()); // 스페셜 도감 등급, 획득한 스페셜 코스튬
            listPanel.OnClickSideTab += OnClickSideTab;
            listPanel.OnClickSlot += OnClickSlot;
            levelInfoPanel.OnClickLevelUp += OnClickLevelUpButton;
            levelInfoPanel.OnClickRewardsDetail += OnClickRewardsDetail;

            presenter.AddEvent();
            presenter.OnShow();
        }

        public void OnHide()
        {
            gameObject.SetActive(false);

            listPanel.OnClickSideTab -= OnClickSideTab;
            listPanel.OnClickSlot -= OnClickSlot;
            levelInfoPanel.OnClickLevelUp -= OnClickLevelUpButton;
            levelInfoPanel.OnClickRewardsDetail -= OnClickRewardsDetail;

            presenter.RemoveEvent();
        }

        public void ResetView()
        {
            if (!BasisOpenContetsType.Pet.IsOpend())
            {
                listPanel.SetSideTabInfo(
                       new UIBookListPanel.TabInfo(LocalizeKey._40239.ToText(), CostumeType.Title)); // 칭호
                listPanel.SetSideTab(0);
                OnClickSideTab(CostumeType.Title);
            }
            else
            {
                listPanel.SetSideTabInfo(
                       new UIBookListPanel.TabInfo(LocalizeKey._40235.ToText(), CostumeType.Pet), // 펫
                       new UIBookListPanel.TabInfo(LocalizeKey._40239.ToText(), CostumeType.Title)); // 칭호
                listPanel.SetSideTab(0);
                OnClickSideTab(CostumeType.Pet);
            }
        }

        public void SetCurrentLevelState(BookData lastData, BookData nextData, int curCount)
        {
            levelInfoPanel.SetLevelInfo(lastData, nextData, curCount);
        }

        public void ShowList(List<BookStateDecoratedData> list)
        {
            list.Sort((a, b) =>
            {
                var aData = a.GetData<ItemInfo>();
                var bData = b.GetData<ItemInfo>();

                return aData.BookOrder - bData.BookOrder;
            });

            listPanel.ShowList(list);
        }

        public void RefreshList()
        {
            listPanel.RefreshList();
        }

        private void OnClickSideTab(object data)
        {
            presenter.ViewEventHandler(Event.OnSpecialTypeChanged, data);
        }

        private void OnClickSlot(BookStateDecoratedData obj)
        {
            presenter.ViewEventHandler(Event.OnClickSlot, obj);
        }

        private void OnClickLevelUpButton()
        {
            presenter.ViewEventHandler(Event.OnClickLevelUp, null);
        }

        private void OnClickRewardsDetail()
        {
            presenter.ViewEventHandler(Event.OnClickRewardDetail, null);
        }
    }
}