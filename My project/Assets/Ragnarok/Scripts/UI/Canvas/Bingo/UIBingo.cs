using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIBingo : UICanvas
    {
        public enum PoringTextType { None, AllMissionCleared, DoCheck, GetReward }
        public enum Event { OnClickCheck, OnClickLineReward, OnClickMissionButton, OnClickGetFinalReward, OnClickClose }

        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] GameObject viewRoot;

        [SerializeField] UILabelHelper titleLabel;
        [SerializeField] UILabelHelper titleLabel2;
        [SerializeField] UILabelHelper remainSeasonTimeLabel;
        [SerializeField] UILabelHelper bingoStateNoticeLabel;
        [SerializeField] UILabelHelper finalRewardPanelLabel;
        [SerializeField] UILabelHelper finalRewardPanelNoticeLabel;
        [SerializeField] UILabelHelper missionPanelLabel;
        [SerializeField] UILabelHelper missionLabel;
        [SerializeField] UILabelHelper remainMissionLabel;

        [SerializeField] UIButtonHelper okButton;
        [SerializeField] UIButtonHelper closeButon;
        [SerializeField] UIButtonHelper finalRewardButton;
        [SerializeField] TweenScale tweenfinalRewardButton;
        [SerializeField] GameObject finalRewardButtonLabelBG;
        [SerializeField] UIButtonHelper missionButton;
        [SerializeField] TweenScale tweenMisionButton;

        [SerializeField] GameObject bingoStateBalloon;
        [SerializeField] UIRewardHelper missionRewardSlot;
        [SerializeField] UIRewardHelper finalRewardSlot;
        [SerializeField] GameObject finalRewardCompleteMark;
        [SerializeField] BingoCheckSlot[] bingoCheckSlots;
        [SerializeField] BingoLineRewardSlot[] bingoLineRewardSlots;

        private BingoPresenter presenter;
        private RemainTime remainTime;
        private float updateTimer;

        protected override void OnInit()
        {
            presenter = new BingoPresenter(this);
            presenter.AddEvent();

            EventDelegate.Add(okButton.OnClick, OnClickOK);
            EventDelegate.Add(missionButton.OnClick, OnClickMissionButtonClicked);
            EventDelegate.Add(finalRewardButton.OnClick, OnClickGetFinalReward);
            EventDelegate.Add(closeButon.OnClick, OnClickOK);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(okButton.OnClick, OnClickOK);
            EventDelegate.Remove(missionButton.OnClick, OnClickMissionButtonClicked);
            EventDelegate.Remove(finalRewardButton.OnClick, OnClickGetFinalReward);
            EventDelegate.Remove(closeButon.OnClick, OnClickOK);
        }

        protected override void OnShow(IUIData data = null)
        {
            viewRoot.SetActive(false);

            updateTimer = 0;
            remainSeasonTimeLabel.Text = "";

            presenter.OnShow();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            okButton.Text = LocalizeKey._45103.ToText();
            titleLabel.Text = LocalizeKey._40102.ToText();
            titleLabel2.Text = LocalizeKey._40102.ToText();
            finalRewardPanelLabel.Text = LocalizeKey._40103.ToText();
            missionPanelLabel.Text = LocalizeKey._40104.ToText();
            finalRewardPanelNoticeLabel.Text = LocalizeKey._40105.ToText();
        }

        public void ShowView()
        {
            viewRoot.SetActive(true);
        }

        public void SetPoringText(PoringTextType textType)
        {
            if (textType == PoringTextType.None)
            {
                bingoStateBalloon.SetActive(false);
            }
            else
            {
                bingoStateBalloon.SetActive(true);

                if (textType == PoringTextType.AllMissionCleared)
                    bingoStateNoticeLabel.Text = LocalizeKey._40106.ToText();
                else if (textType == PoringTextType.DoCheck)
                    bingoStateNoticeLabel.Text = LocalizeKey._40107.ToText();
                else if (textType == PoringTextType.GetReward)
                    bingoStateNoticeLabel.Text = LocalizeKey._40108.ToText();
            }
        }

        public void SetRemainTime(RemainTime remainTime)
        {
            this.remainTime = remainTime;
            UpdateRemainTime();
        }

        private void Update()
        {
            updateTimer -= Time.deltaTime;
            if (updateTimer > 0)
                return;
            updateTimer = 1;
            UpdateRemainTime();
        }

        private void UpdateRemainTime()
        {
            var timeSpan = remainTime.ToRemainTime().ToTimeSpan();

            if (timeSpan.Days > 0)
                remainSeasonTimeLabel.Text = LocalizeKey._40109.ToText().Replace(ReplaceKey.DAYS, timeSpan.Days).Replace(ReplaceKey.TIME, timeSpan.ToString(@"hh\:mm\:ss"));
            else
                remainSeasonTimeLabel.Text = LocalizeKey._40110.ToText().Replace(ReplaceKey.TIME, timeSpan.ToString(@"hh\:mm\:ss"));
        }

        public void SetMission(QuestInfo missionData, int remainMissionCount)
        {
            missionRewardSlot.SetData(missionData.RewardDatas[0]);
            missionLabel.Text = $"{missionData.ConditionText} ({missionData.CurrentValue}/{missionData.MaxValue})";
            remainMissionLabel.Text = LocalizeKey._40115.ToText().Replace(ReplaceKey.COUNT, remainMissionCount);

            if (missionData.CompleteType == QuestInfo.QuestCompleteType.ReceivedReward)
            {
                missionButton.Text = LocalizeKey._40111.ToText();
                missionButton.IsEnabled = false;
                missionButton.SpriteName = "Ui_Common_Btn_03";
                tweenMisionButton.enabled = false;
                tweenMisionButton.tweenFactor = 0;
            }
            else if (missionData.CompleteType == QuestInfo.QuestCompleteType.StandByReward)
            {
                missionButton.Text = LocalizeKey._40112.ToText();
                missionButton.IsEnabled = true;
                missionButton.SpriteName = "Ui_Common_Btn_01";
                tweenMisionButton.enabled = true;
                tweenMisionButton.tweenFactor = 0;
            }
            else
            {
                missionButton.Text = LocalizeKey._40113.ToText();
                missionButton.IsEnabled = true;
                missionButton.SpriteName = "Ui_Common_Btn_02";
                tweenMisionButton.enabled = false;
                tweenMisionButton.tweenFactor = 0;
            }
        }

        public void SetFinalRewardPanel(BingoStateDecoratedData rewardData, int checkedCount)
        {
            finalRewardSlot.SetData(rewardData.bingoData.GetReward());

            if (rewardData.state == BingoStateDecoratedData.State.Rewarded)
            {
                finalRewardButton.Text = LocalizeKey._40114.ToText();
                finalRewardButtonLabelBG.SetActive(false);
                finalRewardButton.IsEnabled = false;
                finalRewardCompleteMark.SetActive(true);
                tweenfinalRewardButton.enabled = false;
                tweenfinalRewardButton.tweenFactor = 0;
            }
            else if (checkedCount == 25)
            {
                finalRewardButton.Text = LocalizeKey._40112.ToText();
                finalRewardButtonLabelBG.SetActive(false);
                finalRewardButton.IsEnabled = true;
                finalRewardCompleteMark.SetActive(false);
                tweenfinalRewardButton.enabled = true;
                tweenfinalRewardButton.tweenFactor = 0;
            }
            else
            {
                finalRewardButton.Text = $"{checkedCount}/25";
                finalRewardButtonLabelBG.SetActive(true);
                finalRewardButton.IsEnabled = false;
                finalRewardCompleteMark.SetActive(false);
                tweenfinalRewardButton.enabled = false;
                tweenfinalRewardButton.tweenFactor = 0;
            }
        }

        public void SetBingoBoard(IEnumerable<BingoStateDecoratedData> datas)
        {
            foreach (var each in datas)
            {
                var data = each.bingoData;

                if (each.IsLineRewardData)
                {
                    var slot = GetRewardSlot(data);
                    slot.SetData(each, OnClickLineRewardSlot);
                }
                else
                {
                    var slot = GetCheckSlot(data);
                    slot.SetData(each, OnClickCheckSlot);
                }
            }
        }

        public void AA()
        {
            presenter.Ref();
        }

        public void RefreshBoard()
        {
            foreach (var each in bingoCheckSlots)
                each.Refresh();
            foreach (var each in bingoLineRewardSlots)
                each.Refresh();
        }

        private BingoCheckSlot GetCheckSlot(BingoData bingoData)
        {
            return bingoCheckSlots[bingoData.bingo_x - 1 + (bingoData.bingo_y - 1) * 5];
        }

        private BingoLineRewardSlot GetRewardSlot(BingoData bingoData)
        {
            return bingoLineRewardSlots[bingoData.bingo_x + bingoData.bingo_y - 1];
        }

        private void OnClickCheckSlot(BingoStateDecoratedData data)
        {
            presenter.ViewEventHandler(Event.OnClickCheck, data);
        }

        private void OnClickLineRewardSlot(BingoStateDecoratedData data)
        {
            presenter.ViewEventHandler(Event.OnClickLineReward, data);
        }

        private void OnClickOK()
        {
            presenter.ViewEventHandler(Event.OnClickClose, null);
        }

        private void OnClickMissionButtonClicked()
        {
            presenter.ViewEventHandler(Event.OnClickMissionButton, null);
        }

        private void OnClickGetFinalReward()
        {
            presenter.ViewEventHandler(Event.OnClickGetFinalReward, null);
        }
    }
}