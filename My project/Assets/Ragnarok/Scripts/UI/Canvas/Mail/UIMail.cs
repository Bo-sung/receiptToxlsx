using UnityEngine;

namespace Ragnarok
{
    public class UIMail : UICanvas, MailPresenter.IView, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] protected UITabHelper tab;
        [SerializeField] protected MailSubView accountView;
        [SerializeField] MailSubView characterView;
        [SerializeField] MailSubView shopView;
        [SerializeField] MailSubView tradeView;

        protected MailPresenter presenter;
        UISubCanvas currentSubCanvas;

        protected override void OnInit()
        {
            presenter = new MailPresenter(this);

            accountView.Initialize(presenter);
            characterView.Initialize(presenter);
            shopView.Initialize(presenter);
            tradeView.Initialize(presenter);

            presenter.AddEvent();
            EventDelegate.Add(btnClose.OnClick, OnBack);
            EventDelegate.Add(tab[0].OnChange, ShowAccountSubView);
            EventDelegate.Add(tab[1].OnChange, ShowCharacterSubView);
            EventDelegate.Add(tab[2].OnChange, ShowShopSubView);
            EventDelegate.Add(tab[3].OnChange, ShowTradeSubView);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            EventDelegate.Remove(btnClose.OnClick, OnBack);
            EventDelegate.Remove(tab[0].OnChange, ShowAccountSubView);
            EventDelegate.Remove(tab[1].OnChange, ShowCharacterSubView);
            EventDelegate.Remove(tab[2].OnChange, ShowShopSubView);
            EventDelegate.Remove(tab[3].OnChange, ShowTradeSubView);
        }

        protected override void OnShow(IUIData data = null)
        {
            accountView.Hide();
            characterView.Hide();
            shopView.Hide();
            tradeView.Hide();
        }

        protected override void OnHide()
        {
            if (currentSubCanvas == null)
                return;

            currentSubCanvas.Hide();
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._12000; // 우편함
            tab[0].LocalKey = LocalizeKey._12004; // 계정
            tab[1].LocalKey = LocalizeKey._12005; // 캐릭터
            tab[2].LocalKey = LocalizeKey._12006; // 상점
            tab[3].LocalKey = LocalizeKey._12008; // 거래
        }

        public void Set(int tabIndex)
        {
            tab[tabIndex].Value = true;
        }

        public void Refresh()
        {
            if (currentSubCanvas == null)
                return;

            currentSubCanvas.Show();
        }

        public virtual async void ShowAccountSubView()
        {
            if (!UIToggle.current.value)
                return;

            await presenter.RequestMailList(MailType.Account);
            ShowSubCanvas(accountView);
            accountView.ResetPosition();
        }

        private async void ShowCharacterSubView()
        {
            if (!UIToggle.current.value)
                return;

            await presenter.RequestMailList(MailType.Character);
            ShowSubCanvas(characterView);
            characterView.ResetPosition();
        }

        private async void ShowShopSubView()
        {
            if (!UIToggle.current.value)
                return;

            await presenter.RequestMailList(MailType.Shop);
            ShowSubCanvas(shopView);
            shopView.ResetPosition();
        }

        private async void ShowTradeSubView()
        {
            if (!UIToggle.current.value)
                return;

            await presenter.RequestMailList(MailType.Trade);
            ShowSubCanvas(tradeView);
            tradeView.ResetPosition();
        }

        protected void ShowSubCanvas(UISubCanvas subCanvas)
        {
            currentSubCanvas = subCanvas;

            HideAllSubCanvas();
            Refresh();
        }

        public virtual void SetMailNew(AlarmType alarmType)
        {
            tab[0].SetNotice(alarmType.HasFlag(AlarmType.MailAccount));
            tab[1].SetNotice(alarmType.HasFlag(AlarmType.MailCharacter));
            tab[2].SetNotice(alarmType.HasFlag(AlarmType.MailShop));
            tab[3].SetNotice(alarmType.HasFlag(AlarmType.MailTrade));
        }
    }
}