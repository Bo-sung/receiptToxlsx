using MEC;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UINabihoSelectBar : UIView
    {
        public interface IInput
        {
            int Id { get; }
            RewardData Reward { get; }
            RemainTime RemainTime { get; }
            RemainTime AdCooldownTime { get; }
            int AdRemainCount { get; }
            int AdMaxCount { get; }
        }

        [Header("Enter")]
        [SerializeField] UIButtonHelper btnEnter;
        [SerializeField] GameObject goLock;
        [SerializeField] UILabelHelper labelLock;

        [Header("InProgress")]
        [SerializeField] GameObject inProgress;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UILabelHelper labelRewardName;
        [SerializeField] UILabelValue remainTime;
        [SerializeField] UIButtonWithIconValue btnShowAd;
        [SerializeField] GameObject goCooldown;
        [SerializeField] UILabelHelper labelCooldown;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnReward;

        public int LocalKey
        {
            set => btnEnter.LocalKey = value;
        }

        private int needLevel;
        private IInput input;

        public event System.Action OnSelectEnter;
        public event System.Action<int> OnSelectShowAd;
        public event System.Action<int> OnSelectCancel;
        public event System.Action<int> OnSelectReward;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnEnter.OnClick, OnClickedBtnEnter);
            EventDelegate.Add(btnShowAd.OnClick, OnClickedBtnShowAd);
            EventDelegate.Add(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Add(btnReward.OnClick, OnClickedBtnReward);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnEnter.OnClick, OnClickedBtnEnter);
            EventDelegate.Remove(btnShowAd.OnClick, OnClickedBtnShowAd);
            EventDelegate.Remove(btnCancel.OnClick, OnClickedBtnCancel);
            EventDelegate.Remove(btnReward.OnClick, OnClickedBtnReward);

            Timing.KillCoroutines(nameof(YieldRefreshRemainTime));
            Timing.KillCoroutines(nameof(YieldRefreshAdCooldown));
        }

        void OnClickedBtnEnter()
        {
            if (IsInvalid())
                return;

            OnSelectEnter?.Invoke();
        }

        void OnClickedBtnShowAd()
        {
            OnSelectShowAd?.Invoke(input.Id);
        }

        void OnClickedBtnCancel()
        {
            ShowCancelPopup().WrapUIErrors();
        }

        void OnClickedBtnReward()
        {
            OnSelectReward?.Invoke(input.Id);
        }

        protected override void OnLocalize()
        {
            remainTime.TitleKey = LocalizeKey._10909; // 완료까지 남은 시간
            btnShowAd.LocalKey = LocalizeKey._10910; // 시간 단축
            btnCancel.LocalKey = LocalizeKey._10911; // 의뢰 취소
            btnReward.LocalKey = LocalizeKey._10913; // 보상 받기

            UpdateLockText();
        }

        public void Initialize(int needLevel)
        {
            this.needLevel = needLevel;
            UpdateLockText();
        }

        public void RefreshLevel(int currentLevel)
        {
            SetLock(currentLevel < needLevel || needLevel == -1);
        }

        public void SetData(IInput input)
        {
            this.input = input;

            if (this.input == null)
            {
                SetActive(false);
                return;
            }

            SetActive(true);
            Refresh();
        }

        private void Refresh()
        {
            Timing.KillCoroutines(gameObject, nameof(YieldRefreshRemainTime));
            Timing.KillCoroutines(gameObject, nameof(YieldRefreshAdCooldown));

            bool isEmptyReward = input.Id == 0;
            btnEnter.SetActive(isEmptyReward);
            NGUITools.SetActive(inProgress, !isEmptyReward);

            if (isEmptyReward)
                return;

            goLock.SetActive(false);
            rewardHelper.SetData(input.Reward);
            labelRewardName.Text = input.Reward.ItemName;
            string adCountText = StringBuilderPool.Get()
                .Append(input.AdRemainCount).Append('/').Append(input.AdMaxCount)
                .Release();
            btnShowAd.SetLabelValue(adCountText);

            Timing.RunCoroutine(YieldRefreshRemainTime(input.RemainTime), gameObject, nameof(YieldRefreshRemainTime));
            Timing.RunCoroutine(YieldRefreshAdCooldown(input.AdCooldownTime), gameObject, nameof(YieldRefreshAdCooldown));
        }

        private void SetLock(bool isLock)
        {
            NGUITools.SetActive(goLock, isLock);
            btnEnter.SetNotice(!isLock);
        }

        private bool IsInvalid()
        {
            return needLevel < 0;
        }

        private void UpdateLockText()
        {
            if (needLevel == -1)
            {
                labelLock.LocalKey = LocalizeKey._10925; // 이벤트 기간 중이 아닙니다.\n더 이상 의뢰 요청을 진행할 수 없습니다.
            }
            else
            {
                labelLock.Text = LocalizeKey._10908.ToText() // 친밀도 {VALUE}단계 필요
                    .Replace(ReplaceKey.VALUE, needLevel);
            }
        }

        private IEnumerator<float> YieldRefreshRemainTime(RemainTime inputRemainTime)
        {
            btnShowAd.SetActive(true);
            btnCancel.SetActive(true);
            btnReward.SetActive(false);

            while (true)
            {
                float time = inputRemainTime.ToRemainTime();
                if (time <= 0f)
                    break;

                System.TimeSpan timeSpan = time.ToTimeSpan();
                if (timeSpan.Days > 0)
                {
                    remainTime.Value = LocalizeKey._47844.ToText() // {DAYS}일 {HOURS}:{MINUTES}:{SECONDS}
                        .Replace(ReplaceKey.DAYS, timeSpan.Days.ToString())
                        .Replace(ReplaceKey.HOURS, timeSpan.Hours.ToString("00"))
                        .Replace(ReplaceKey.MINUTES, timeSpan.Minutes.ToString("00"))
                        .Replace(ReplaceKey.SECONDS, timeSpan.Seconds.ToString("00"));
                }
                else
                {
                    remainTime.Value = timeSpan.ToString(@"hh\:mm\:ss");
                }

                yield return Timing.WaitForSeconds(1f);
            }

            remainTime.Value = LocalizeKey._10914.ToText(); // 완료
            btnShowAd.SetActive(false);
            btnCancel.SetActive(false);
            btnReward.SetActive(true);
        }

        private IEnumerator<float> YieldRefreshAdCooldown(RemainTime inputRemainTime)
        {
            btnShowAd.IsEnabled = false;
            btnShowAd.SetNotice(false);
            goCooldown.SetActive(true);

            while (true)
            {
                float time = inputRemainTime.ToRemainTime();
                if (time <= 0f)
                    break;

                System.TimeSpan timeSpan = time.ToTimeSpan();

                // 분, 초만 보여주기
                int totalMinuts = (int)timeSpan.TotalMinutes;
                labelCooldown.Text = StringBuilderPool.Get()
                    .Append(totalMinuts.ToString("00"))
                    .Append(':')
                    .Append(timeSpan.Seconds.ToString("00"))
                    .Release();

                yield return Timing.WaitForSeconds(1f);
            }

            bool canShowAd = input.AdRemainCount > 0;
            btnShowAd.IsEnabled = canShowAd;
            btnShowAd.SetNotice(canShowAd);
            goCooldown.SetActive(false);
        }

        private async Task ShowCancelPopup()
        {
            string message = LocalizeKey._10912.ToText(); // 의뢰를 취소하시겠습니까?\n\n진행 중인 정보는 초기화됩니다.
            if (!await UI.SelectPopup(message))
                return;

            // 완료 체크
            float time = input.RemainTime.ToRemainTime();
            bool isFinished = time <= 0f;
            if (isFinished)
                return;

            OnSelectCancel?.Invoke(input.Id);
        }
    }
}