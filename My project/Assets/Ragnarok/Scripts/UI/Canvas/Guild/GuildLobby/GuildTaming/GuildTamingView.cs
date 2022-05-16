using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class GuildTamingView : UIView, IInspectorFinder
    {
        public interface IInput
        {
            RewardData GetFeedItem();
            int GetFeedItemCount();
            MonsterInfo[] GetMonsterInfos();
        }

        private const int MIN_DAY_INDEX = 1;
        private const int MAX_DAY_INDEX = 7;

        private readonly Color COLOR_LEFT_TIME_TITLE_DISABLED = new Color32(160, 160, 160, 255);
        private readonly Color COLOR_LEFT_TIME_TITLE_ENABLED = Color.white;
        private readonly Color COLOR_LEFT_TIME_DISABLED = new Color32(168, 168, 48, 255);
        private readonly Color COLOR_LEFT_TIME_ENABLED = new Color32(255, 255, 135, 255);

        [SerializeField] UILabelHelper labelWeek;
        [SerializeField] UIButtonHelper btnHelp;
        [SerializeField] UILabel labelLeftTimeTitle;
        [SerializeField] UILabel labelLeftTime;

        // 중단
        [SerializeField] UILabelHelper labelFeedTitle;
        [SerializeField] UIRewardHelper feedInfo;
        [SerializeField] UILabelHelper labelFeedItemCount; // 보유량 : {COUNT}

        [SerializeField] UILabelHelper labelMonsterTitle;
        [SerializeField] UIMonsterIcon[] monsterIcon;

        [SerializeField] UIButtonHelper btnLeft;
        [SerializeField] UIButtonHelper btnRight;

        // 하단
        [SerializeField] UIButtonHelper btnEnter;

        public event System.Action OnSelectHelp;
        public event System.Action<int> OnSelectDay;
        public event System.Action OnSelectEnter;
        public event System.Action OnTimeCheck;

        private IInput input;

        private int dayIndex = -1;
        private RemainTime remainTime;
        private bool isInProgress;
        private int todayIndex;
        private bool isOpenTime;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnHelp.OnClick, OnClickedBtnHelp);
            EventDelegate.Add(btnLeft.OnClick, OnClickedBtnLeft);
            EventDelegate.Add(btnRight.OnClick, OnClickedBtnRight);
            EventDelegate.Add(btnEnter.OnClick, OnClickedBtnEnter);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnHelp.OnClick, OnClickedBtnHelp);
            EventDelegate.Remove(btnLeft.OnClick, OnClickedBtnLeft);
            EventDelegate.Remove(btnRight.OnClick, OnClickedBtnRight);
            EventDelegate.Remove(btnEnter.OnClick, OnClickedBtnEnter);
        }

        protected override void OnLocalize()
        {
            labelFeedTitle.LocalKey = LocalizeKey._33110; // 먹이
            labelMonsterTitle.LocalKey = LocalizeKey._33111; // 등장 몬스터
            btnEnter.LocalKey = LocalizeKey._33113; // 입장하기
        }

        void OnClickedBtnHelp()
        {
            OnSelectHelp?.Invoke();
        }

        void OnClickedBtnLeft()
        {
            SetSelectDay(dayIndex - 1);
        }

        void OnClickedBtnRight()
        {
            SetSelectDay(dayIndex + 1);
        }

        void OnClickedBtnEnter()
        {
            OnSelectEnter?.Invoke();
        }

        public void SetSelectDay(int value)
        {
            int index = Mathf.Clamp(value, MIN_DAY_INDEX, MAX_DAY_INDEX);
            if (dayIndex == index)
                return;

            dayIndex = index;
            labelWeek.Text = GetWeekString(dayIndex);

            RefreshArrowButtonEnable();
            RefreshEnterButtonEnable();

            OnSelectDay?.Invoke(dayIndex);
        }

        public void SetData(IInput input)
        {
            this.input = input;
            Refresh();
        }

        public void UpdateData(int todayIndex, bool isInProgress, RemainTime remainTime)
        {
            this.remainTime = remainTime;
            this.isInProgress = isInProgress;
            this.todayIndex = todayIndex;

            UpdateLeftTimeTitle();
            Timing.RunCoroutineSingleton(YieldRefreshTime().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private void Refresh()
        {
            if (input == null)
                return;

            feedInfo.SetData(input.GetFeedItem());
            labelFeedItemCount.Text = LocalizeKey._33116.ToText() // 보유량 : {COUNT}
                .Replace(ReplaceKey.COUNT, input.GetFeedItemCount());

            MonsterInfo[] monsterInfos = input.GetMonsterInfos();
            for (int i = 0; i < monsterIcon.Length; i++)
            {
                monsterIcon[i].SetData(i < monsterInfos.Length ? monsterInfos[i] : null);
            }
        }

        /// <summary>
        /// 요일 반환
        /// </summary>
        private string GetWeekString(int day)
        {
            switch (day)
            {
                case 1: return LocalizeKey._33102.ToText(); // 월요일
                case 2: return LocalizeKey._33103.ToText(); // 화요일
                case 3: return LocalizeKey._33104.ToText(); // 수요일
                case 4: return LocalizeKey._33105.ToText(); // 목요일
                case 5: return LocalizeKey._33106.ToText(); // 금요일
                case 6: return LocalizeKey._33107.ToText(); // 토요일
                case 7: return LocalizeKey._33108.ToText(); // 일요일
            }

            Debug.LogError($"지정되지 않은 요일Index = {day}");
            return string.Empty;
        }

        private IEnumerator<float> YieldRefreshTime()
        {
            while (true)
            {
                float leftTime = remainTime.ToRemainTime();
                if (leftTime <= 0)
                    break;

                UpdateTimeText(leftTime);
                yield return Timing.WaitForSeconds(1f);
            }

            UpdateTimeText(0f);
            OnTimeCheck?.Invoke();
        }

        private void UpdateLeftTimeTitle()
        {
            if (isInProgress)
            {
                labelLeftTimeTitle.text = LocalizeKey._33117.ToText(); // 남은 시간
                labelLeftTimeTitle.color = COLOR_LEFT_TIME_TITLE_ENABLED;
                labelLeftTime.color = COLOR_LEFT_TIME_ENABLED;
            }
            else
            {
                labelLeftTimeTitle.text = LocalizeKey._33109.ToText(); // 남은 입장 시간
                labelLeftTimeTitle.color = COLOR_LEFT_TIME_TITLE_DISABLED;
                labelLeftTime.color = COLOR_LEFT_TIME_DISABLED;
            }
        }

        private void UpdateTimeText(float leftTime)
        {
            isOpenTime = leftTime > 0f;
            labelLeftTime.text = leftTime.ToStringTime();
            RefreshEnterButtonEnable();
        }

        private void RefreshArrowButtonEnable()
        {
            btnLeft.IsEnabled = dayIndex > MIN_DAY_INDEX;
            btnRight.IsEnabled = dayIndex < MAX_DAY_INDEX;
        }

        private void RefreshEnterButtonEnable()
        {
            btnEnter.IsEnabled = isInProgress && dayIndex == todayIndex && isOpenTime;
        }

        bool IInspectorFinder.Find()
        {
            monsterIcon = GetComponentsInChildren<UIMonsterIcon>();
            return true;
        }
    }
}