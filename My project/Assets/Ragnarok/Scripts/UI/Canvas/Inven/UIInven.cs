using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIInven : UICanvas, InvenPresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single;

        public enum TabType
        {
            Equipment = 0,
            Card = 1,
            Parts = 2,
            Consumable = 3,
            Costume = 4,
        }

        public static TabType tabType = TabType.Equipment;

        /******************** Canvas ********************/
        [SerializeField] TitleView titleView;
        [SerializeField] UILabelHelper labWeight;
        [SerializeField] UIButtonHelper btnPlus;

        /******************** SubCanvas ********************/
        [SerializeField] UITabHelper tab;
        [SerializeField] EquipmentItemView equipmentItemView;
        [SerializeField] CardItemView cardItemView;
        [SerializeField] PartsItemView partsItemView;
        [SerializeField] ConsumableItemView consumableItemView;
        [SerializeField] CostumeItemView costumeItemView;

        InvenPresenter presenter;
        UISubCanvas currentSubCanvas;

        public UIWidget FirstItemWidget { get { return equipmentItemView.FirstItemWidget; } }

        protected override void OnInit()
        {
            presenter = new InvenPresenter(this);

            equipmentItemView.Initialize(presenter);
            cardItemView.Initialize(presenter);
            partsItemView.Initialize(presenter);
            consumableItemView.Initialize(presenter);
            costumeItemView.Initialize(presenter);

            presenter.AddEvent();

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);

            EventDelegate.Add(tab.OnChange[0], ShowEquipmentItemView);
            EventDelegate.Add(tab.OnChange[1], ShowCardItemView);
            EventDelegate.Add(tab.OnChange[2], ShowPartsItemView);
            EventDelegate.Add(tab.OnChange[3], ShowConsumableItemView);
            EventDelegate.Add(tab.OnChange[4], ShowCostumeItemView);
            EventDelegate.Add(btnPlus.OnClick, OnClickedBtnInvenExpand);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(tab.OnChange[0], ShowEquipmentItemView);
            EventDelegate.Remove(tab.OnChange[1], ShowCardItemView);
            EventDelegate.Remove(tab.OnChange[2], ShowPartsItemView);
            EventDelegate.Remove(tab.OnChange[3], ShowConsumableItemView);
            EventDelegate.Remove(tab.OnChange[4], ShowCostumeItemView);
            EventDelegate.Remove(btnPlus.OnClick, OnClickedBtnInvenExpand);
        }

        protected override void OnShow(IUIData data = null)
        {
            UIToggle.current = null;

            if (tab[(int)tabType].Value != true)
            {
                tab[(int)tabType].Value = true;
            }
            else
            {
                if (tabType == TabType.Equipment)
                    ShowEquipmentItemView();
                else if (tabType == TabType.Card)
                    ShowCardItemView();
                else if (tabType == TabType.Parts)
                    ShowPartsItemView();
                else if (tabType == TabType.Consumable)
                    ShowConsumableItemView();
                else if (tabType == TabType.Costume)
                    ShowCostumeItemView();
            }

            for (int i = 0; i < tab.Count; i++)
            {
                tab[i].SetNotice(presenter.IsNew(i));
            }
        }

        protected override void OnHide()
        {
            HideAllNew();
        }

        protected override void OnLocalize()
        {
            titleView.ShowTitle(LocalizeKey._6000.ToText()); // 가방
            tab[0].LocalKey = LocalizeKey._6001; // 장비
            tab[1].LocalKey = LocalizeKey._6002; // 카드
            tab[2].LocalKey = LocalizeKey._6003; // 재료
            tab[3].LocalKey = LocalizeKey._6004; // 소비
            tab[4].LocalKey = LocalizeKey._6017; // 코스튬
        }

        public void UpdateView()
        {
            if (currentSubCanvas == null)
                return;

            for (int i = 0; i < tab.Count; i++)
            {
                tab[i].SetNotice(presenter.IsNew(i));
            }

            currentSubCanvas.Show();
        }

        private void CloseUI()
        {
            UI.Close<UIInven>();
        }

        public void SortItem()
        {
            if (currentSubCanvas is EquipmentItemView)
            {
                equipmentItemView.SortItem();
            }
            else if (currentSubCanvas is CardItemView)
            {
                cardItemView.SortItem();
            }
            else if (currentSubCanvas is PartsItemView)
            {
                partsItemView.SortItem();
            }
            else if (currentSubCanvas is ConsumableItemView)
            {
                consumableItemView.SortItem();
            }
            else if (currentSubCanvas is CostumeItemView)
            {
                costumeItemView.SortItem();
            }
        }

        private void ShowEquipmentItemView()
        {
            if (UIToggle.current != null && !UIToggle.current.value)
                return;

            tabType = TabType.Equipment;
            equipmentItemView.SortItem();
            ShowSubCanvas(equipmentItemView);
        }

        private void ShowCardItemView()
        {
            if (UIToggle.current != null && !UIToggle.current.value)
                return;

            tabType = TabType.Card;
            cardItemView.SortItem();
            ShowSubCanvas(cardItemView);
        }

        private void ShowPartsItemView()
        {
            if (UIToggle.current != null && !UIToggle.current.value)
                return;

            tabType = TabType.Parts;
            partsItemView.SortItem();
            ShowSubCanvas(partsItemView);
        }

        private void ShowConsumableItemView()
        {
            if (UIToggle.current != null && !UIToggle.current.value)
                return;

            tabType = TabType.Consumable;
            consumableItemView.SortItem();
            ShowSubCanvas(consumableItemView);
        }

        private void ShowCostumeItemView()
        {
            if (UIToggle.current != null && !UIToggle.current.value)
                return;

            tabType = TabType.Costume;
            costumeItemView.SortItem();
            ShowSubCanvas(costumeItemView);
        }

        private void ShowSubCanvas(UISubCanvas subCanvas)
        {
            HideAllNew();

            currentSubCanvas = subCanvas;

            HideAllSubCanvas();
            UpdateView();
        }

        void InvenPresenter.IView.ShowInvenWeight(int invenWeight, int currentInvenWeight)
        {
            labWeight.Text = $"{(currentInvenWeight * 0.1f):0.#}/{(invenWeight * 0.1f):0.#}";
        }

        EquipmentItemView InvenPresenter.IView.GetEquipmentItemView()
        {
            return equipmentItemView;
        }

        CardItemView InvenPresenter.IView.GetCardItemView()
        {
            return cardItemView;
        }

        CostumeItemView InvenPresenter.IView.GetCostumeView()
        {
            return costumeItemView;
        }

        // 인벤 확장 버튼
        void OnClickedBtnInvenExpand()
        {
            presenter.RequestInvenExpand();
        }

        void InvenPresenter.IView.SetZeny(long value)
        {
            titleView.ShowZeny(value);
        }

        void InvenPresenter.IView.SetCatCoin(long value)
        {
            titleView.ShowCatCoin(value);
        }

        private void HideAllNew()
        {
            if (equipmentItemView.IsVisible)
            {
                equipmentItemView.HideAllNew();
            }
            else if (cardItemView.IsVisible)
            {
                cardItemView.HideAllNew();
            }
            else if (partsItemView.IsVisible)
            {
                partsItemView.HideAllNew();
            }
            else if (consumableItemView.IsVisible)
            {
                consumableItemView.HideAllNew();
            }
            else if (costumeItemView.IsVisible)
            {
                costumeItemView.HideAllNew();
            }
        }
    }
}