using UnityEngine;

namespace Ragnarok
{
    public sealed class UIOption : UICanvas, OptionPresenter.IView, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        /******************** Canvas ********************/
        [SerializeField] UIButton background;
        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UITabHelper tab;

        /******************** SubCanvas ********************/
        [SerializeField] UIOptionAccount optionAccount;
        [SerializeField] UIOptionSystem optionSystem;
        [SerializeField] UIPopupChangeName popupChangeName;
        [SerializeField] UIPopupOnBuffLink popupOnBuffLink;
        [SerializeField] UIPopupOnBuffUnLink popupOnBuffUnLink;

        OptionPresenter presenter;
        UISubCanvas currentSubCanvas;
 
        protected override void OnInit()
        {
            presenter = new OptionPresenter(this, optionAccount);

            optionSystem.Initialize(presenter);
            optionAccount.Initialize(presenter);
            popupChangeName.Initialize(presenter);

            optionAccount.onClickedBtnLanguage += ShowLanguagePopup;
            optionAccount.onClickedBtnChangeName += popupChangeName.Show;
            optionAccount.onClickedBtnDeleteMember += presenter.OnClickedBtnDeleteMember;
            optionAccount.onClickedBtnOnBuffLink += OnClickedBtnOnBuffLink;
            optionAccount.onClickedBtnOnBuffPoint += presenter.RequestOnBuffMyPoint;
            popupOnBuffLink.OnClickedBtnLink += presenter.RequestOnBuffLink;
            popupOnBuffUnLink.OnClickedBtnUnLink += presenter.RequestOnBuffUnLink;
            presenter.OnUpdateOptionSetting += OnChangePushState;
            presenter.OnBuffAccountLink += OnBuffAccountLink;
            presenter.OnBuffAccountUnLink += OnBuffAccountUnLink;

            presenter.AddEvent();
            EventDelegate.Add(background.onClick, OnBack);
            EventDelegate.Add(btnClose.OnClick, OnClickedBtnClose);
            EventDelegate.Add(tab.OnChange[0], ShowOptionAccountView);
            EventDelegate.Add(tab.OnChange[1], ShowOptionSystemView);
        }       

        protected override void OnClose()
        {
            optionAccount.onClickedBtnLanguage -= ShowLanguagePopup;
            optionAccount.onClickedBtnChangeName -= popupChangeName.Show;
            optionAccount.onClickedBtnDeleteMember -= presenter.OnClickedBtnDeleteMember;
            optionAccount.onClickedBtnOnBuffLink -= OnClickedBtnOnBuffLink;
            optionAccount.onClickedBtnOnBuffPoint -= presenter.RequestOnBuffMyPoint;
            popupOnBuffLink.OnClickedBtnLink -= presenter.RequestOnBuffLink;
            popupOnBuffUnLink.OnClickedBtnUnLink -= presenter.RequestOnBuffUnLink;
            presenter.OnUpdateOptionSetting -= OnChangePushState;
            presenter.OnBuffAccountLink -= OnBuffAccountLink;
            presenter.OnBuffAccountUnLink -= OnBuffAccountUnLink;

            presenter.RemoveEvent();
            EventDelegate.Remove(background.onClick, OnBack);
            EventDelegate.Remove(btnClose.OnClick, OnClickedBtnClose);
            EventDelegate.Remove(tab.OnChange[0], ShowOptionAccountView);
            EventDelegate.Remove(tab.OnChange[1], ShowOptionSystemView);

            if (presenter != null)
                presenter = null;

            if (currentSubCanvas != null)
                currentSubCanvas = null;
        }

        protected override void OnShow(IUIData data = null)
        {
            popupOnBuffLink.Hide();
            popupOnBuffUnLink.Hide();
            presenter.SetView();
        }

        protected override void OnHide()
        {
        }

        protected override void OnBack()
        {
            if(popupChangeName.IsActivePopup())
            {
                popupChangeName.Hide();
                return;
            }

            if(popupOnBuffLink.IsShow)
            {
                popupOnBuffLink.Hide();
                return;
            }

            if(popupOnBuffUnLink.IsShow)
            {
                popupOnBuffUnLink.Hide();
                return;
            }

            base.OnBack();
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._14000; // 옵션
            tab[0].LocalKey = LocalizeKey._14002; // 계정
            tab[1].LocalKey = LocalizeKey._14001; // 시스템
        }

        public void Refresh()
        {
            if (currentSubCanvas != null)
                currentSubCanvas.Show();
        }

        public void CloseChangeNamePopup()
        {
            popupChangeName.Hide();
        }

        void ShowLanguagePopup()
        {
            UI.Show<UILanguagePopup>();
        }

        void OnClickedBtnClose()
        {
            UI.Close<UIOption>();
        }

        private void ShowOptionAccountView()
        {
            ShowSubCanvas(optionAccount);
            optionAccount.SetOnBuffInfo(presenter.IsOnBuffAccountLink());
        }

        private void ShowOptionSystemView()
        {
            ShowSubCanvas(optionSystem);
        }

        private void ShowSubCanvas(UISubCanvas subCanvas)
        {
            if (!UIToggle.current.value)
                return;

            currentSubCanvas = subCanvas;

            HideAllSubCanvas();
            Refresh();
        }

        void OnChangePushState()
        {
            Refresh();
        }

        /// <summary>
        /// 온버프 연동 성공 이벤트
        /// </summary>
        private void OnBuffAccountLink()
        {
            if (currentSubCanvas != optionAccount)
                return;

            optionAccount.SetOnBuffInfo(presenter.IsOnBuffAccountLink());
            popupOnBuffLink.Hide();
        }

        /// <summary>
        /// 온버프 연동 해제 성공 이벤트
        /// </summary>
        private void OnBuffAccountUnLink()
        {
            if (currentSubCanvas != optionAccount)
                return;

            optionAccount.SetOnBuffInfo(presenter.IsOnBuffAccountLink());
            popupOnBuffUnLink.Hide();
        }

        /// <summary>
        /// 온버프 버튼 클릭 이벤트
        /// </summary>
        private void OnClickedBtnOnBuffLink()
        {
            if(presenter.IsOnBuffAccountLink())
            {
                popupOnBuffUnLink.SetInnoUID(presenter.GetInnoUID());
                popupOnBuffUnLink.Show();
            }
            else
            {
                popupOnBuffLink.Show();
            }
        }
    }
}