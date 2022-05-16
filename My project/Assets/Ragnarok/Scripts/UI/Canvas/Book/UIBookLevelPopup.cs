using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIBookLevelPopup : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UILabelHelper titleLabel;
        [SerializeField] UIButtonHelper closeButton;
        [SerializeField] UIButtonHelper okButton;
        [SerializeField] UILabelHelper noticeLabel;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;

        private BookLevelPopupPresenter presenter;
        private List<BookData> items;
        private int currentLevel;
        private int curCount;
        private BookTabType tabType;

        protected override void OnInit()
        {
            presenter = new BookLevelPopupPresenter(this);
            presenter.AddEvent();

            EventDelegate.Add(closeButton.OnClick, CloseUI);
            EventDelegate.Add(okButton.OnClick, CloseUI);

            wrapper.SpawnNewList(prefab, 0, 0);
            wrapper.SetRefreshCallback(OnElementRefresh);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(closeButton.OnClick, CloseUI);
            EventDelegate.Remove(okButton.OnClick, CloseUI);
        }

        protected override void OnLocalize()
        {
            okButton.LocalKey = LocalizeKey._40229; // 확인
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        public void ShowBookLevel(BookTabType tabType)
        {
            this.tabType = tabType;

            switch (tabType)
            {
                case BookTabType.Equipment:
                    titleLabel.Text = LocalizeKey._40216.ToText(); // 장비 도감 등급
                    noticeLabel.LocalKey = LocalizeKey._40227; // 장비를 획득하면 도감에 자동 등록됩니다.\n등록된 갯수에 따라 레벨이 증가하고, 스텟을 획득할 수 있습니다.
                    break;
                case BookTabType.Card:
                    titleLabel.Text = LocalizeKey._40217.ToText(); // 카드 도감 등급
                    noticeLabel.LocalKey = LocalizeKey._40230; // 카드를 획득하면 도감에 자동 등록됩니다.\n등록된 갯수에 따라 레벨이 증가하고, 스텟을 획득할 수 있습니다.
                    break;
                case BookTabType.Monster:
                    titleLabel.Text = LocalizeKey._40218.ToText(); // 몬스터 도감 등급
                    noticeLabel.LocalKey = LocalizeKey._40231; // 몬스터를 처치하면 일정 확률로 도감에 자동 등록됩니다.\n등록된 갯수에 따라 레벨이 증가하고, 스텟을 획득할 수 있습니다.
                    break;
                case BookTabType.Costume:
                    titleLabel.Text = LocalizeKey._40219.ToText(); // 코스튬 도감 등급
                    noticeLabel.LocalKey = LocalizeKey._40232; // 코스튬를 획득하면 도감에 자동 등록됩니다.\n등록된 갯수에 따라 레벨이 증가하고, 스텟을 획득할 수 있습니다.
                    break;
                case BookTabType.Special:
                    titleLabel.Text = LocalizeKey._40234.ToText(); // 스페셜 도감 등급
                    noticeLabel.LocalKey = LocalizeKey._40236; // 스페셜 코스튬를 획득하면 도감에 자동 등록됩니다.\n등록된 갯수에 따라 레벨이 증가하고, 스텟을 획득할 수 있습니다.
                    break;

                case BookTabType.OnBuff:
                    titleLabel.Text = "#온버프 도감 등급";
                    noticeLabel.Text = "#스페셜 코스튬를 획득하면 도감에 자동 등록됩니다.\n등록된 갯수에 따라 레벨이 증가하고, 스텟을 획득할 수 있습니다.";
                    break;
            }

            presenter.OnShow(tabType);
        }

        protected override void OnHide()
        {
        }

        public void ShowList(List<BookData> items, int currentLevel, int curCount)
        {
            this.items = items;
            this.currentLevel = currentLevel;
            this.curCount = curCount;
            wrapper.Resize(items.Count);
        }

        private void CloseUI()
        {
            UI.Close<UIBookLevelPopup>();
        }

        private void OnElementRefresh(GameObject go, int index)
        {
            var slot = go.GetComponent<UIBookLevelPopupSlot>();

            UIBookLevelPopupSlot.OpenState state = UIBookLevelPopupSlot.OpenState.Opened;

            if (currentLevel + 1 == items[index].Level)
                state = UIBookLevelPopupSlot.OpenState.Challenging;
            else if (currentLevel + 1 < items[index].Level)
                state = UIBookLevelPopupSlot.OpenState.Locked;

            slot.SetData(tabType, curCount, items[index], index > 0 ? items[index - 1] : null, state);
        }
    }
}
