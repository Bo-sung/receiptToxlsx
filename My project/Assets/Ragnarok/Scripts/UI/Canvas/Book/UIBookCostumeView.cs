using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIBookCostumeView : MonoBehaviour, IUIBookView
    {
        public enum Event { OnCostumeTypeChanged, OnClickSlot, OnClickLevelUp, OnClickRewardDetail }

        [SerializeField] UIBookListPanel listPanel;
        [SerializeField] UIBookLevelInfoPanel levelInfoPanel;

        private BookCostumeViewPresenter presenter;

        public void OnInit()
        {
            presenter = new BookCostumeViewPresenter(this);
        }

        public void OnClose()
        {
            presenter = null;
        }

        public void OnShow()
        {
            gameObject.SetActive(true);
            listPanel.gameObject.SetActive(true);

            levelInfoPanel.LocalizeText(LocalizeKey._40207.ToText(), LocalizeKey._40208.ToText());
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
            listPanel.SetSideTabInfo(
                       new UIBookListPanel.TabInfo(LocalizeKey._40222.ToText(), CostumeType.Hat),
                       new UIBookListPanel.TabInfo(LocalizeKey._40223.ToText(), CostumeType.Face),
                       new UIBookListPanel.TabInfo(LocalizeKey._40224.ToText(), CostumeType.Garment),
                       new UIBookListPanel.TabInfo(LocalizeKey._40225.ToText(), CostumeType.Weapon),
                       new UIBookListPanel.TabInfo(LocalizeKey._40241.ToText(), CostumeType.Body));
            listPanel.SetSideTab(0);
            OnClickSideTab(CostumeType.Hat);
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
            presenter.ViewEventHandler(Event.OnCostumeTypeChanged, data);
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