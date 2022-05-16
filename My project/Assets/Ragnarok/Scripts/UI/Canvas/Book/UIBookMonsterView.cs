using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Ragnarok
{
    public class UIBookMonsterView : MonoBehaviour, IUIBookView
    {
        public enum Event { OnClickSlot, OnClickLevelUp, OnClickRewardDetail }

        private enum Tab { None, Weapon, Armor }

        [SerializeField] UIBookListPanel listPanel;
        [SerializeField] UIBookLevelInfoPanel levelInfoPanel;

        private BookMonsterViewPresenter presenter;

        public void OnInit()
        {
            presenter = new BookMonsterViewPresenter(this);
        }

        public void OnClose()
        {
            presenter = null;
        }

        public void OnShow()
        {
            gameObject.SetActive(true);
            listPanel.gameObject.SetActive(true);

            levelInfoPanel.LocalizeText(LocalizeKey._40205.ToText(), LocalizeKey._40206.ToText());
            listPanel.OnClickSlot += OnClickSlot;
            levelInfoPanel.OnClickLevelUp += OnClickLevelUpButton;
            levelInfoPanel.OnClickRewardsDetail += OnClickRewardsDetail;

            presenter.AddEvent();
            presenter.OnShow();
        }

        public void OnHide()
        {
            gameObject.SetActive(false);

            listPanel.OnClickSlot -= OnClickSlot;
            levelInfoPanel.OnClickLevelUp -= OnClickLevelUpButton;
            levelInfoPanel.OnClickRewardsDetail -= OnClickRewardsDetail;

            presenter.RemoveEvent();
        }

        public void SetCurrentLevelState(BookData lastData, BookData nextData, int curCount)
        {
            levelInfoPanel.SetLevelInfo(lastData, nextData, curCount);
        }

        public void ShowList(List<BookStateDecoratedData> list)
        {
            list.Sort((a, b) =>
            {
                return a.GetData<MonsterData>().dic_order - b.GetData<MonsterData>().dic_order;
            });

            listPanel.ShowList(list);
        }

        public void RefreshList()
        {
            listPanel.RefreshList();
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