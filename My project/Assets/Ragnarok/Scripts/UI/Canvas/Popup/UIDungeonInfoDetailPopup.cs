using UnityEngine;

namespace Ragnarok
{
    public sealed class UIDungeonInfoDetailPopup : UICanvas<DungeonInfoPopupPresenter>, DungeonInfoPopupPresenter.IView, IInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        private const int TAB_INDEX_1 = 0;
        private const int TAB_INDEX_2 = 1;

        [SerializeField] UIButton blackCover;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UITabHelper tab;
        [SerializeField] UILabelValue[] labelTitleValue;
        [SerializeField] UIButtonHelper btnConfirm;

        private int info1, info2;

        protected override void OnInit()
        {
            presenter = new DungeonInfoPopupPresenter(this);
            presenter.AddEvent();

            EventDelegate.Add(blackCover.onClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnConfirm.OnClick, CloseUI);

            tab.OnSelect += OnSelectTab;
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(blackCover.onClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnConfirm.OnClick, CloseUI);

            tab.OnSelect -= OnSelectTab;
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnConfirm.LocalKey = LocalizeKey._7100; // 확인
            labelMainTitle.LocalKey = LocalizeKey._7102; // 가이드
        }

        void OnSelectTab(int index)
        {
            int infoId = 0;
            switch (index)
            {
                default:
                case TAB_INDEX_1:
                    infoId = info1;
                    break;

                case TAB_INDEX_2:
                    infoId = info2;
                    break;
            }

            string[] titles = presenter.GetTitles(infoId);
            string[] descriptions = presenter.GetDescriptions(infoId);

            for (int i = 0; i < labelTitleValue.Length; i++)
            {
                bool hasData = i < titles.Length;
                labelTitleValue[i].SetActive(hasData);

                if (!hasData)
                    continue;

                labelTitleValue[i].Title = titles[i];
                labelTitleValue[i].Value = descriptions[i];
            }
        }

        public void Show(int info1, int info2)
        {
            if (info1 == 0 || info2 == 0)
            {
#if UNITY_EDITOR
                Debug.LogError($"던전 인포 아이디가 음슴.");
#endif
                CloseUI();
                return;
            }

            this.info1 = info1;
            this.info2 = info2;

            tab[TAB_INDEX_1].Text = presenter.GetTitle(this.info1);
            tab[TAB_INDEX_2].Text = presenter.GetTitle(this.info2);

            tab[TAB_INDEX_1].Set(false, false);
            tab.Value = TAB_INDEX_1;
        }

        private void CloseUI()
        {
            UI.Close<UIDungeonInfoDetailPopup>();
        }

        public override bool Find()
        {
            base.Find();

            labelTitleValue = GetComponentsInChildren<UILabelValue>();
            return true;
        }
    }
}