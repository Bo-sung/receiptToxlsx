using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public interface IUIBookView
    {
        void OnShow();
        void OnHide();
    }

    public class UIBook : UICanvas
    {
        [SerializeField] TitleView titleView;
        [SerializeField] protected UITabHelper mainTab;
        [SerializeField] UIBookEquipmentView equipmentView;
        [SerializeField] UIBookCardView cardView;
        [SerializeField] UIBookMonsterView monsterView;
        [SerializeField] UIBookCostumeView costumeView;
        [SerializeField] UIBookSpecialView specialView;

        [SerializeField] UIBookListPanel[] listPanels;

        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single;

        private BookPresenter presenter;
        private IUIBookView curShowingView;

        protected override void OnInit()
        {
            presenter = new BookPresenter(this);
            presenter.AddEvent();
            presenter.OnUpdateGoodsZeny += OnUpdateGoodsZeny;
            presenter.OnUpdateGoodsCatCoin += OnUpdateGoodsCatCoin;
            AddEventButtons();

            foreach (var each in listPanels)
            {
                each.Init();
                each.gameObject.SetActive(false);
            }

            InitView();
            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);
        }

        protected virtual void AddEventButtons()
        {
            EventDelegate.Add(mainTab[0].OnChange, OnTabChangeEquipment);
            EventDelegate.Add(mainTab[1].OnChange, OnTabChangeCard);
            EventDelegate.Add(mainTab[2].OnChange, OnTabChangeMonster);
            EventDelegate.Add(mainTab[3].OnChange, OnTabChangeCostume);
            EventDelegate.Add(mainTab[4].OnChange, OnTabChangeSpecial);
        }

        protected virtual void InitView()
        {
            equipmentView.OnInit();
            cardView.OnInit();
            monsterView.OnInit();
            costumeView.OnInit();
            specialView.OnInit();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            presenter.OnUpdateGoodsZeny -= OnUpdateGoodsZeny;
            presenter.OnUpdateGoodsCatCoin -= OnUpdateGoodsCatCoin;
            RemoveEventButtons();

            if (curShowingView != null)
            {
                HideListView();
                curShowingView.OnHide();
            }
            curShowingView = null;

            CloseView();
        }

        protected virtual void RemoveEventButtons()
        {
            EventDelegate.Remove(mainTab[0].OnChange, OnTabChangeEquipment);
            EventDelegate.Remove(mainTab[1].OnChange, OnTabChangeCard);
            EventDelegate.Remove(mainTab[2].OnChange, OnTabChangeMonster);
            EventDelegate.Remove(mainTab[3].OnChange, OnTabChangeCostume);
            EventDelegate.Remove(mainTab[4].OnChange, OnTabChangeSpecial);
        }

        protected virtual void CloseView()
        {
            equipmentView.OnClose();
            cardView.OnClose();
            monsterView.OnClose();
            costumeView.OnClose();
            specialView.OnClose();
        }

        protected override void OnLocalize()
        {
            titleView.ShowTitle(LocalizeKey._40200.ToText()); // 무한의 공간
            mainTab[0].Text = LocalizeKey._4055.ToText(); // 장비
            mainTab[1].Text = LocalizeKey._6002.ToText(); // 카드
            mainTab[2].Text = LocalizeKey._8502.ToText(); // 몬스터
            mainTab[3].Text = LocalizeKey._4056.ToText(); // 코스튬
            mainTab[4].Text = LocalizeKey._40233.ToText(); // 스페셜
        }

        protected override void OnShow(IUIData data = null)
        {
            ShowTab();

            if (curShowingView == null)
                SetCurView(equipmentView);

            presenter.OnShow();

            titleView.ShowZeny(presenter.GetZeny());
            titleView.ShowCatCoin(presenter.GetCatCoin());
        }

        protected virtual void ShowTab()
        {
            mainTab[0].Value = true;
            mainTab[1].Value = false;
            mainTab[2].Value = false;
            mainTab[3].Value = false;
            mainTab[4].Value = false;
        }

        protected override void OnHide()
        {
            if (curShowingView != null)
            {
                HideListView();
                curShowingView.OnHide();
            }
            curShowingView = null;
        }

        void OnUpdateGoodsZeny()
        {
            titleView.ShowZeny(presenter.GetZeny());
        }

        void OnUpdateGoodsCatCoin()
        {
            titleView.ShowCatCoin(presenter.GetCatCoin());
        }

        private void OnTabChangeEquipment()
        {
            if (!mainTab[0].Value)
                return;

            SetCurView(equipmentView);
        }

        private void OnTabChangeCard()
        {
            if (!mainTab[1].Value)
                return;

            SetCurView(cardView);
        }

        private void OnTabChangeMonster()
        {
            if (!mainTab[2].Value)
                return;

            SetCurView(monsterView);
        }

        private void OnTabChangeCostume()
        {
            if (!mainTab[3].Value)
                return;

            SetCurView(costumeView);
        }

        private void OnTabChangeSpecial()
        {
            if (!mainTab[4].Value)
                return;

            SetCurView(specialView);
        }

        protected void SetCurView(IUIBookView view)
        {
            if (curShowingView != null)
            {
                HideListView();
                curShowingView.OnHide();
            }
            curShowingView = view;
            curShowingView.OnShow();
        }

        private void HideListView()
        {
            foreach (var each in listPanels)
                each.gameObject.SetActive(false);
        }

        public void SetNotice(BookTabType tabType, bool value)
        {
            int index = tabType.ToIntValue();

            // 탭 크기 벗어남
            if (index >= mainTab.Count)
                return;

            mainTab[index].SetNotice(value);
        }
    }
}