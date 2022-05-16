using UnityEngine;

namespace Ragnarok
{
    public sealed class UIDungeonInfoPopup : UICanvas<DungeonInfoPopupPresenter>, DungeonInfoPopupPresenter.IView, IInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButton blackCover;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UILabelValue[] labelTitleValue;
        [SerializeField] UIButtonHelper btnConfirm;

        protected override void OnInit()
        {
            presenter = new DungeonInfoPopupPresenter(this);
            presenter.AddEvent();

            EventDelegate.Add(blackCover.onClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnConfirm.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(blackCover.onClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnConfirm.OnClick, CloseUI);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        public void Show(int dungeonInfoId)
        {
            if (dungeonInfoId == 0)
            {
#if UNITY_EDITOR
                Debug.LogError($"던전 인포 아이디가 0이다.");
#endif
                CloseUI();
                return;
            }

            Show();

            labelMainTitle.Text = presenter.GetTitle(dungeonInfoId);

            string[] titles = presenter.GetTitles(dungeonInfoId);
            string[] descriptions = presenter.GetDescriptions(dungeonInfoId);

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

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnConfirm.LocalKey = LocalizeKey._7100; // 확인
        }

        private void CloseUI()
        {
            UI.Close<UIDungeonInfoPopup>();
        }

        public override bool Find()
        {
            base.Find();

            labelTitleValue = GetComponentsInChildren<UILabelValue>();
            return true;
        }
    }
}