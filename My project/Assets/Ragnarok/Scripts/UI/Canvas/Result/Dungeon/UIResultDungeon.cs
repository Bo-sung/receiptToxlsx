using UnityEngine;

namespace Ragnarok
{
    public class UIResultDungeon : UICanvas, ResultClearPresenter.IView
    {
        public enum TitleType { Clear, Failed, Result }

        protected override UIType uiType => UIType.Hide;

        [SerializeField] GameObject[] resultSprites;
        [SerializeField] UIRewardHelper[] rewardHelpers;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UILabelHelper noRewardLabelDesc;
        [SerializeField] UILabelHelper labelCount;
        [SerializeField] UIButtonHelper btnRetry;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UITable tableBtn;
        [SerializeField] UIGrid rewardGrid;
        [SerializeField] Color enabledLabelCountColor;
        [SerializeField] Color disabledLabelCountColor;

        ResultClearPresenter presenter;

        private DungeonType dungeonType;

        public event System.Action OnRetryDungeon;
        public event System.Action OnFinishDungeon;

        protected override void OnInit()
        {
            presenter = new ResultClearPresenter(this);
            presenter.AddEvent();

            EventDelegate.Add(btnRetry.OnClick, OnClickedBtnRetry);
            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnRetry.OnClick, OnClickedBtnRetry);
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnShow(IUIData data)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnRetry.LocalKey = LocalizeKey._7504; // 다시하기
            btnConfirm.LocalKey = LocalizeKey._7505; // 확인
            Refresh();
        }

        public void Show(RewardData[] rewards, DungeonType dungeonType, bool isRetry, TitleType titleType, string desc)
        {
            ShowUI();

            resultSprites[0].SetActive(titleType == TitleType.Clear);
            resultSprites[1].SetActive(titleType == TitleType.Failed);
            resultSprites[2].SetActive(titleType == TitleType.Result);

            noRewardLabelDesc.Text = labelDesc.Text = desc;

            labelDesc.SetActive(rewards.Length > 0);
            noRewardLabelDesc.SetActive(rewards.Length == 0);

            this.dungeonType = dungeonType;

            btnRetry.SetActive(isRetry);
            tableBtn.Reposition();

            for (int i = 0; i < rewardHelpers.Length; ++i)
            {
                if (i < rewards.Length)
                {
                    rewardHelpers[i].gameObject.SetActive(true);
                    rewardHelpers[i].SetData(rewards[i]);
                }
                else
                {
                    rewardHelpers[i].gameObject.SetActive(false);
                }
            }

            rewardGrid.repositionNow = true;
            Refresh();
        }

        void OnClickedBtnRetry()
        {
            if (dungeonType == default)
                return;

            int freeEntryCount = presenter.GetDungeonFreeEntryCount(dungeonType);

            if (freeEntryCount == 0)
                return;

            OnRetryDungeon?.Invoke();
            HideUI();
        }

        void OnClickedBtnConfirm()
        {
            OnFinishDungeon?.Invoke();
            HideUI();
        }

        public void Refresh()
        {
            if (dungeonType == default)
                return;

            int freeEntryCount = presenter.GetDungeonFreeEntryCount(dungeonType);
            int freeEntryMaxCount = presenter.GetFreeEntryMaxCount(dungeonType);

            btnRetry.IsEnabled = freeEntryCount > 0;

            labelCount.uiLabel.color = freeEntryCount > 0 ? enabledLabelCountColor : disabledLabelCountColor;
            labelCount.Text = LocalizeKey._7503.ToText() // {COUNT}/{MAX}
                 .Replace(ReplaceKey.COUNT, freeEntryCount)
                 .Replace(ReplaceKey.MAX, freeEntryMaxCount);
        }

        private void HideUI()
        {
            Hide();
        }

        private void ShowUI()
        {
            Show();
        }

        protected override void OnBack()
        {
            OnClickedBtnConfirm();
        }
    }
}