using MEC;
using Ragnarok.View;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIAdventureMap : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single;

        [SerializeField] SwitchWidgetMaterialColor uvTexture;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIPlayTween tweenNotice;
        [SerializeField] UILabelHelper labelNotice;

        [Header("Views")]
        [SerializeField] AdventureView adventureView;
        [SerializeField] EventStageEntryPopupView eventStageEntryPopupView;

        [Header("Bottom-Right")]
        [SerializeField] UIGrid gridButton;
        [SerializeField] UIButtonHelper btnMap;
        [SerializeField] UIButtonWithIconHelper btnMvpReward;
        [SerializeField] UIButtonHelper btnEventMode;
        [SerializeField] UIButtonHelper btnGuide, btnRank, btnChallenge, btnNormalMode;

        AdventureMapPresenter presenter;

        private int curChapter;
        private bool isEvent;

        protected override void OnInit()
        {
            presenter = new AdventureMapPresenter();

            adventureView.OnSelectChapter += SelectChapter;
            adventureView.OnSelectStage += OnSelectStage;

            EventDelegate.Add(btnMap.OnClick, OnClickedBtnMap);
            EventDelegate.Add(btnMvpReward.OnClick, OnClickedBtnReward);
            EventDelegate.Add(btnEventMode.OnClick, OnClickedBtnEventMode);

            EventDelegate.Add(btnGuide.OnClick, OnClickedBtnGuide);
            EventDelegate.Add(btnRank.OnClick, OnClickedBtnRank);
            EventDelegate.Add(btnChallenge.OnClick, OnClickedBtnChallenge);
            EventDelegate.Add(btnNormalMode.OnClick, OnClickedBtnNormalMode);

            presenter.OnUpdateEventStageInfo += UpdateEventStageInfo;
            presenter.OnUpdateEventStageCount += UpdateChallengeNotice;
            presenter.OnUpdateClearedStage += UpdateChallengeNotice;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnUpdateEventStageInfo -= UpdateEventStageInfo;
            presenter.OnUpdateEventStageCount -= UpdateChallengeNotice;
            presenter.OnUpdateClearedStage -= UpdateChallengeNotice;

            EventDelegate.Remove(btnMap.OnClick, OnClickedBtnMap);
            EventDelegate.Remove(btnMvpReward.OnClick, OnClickedBtnReward);
            EventDelegate.Remove(btnEventMode.OnClick, OnClickedBtnEventMode);

            EventDelegate.Remove(btnGuide.OnClick, OnClickedBtnGuide);
            EventDelegate.Remove(btnRank.OnClick, OnClickedBtnRank);
            EventDelegate.Remove(btnChallenge.OnClick, OnClickedBtnChallenge);
            EventDelegate.Remove(btnNormalMode.OnClick, OnClickedBtnNormalMode);

            adventureView.OnSelectChapter -= SelectChapter;
            adventureView.OnSelectStage -= OnSelectStage;

            Timing.KillCoroutines(gameObject);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Timing.KillCoroutines(gameObject);
        }

        protected override void OnShow(IUIData data = null)
        {
            eventStageEntryPopupView.Hide();

            isEvent = presenter.GetCurrentEventMode();
            RefreshMode();

            SelectChapter(presenter.GetCurrentChapter());
            UpdateEventStageInfo();
        }

        protected override void OnHide()
        {
            SetEventMode(isEvent: false);
            eventStageEntryPopupView.Hide();
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._48209; // 모험

            btnMap.LocalKey = LocalizeKey._48282; // 지도
            btnMvpReward.LocalKey = LocalizeKey._48208; // MVP 보상
            btnEventMode.LocalKey = LocalizeKey._48210; // 이벤트 모드

            btnGuide.LocalKey = LocalizeKey._48211; // 가이드
            btnRank.LocalKey = LocalizeKey._48212; // 랭킹
            btnChallenge.LocalKey = LocalizeKey._48213; // 챌린지
            btnNormalMode.LocalKey = LocalizeKey._48214; // 일반 모드
        }

        void SelectChapter(int chapter)
        {
            if (curChapter == chapter)
                return;

            curChapter = chapter;

            adventureView.SetChapterData(presenter.GetChapterData(curChapter));
            adventureView.SetStageData(presenter.GetStageData(curChapter));
            presenter.SelectChapter(curChapter);
            MoveToChapter(curChapter);

            string iconName = StringBuilderPool.Get()
                .Append("Ui_Common_MVPReward_").Append(curChapter)
                .Release();
            btnMvpReward.SetIconName(iconName);

            UpdateChallengeNotice();
        }

        void OnSelectStage(int stageId)
        {
            if (!presenter.IsCheckSelectStage(stageId))
                return;

            AsyncStartStage(stageId).WrapUIErrors();
        }

        void OnClickedBtnMap()
        {
            AsyncShowSelectWorldPopup().WrapUIErrors();
        }

        void OnClickedBtnReward()
        {
            UI.Show<UIMvpReward>().SetData(curChapter);
        }

        void OnClickedBtnEventMode()
        {
            SetEventMode(isEvent: true);
        }

        void OnClickedBtnGuide()
        {
            UI.Show<UIAdventureGuide>();
        }

        void OnClickedBtnRank()
        {
            UI.Show<UIAdventureRanking>();
        }

        void OnClickedBtnChallenge()
        {
            if (!presenter.IsCheckSelectChallenge(curChapter))
                return;

            AsyncStartChallenge().WrapUIErrors();
        }

        void OnClickedBtnNormalMode()
        {
            SetEventMode(isEvent: false);
        }

        void UpdateEventStageInfo()
        {
            UpdateEventStageTime();
            UpdateChallengeNotice();
        }

        public void SetEventMode(bool isEvent)
        {
            if (this.isEvent == isEvent)
                return;

            this.isEvent = isEvent;
            RefreshMode();
        }

        public void SetStage(int stageId)
        {
            int chapter = presenter.GetChapter(stageId);
            SelectChapter(chapter);
        }

        private void RefreshMode()
        {
            // BackGround
            uvTexture.Switch(isEvent);

            // Normal Buttons
            btnMap.SetActive(true);
            btnEventMode.SetActive(!isEvent);
            btnMvpReward.SetActive(!isEvent);
            //btnTimePatrol.SetActive(!isEvent && presenter.IsOpenTimePatrol(isShowMessage: false));

            // Event Buttons
            btnNormalMode.SetActive(isEvent);
            btnChallenge.SetActive(isEvent);
            btnRank.SetActive(isEvent);
            btnGuide.SetActive(isEvent);

            gridButton.Reposition();

            // Notice
            tweenNotice.Play(forward: isEvent);

            presenter.SelectEventMode(isEvent);
        }

        /// <summary>
        /// 이벤트스테이지 남은시간 업데이트
        /// </summary>
        private void UpdateEventStageTime()
        {
            Timing.KillCoroutines(gameObject);
            Timing.RunCoroutine(YieldRefreshCloseTime(presenter.GetEventStageRemainTime()), gameObject); // 시즌 종료까지 남은 시간 보여주기
        }

        /// <summary>
        /// 챌린지버튼 알림표시
        /// </summary>
        private void UpdateChallengeNotice()
        {
            btnChallenge.SetNotice(presenter.HasNotice(curChapter));
            btnEventMode.SetNotice(presenter.HasNotice(curChapter));
        }

        /// <summary>
        /// 선택한 챕터로 이동 처리
        /// </summary>
        private void MoveToChapter(int chapter)
        {
            int group = presenter.GetAdventureGroup(chapter);
            int firstChapter = presenter.GetFirstChapter(group);
            adventureView.MoveChapter(chapter - firstChapter);
        }

        /// <summary>
        /// 스테이지 시작
        /// </summary>
        private async Task AsyncStartStage(int stageId)
        {
            if (isEvent)
            {
                string title = LocalizeKey._48225.ToText(); // 이벤트 모드 도전
                string message = LocalizeKey._48227.ToText(); // 이벤트 모드에 입장하시겠습니까?
                int level = presenter.GetEventLevel(stageId);
                int point = presenter.GetEventPoint(stageId);
                string desc = LocalizeKey._48231.ToText() // 최대 {LEVEL}레벨까지 도전할 수 있습니다.
                    .Replace(ReplaceKey.LEVEL, presenter.eventStageMaxLevel);
                RewardData rewardData = presenter.GetEventReward(stageId);
                string confirmText = LocalizeKey._48235.ToText(); // 입장
                EventStageEntryPopupView.SelectResult result = await eventStageEntryPopupView.Show(title, message, level, point, desc, rewardData, confirmText);
                if (result != EventStageEntryPopupView.SelectResult.Confirm)
                    return;
            }

            presenter.StartStage(stageId);
        }

        /// <summary>
        /// 챌린지 시작
        /// </summary>
        private async Task AsyncStartChallenge()
        {
            int stageId = presenter.GetChallengeStageId(curChapter);
            int todayClearCount = presenter.GetChallengeClearCount(stageId);
            string title = LocalizeKey._48226.ToText(); // 챌린지 모드 도전
            string message = LocalizeKey._48228.ToText(); // 챌린지 모드에 입장하시겠습니까?
            int level = presenter.GetEventLevel(stageId);
            int point = presenter.GetEventPoint(stageId);
            string desc = LocalizeKey._48232.ToText() // 도전횟수는 자정(GMT+8)에 초기화 됩니다.
                .Replace(ReplaceKey.LEVEL, presenter.eventStageMaxLevel);
            RewardData rewardData = presenter.GetEventReward(stageId);
            int eventStageFreeEnterCount = presenter.eventStageFreeEnterCount;
            if (todayClearCount < eventStageFreeEnterCount)
            {
                string confirmText;
                if (eventStageFreeEnterCount > 1) // 무료 입장 횟수가 1번 이상일 경우에는 카운트를 포함하여 보여 줌
                {
                    confirmText = StringBuilderPool.Get()
                        .Append(LocalizeKey._48236.ToText()) // 무료 입장
                        .Append("(").Append(eventStageFreeEnterCount - todayClearCount).Append(")")
                        .Release();
                }
                else
                {
                    confirmText = LocalizeKey._48236.ToText(); // 무료 입장
                }

                EventStageEntryPopupView.SelectResult result = await eventStageEntryPopupView.Show(title, message, level, point, desc, rewardData, confirmText);
                if (result != EventStageEntryPopupView.SelectResult.Confirm)
                    return;
            }
            else
            {
                int remainEntryCount = presenter.eventStageClearCountLimit - todayClearCount;
                string itemIcon = presenter.eventTicketItemIcon;
                int itemCount = presenter.GetEventTicketItemCount();
                EventStageEntryPopupView.SelectResult result = await eventStageEntryPopupView.Show(title, message, level, point, desc, rewardData, remainEntryCount, itemIcon, itemCount);
                if (result != EventStageEntryPopupView.SelectResult.Confirm)
                    return;

                // 아이템이 없을 경우에 재료 정보 보여주기
                if (itemCount == 0)
                {
                    UI.ShowConfirmItemPopup(RewardType.Item, presenter.eventStageTicketItemId, 1, LocalizeKey._90262); // 아이템이 부족합니다.
                    return;
                }
            }

            presenter.StartChallenge(curChapter);
        }

        /// <summary>
        /// 월드 선택 팝업
        /// </summary>
        private async Task AsyncShowSelectWorldPopup()
        {
            int groupId = await UI.Show<UIAdventureGroupSelect>().AsyncShow();

            // group 선택하지 않음
            if (groupId <= 0)
                return;

            // group이 바뀌지 않음
            if (groupId == presenter.GetAdventureGroup(curChapter))
                return;

            int chapter = presenter.GetDisplayChapter(groupId);
            if (chapter > presenter.maxChapter)
            {
                UI.ShowToastPopup(LocalizeKey._90045.ToText()); // 업데이트 예정입니다.
                return;
            }

            adventureView.InitChapterProgress(); // Chapter Progress 초기화

            SelectChapter(chapter);
        }

        IEnumerator<float> YieldRefreshCloseTime(RemainTime closeTime)
        {
            while (true)
            {
                float remainTime = closeTime.ToRemainTime();
                if (remainTime <= 0f)
                    break;

                System.TimeSpan timeSpan = remainTime.ToTimeSpan();
                labelNotice.Text = LocalizeKey._48224.ToText() // 이벤트 종료일 까지: {DAYS}일 {HOURS}시간 {MINUTES}분 {SECONDS}초
                    .Replace(ReplaceKey.DAYS, timeSpan.Days)
                    .Replace(ReplaceKey.HOURS, timeSpan.Hours)
                    .Replace(ReplaceKey.MINUTES, timeSpan.Minutes)
                    .Replace(ReplaceKey.SECONDS, timeSpan.Seconds);

                yield return Timing.WaitForSeconds(1f);
            }

            labelNotice.Text = LocalizeKey._48223.ToText(); // 이벤트 기간이 아닙니다.
        }

        protected override void OnBack()
        {
            if (eventStageEntryPopupView.IsShow)
            {
                eventStageEntryPopupView.Hide();
                return;
            }

            base.OnBack();
        }
    }
}