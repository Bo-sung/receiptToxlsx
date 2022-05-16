using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIBookCardView : MonoBehaviour, IUIBookView
    {
        public enum Event { OnClassTypeChanged, OnClickSlot, OnClickLevelUp, OnClickRewardDetail }

        [SerializeField] UIBookListPanel listPanel;
        [SerializeField] UIBookLevelInfoPanel levelInfoPanel;

        private BookCardViewPresenter presenter;

        public void OnInit()
        {
            presenter = new BookCardViewPresenter(this);
        }

        public void OnClose()
        {
            presenter = null;
        }
        
        public void OnShow()
        {
            gameObject.SetActive(true);
            listPanel.gameObject.SetActive(true);

            levelInfoPanel.LocalizeText(LocalizeKey._40203.ToText(), LocalizeKey._40204.ToText());
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
                           new UIBookListPanel.TabInfo(LocalizeKey._40220.ToText(), null),
                           new UIBookListPanel.TabInfo(LocalizeKey._40221.ToText(), (EquipmentClassType.All, false)),
                           new UIBookListPanel.TabInfo(LocalizeKey._28017.ToText(), (EquipmentClassType.Weapon, false)),
                           new UIBookListPanel.TabInfo(LocalizeKey._28012.ToText(), (EquipmentClassType.Armor, false)),
                           new UIBookListPanel.TabInfo(LocalizeKey._28013.ToText(), (EquipmentClassType.HeadGear, false)),
                           new UIBookListPanel.TabInfo(LocalizeKey._28014.ToText(), (EquipmentClassType.Garment, false)),
                           new UIBookListPanel.TabInfo(LocalizeKey._28015.ToText(), (EquipmentClassType.Accessory1, false)),
                           new UIBookListPanel.TabInfo(LocalizeKey._28016.ToText(), (EquipmentClassType.Accessory2, false)));
            }
            else
            {
                listPanel.SetSideTabInfo(
                           new UIBookListPanel.TabInfo(LocalizeKey._40220.ToText(), null),
                           new UIBookListPanel.TabInfo(LocalizeKey._40221.ToText(), (EquipmentClassType.All, false)),
                           new UIBookListPanel.TabInfo(LocalizeKey._28017.ToText(), (EquipmentClassType.Weapon, false)),
                           new UIBookListPanel.TabInfo(LocalizeKey._28012.ToText(), (EquipmentClassType.Armor, false)),
                           new UIBookListPanel.TabInfo(LocalizeKey._28013.ToText(), (EquipmentClassType.HeadGear, false)),
                           new UIBookListPanel.TabInfo(LocalizeKey._28014.ToText(), (EquipmentClassType.Garment, false)),
                           new UIBookListPanel.TabInfo(LocalizeKey._28015.ToText(), (EquipmentClassType.Accessory1, false)),
                           new UIBookListPanel.TabInfo(LocalizeKey._28016.ToText(), (EquipmentClassType.Accessory2, false)),
                           new UIBookListPanel.TabInfo(LocalizeKey._40240.ToText(), (EquipmentClassType.All, true)));
            }


            listPanel.SetSideTab(0);
            OnClickSideTab(null);
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

                if (aData.IsShadow != bData.IsShadow)
                {
                    return aData.IsShadow ? 1 : -1;
                }
                else if (aData.Rating == bData.Rating)
                {
                    return aData.BookOrder - bData.BookOrder;
                }
                else
                {
                    return aData.Rating - bData.Rating;
                }
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
