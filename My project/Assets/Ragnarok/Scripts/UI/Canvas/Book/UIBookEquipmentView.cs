using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Ragnarok
{
    public class UIBookEquipmentView : MonoBehaviour, IUIBookView
    {
        public enum Event { OnClassTypeChanged, OnClickSlot, OnClickLevelUp, OnClickRewardDetail }

        private enum Tab { None, Weapon, Armor }

        [SerializeField] UIBookListPanel listPanel;
        [SerializeField] UIBookLevelInfoPanel levelInfoPanel;

        private BookEquipmentViewPresenter presenter;

        public void OnInit()
        {
            presenter = new BookEquipmentViewPresenter(this);
        }

        public void OnClose()
        {
            presenter = null;
        }

        public void OnShow()
        {
            gameObject.SetActive(true);
            listPanel.gameObject.SetActive(true);

            levelInfoPanel.LocalizeText(LocalizeKey._40201.ToText(), LocalizeKey._40202.ToText()); // 장비 도감, 획득한 장비
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
            if (!BasisOpenContetsType.Shadow.IsOpend())
            {
                listPanel.SetSideTabInfo(
                    new UIBookListPanel.TabInfo(LocalizeKey._28006.ToText(), (EquipmentClassType.OneHandedSword, false)), // 아이템 타입, 쉐도우 장비 여부
                    new UIBookListPanel.TabInfo(LocalizeKey._28007.ToText(), (EquipmentClassType.OneHandedStaff, false)),
                    new UIBookListPanel.TabInfo(LocalizeKey._28008.ToText(), (EquipmentClassType.Dagger, false)),
                    new UIBookListPanel.TabInfo(LocalizeKey._28009.ToText(), (EquipmentClassType.Bow, false)),
                    new UIBookListPanel.TabInfo(LocalizeKey._28010.ToText(), (EquipmentClassType.TwoHandedSword, false)),
                    new UIBookListPanel.TabInfo(LocalizeKey._28011.ToText(), (EquipmentClassType.TwoHandedSpear, false)),
                    new UIBookListPanel.TabInfo(LocalizeKey._28012.ToText(), (EquipmentClassType.Armor, false)),
                    new UIBookListPanel.TabInfo(LocalizeKey._28013.ToText(), (EquipmentClassType.HeadGear, false)),
                    new UIBookListPanel.TabInfo(LocalizeKey._28014.ToText(), (EquipmentClassType.Garment, false)),
                    new UIBookListPanel.TabInfo(LocalizeKey._28015.ToText(), (EquipmentClassType.Accessory1, false)),
                    new UIBookListPanel.TabInfo(LocalizeKey._28016.ToText(), (EquipmentClassType.Accessory2, false)));
            }
            else
            {
                listPanel.SetSideTabInfo(
                   new UIBookListPanel.TabInfo(LocalizeKey._28006.ToText(), (EquipmentClassType.OneHandedSword, false)), // 아이템 타입, 쉐도우 장비 여부
                   new UIBookListPanel.TabInfo(LocalizeKey._28007.ToText(), (EquipmentClassType.OneHandedStaff, false)),
                   new UIBookListPanel.TabInfo(LocalizeKey._28008.ToText(), (EquipmentClassType.Dagger, false)),
                   new UIBookListPanel.TabInfo(LocalizeKey._28009.ToText(), (EquipmentClassType.Bow, false)),
                   new UIBookListPanel.TabInfo(LocalizeKey._28010.ToText(), (EquipmentClassType.TwoHandedSword, false)),
                   new UIBookListPanel.TabInfo(LocalizeKey._28011.ToText(), (EquipmentClassType.TwoHandedSpear, false)),
                   new UIBookListPanel.TabInfo(LocalizeKey._28012.ToText(), (EquipmentClassType.Armor, false)),
                   new UIBookListPanel.TabInfo(LocalizeKey._28013.ToText(), (EquipmentClassType.HeadGear, false)),
                   new UIBookListPanel.TabInfo(LocalizeKey._28014.ToText(), (EquipmentClassType.Garment, false)),
                   new UIBookListPanel.TabInfo(LocalizeKey._28015.ToText(), (EquipmentClassType.Accessory1, false)),
                   new UIBookListPanel.TabInfo(LocalizeKey._28016.ToText(), (EquipmentClassType.Accessory2, false)),
                   new UIBookListPanel.TabInfo(LocalizeKey._40240.ToText(), (EquipmentClassType.All, true)));
            }
            listPanel.SetSideTab(0);
            OnClickSideTab((EquipmentClassType.OneHandedSword, false));
        }

        public void SetCurrentLevelState(BookData lastData, BookData nextData, int curCount)
        {
            levelInfoPanel.SetLevelInfo(lastData, nextData, curCount);
        }

        private List<BookStateDecoratedData> items = new List<BookStateDecoratedData>();

        public void ShowList(List<BookStateDecoratedData> list)
        {
            items.Clear();

            list.Sort((a, b) =>
            {
                var aData = a.GetData<ItemInfo>();
                var bData = b.GetData<ItemInfo>();

                if (aData.Rating == bData.Rating)
                    return aData.BookOrder - bData.BookOrder;
                else
                    return aData.Rating - bData.Rating;
            });

            for (int i = 0; i < list.Count; ++i)
            {
                items.Add(list[i]);

                if (i == list.Count - 1)
                    break;

                if (list[i + 1].GetData<ItemInfo>().Rating != list[i].GetData<ItemInfo>().Rating)
                    while (items.Count % 4 != 0)
                        items.Add(null);
            }

            listPanel.ShowList(items);
        }

        public void RefreshList()
        {
            listPanel.RefreshList();
        }

        private void OnClickSideTab(object data)
        {
            presenter.ViewEventHandler(Event.OnClassTypeChanged, data);
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