using UnityEngine;

namespace Ragnarok
{
    public class UILanguagePopup : UICanvas, LanguagePopupPresenter.IView, IInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButton btnClose;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UILanguageInfo[] languageInfos;
        [SerializeField] UIButtonHelper btnYes;

        LanguagePopupPresenter preseneter;

        private System.Action onFinish;

        protected override void OnInit()
        {
            preseneter = new LanguagePopupPresenter(this);

            foreach (var item in languageInfos)
            {
                item.Initialize(preseneter);
            }

            EventDelegate.Add(btnClose.onClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnYes.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnClose.onClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnYes.OnClick, CloseUI);
        }

        protected override void OnShow(IUIData data = null)
        {
            Refresh();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._11; // 언어 선택
            btnYes.LocalKey = LocalizeKey._1; // 확인
        }

        public void Refresh()
        {
            foreach (var item in languageInfos)
            {
                item.Refresh();
            }
        }

        public void OnFinish(System.Action onFinish)
        {
            this.onFinish = onFinish;
        }

        void CloseUI()
        {
            UI.Close<UILanguagePopup>();
            if (onFinish != null)
            {
                onFinish?.Invoke();
                onFinish = null;
            }
        }

        public override bool Find()
        {
            base.Find();

            languageInfos = GetComponentsInChildren<UILanguageInfo>(includeInactive: true);
            return true;
        }
    }
}