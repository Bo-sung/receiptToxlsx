using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIMazeSelect : UICanvas
    {
        public enum Event { EnterMultiMaze, EnterScenarioMaze, Close, }

        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UILabelHelper[] labels;
        [SerializeField] TicketBuyView ticketBuyView;
        [SerializeField] UIButtonHelper closeButton;
        [SerializeField] UIButtonHelper btnHelp;

        [SerializeField] UIButtonHelper scenarionMazeButton;
        [SerializeField] UIRewardHelper scenarioFirstClearReward;
        [SerializeField] GameObject scenarioRewardedMark;
        [SerializeField] GameObject scenarioMazeLock;

        [SerializeField] UIButtonHelper multiMazeButton;
        [SerializeField] GameObject multiMazeLock;
        [SerializeField] UILabelHelper multiMazeTicketLabel;
        [SerializeField] UILabelValue recommendPower;
        [SerializeField] UILabelHelper labelDescription;

        MazeSelectPresenter presenter;

        protected override void OnInit()
        {
            presenter = new MazeSelectPresenter(this);

            presenter.AddEvent();

            ticketBuyView.OnSelectEnter += OnSelectEnter;

            EventDelegate.Add(closeButton.OnClick, OnClickClose);
            EventDelegate.Add(scenarionMazeButton.OnClick, OnClickScenarioMaze);
            EventDelegate.Add(multiMazeButton.OnClick, OnClickMultiMaze);
            EventDelegate.Add(btnHelp.OnClick, OnClickBtnHelp);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            ticketBuyView.OnSelectEnter -= OnSelectEnter;

            EventDelegate.Remove(closeButton.OnClick, OnClickClose);
            EventDelegate.Remove(scenarionMazeButton.OnClick, OnClickScenarioMaze);
            EventDelegate.Remove(multiMazeButton.OnClick, OnClickMultiMaze);
            EventDelegate.Remove(btnHelp.OnClick, OnClickBtnHelp);
        }

        protected override void OnLocalize()
        {
            labels[0].LocalKey = LocalizeKey._48700;
            labels[1].LocalKey = LocalizeKey._48701;
            labels[2].LocalKey = LocalizeKey._48702;
            labels[3].LocalKey = LocalizeKey._48703;
            labels[4].LocalKey = LocalizeKey._2205;
            recommendPower.TitleKey = LocalizeKey._48705; // 권장 전투력
            labelDescription.LocalKey = LocalizeKey._48706; // 멀티 모드 보스 처치 시 추가 보상 획득 가능 (입장권은 자정에 초기화)
        }

        protected override void OnHide()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnBack()
        {
            if (ticketBuyView.IsShow)
            {
                ticketBuyView.Hide();
                return;
            }

            base.OnBack();
        }

        private void OnClickBtnHelp()
        {
            int info_id = presenter.MultiMazeDungeonInfoId;
            UI.Show<UIDungeonInfoPopup>().Show(info_id);
        }

        public void Show(int portalIndex)
        {
            ticketBuyView.Hide();
            presenter.OnShow(portalIndex);
        }

        public void SetScenarioMazeFirstClearRewardReceived(bool value)
        {
            scenarioRewardedMark.SetActive(value);
        }

        public void SetScenarioMazeFirstClearReward(RewardData rewardData)
        {
            scenarioFirstClearReward.SetData(rewardData);
        }

        public void SetScenarioMazeLock(bool value)
        {
            scenarioMazeLock.SetActive(value);
        }

        public void SetMultiMazeLock(bool value)
        {
            multiMazeLock.SetActive(value);
        }

        public void ShowTicketView(int requiredCatCoin)
        {
            ticketBuyView.Set(RewardType.MultiMazeTicket, requiredCatCoin);
            ticketBuyView.Show();
        }

        public void ShowRemainTicketCount(int curCount, int maxCount)
        {
            multiMazeTicketLabel.Text = string.Concat(curCount, "/", maxCount);
        }

        public void SetRecommandPower(int recommandPower)
        {
            recommendPower.Value = recommandPower.ToString("N0");
        }

        private void OnClickClose()
        {
            presenter.ViewEventHandler(Event.Close, null);
        }

        private void OnClickScenarioMaze()
        {
            isSelectSingleMaze = true;
            presenter.ViewEventHandler(Event.EnterScenarioMaze, null);
        }

        private void OnSelectEnter()
        {
            presenter.ViewEventHandler(Event.EnterMultiMaze, true);
        }

        private void OnClickMultiMaze()
        {
            isSelectMultiMaze = true;
            presenter.ViewEventHandler(Event.EnterMultiMaze, false);
        }

        #region Tutorial
        private bool isSelectSingleMaze;
        private bool isSelectMultiMaze;

        public UIWidget GetBtnSingleWidget()
        {
            return scenarionMazeButton.GetComponent<UIWidget>();
        }

        public UIWidget GetBtnMultiWidget()
        {
            return multiMazeButton.GetComponent<UIWidget>();
        }

        public bool IsSelectSingleMaze()
        {
            if (isSelectSingleMaze)
            {
                isSelectSingleMaze = false;
                return true;
            }

            return false;
        }

        public bool IsSelectMultiMaze()
        {
            if (isSelectMultiMaze)
            {
                isSelectMultiMaze = false;
                return true;
            }

            return false;
        }
        #endregion
    }
}