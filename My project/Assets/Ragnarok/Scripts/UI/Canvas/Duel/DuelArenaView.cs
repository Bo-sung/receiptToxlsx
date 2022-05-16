using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public sealed class DuelArenaView : UIView, IInspectorFinder
    {
        [SerializeField] UIDuelArenaInfo curArena, nextArena;
        [SerializeField] GameObject goArena;
        [SerializeField] UILabelValue ownFlagCount;
        [SerializeField] UIButtonHelper btnEntry;
        [SerializeField] UICostButtonHelper btnBuyFlag;
        [SerializeField] GameObject goCalculateInfo;
        [SerializeField] UILabelHelper labelCalculateNotice;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIRewardHelper[] rewards;
        [SerializeField] UILabel[] labelServerRewards;
        [SerializeField] GameObject[] completeMarks;
        [SerializeField] UILabelValue winCount;
        [SerializeField] UIButtonWithEffect btnReward;
        [SerializeField] UILabelValue remainDate;
        [SerializeField] UIButtonHelper btnRank;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIDuelArenaHistoryElement element;
        [SerializeField] UILabelHelper labelNoData;
        [SerializeField] UILabelValue openRemainDate;

        private SuperWrapContent<UIDuelArenaHistoryElement, UIDuelArenaHistoryElement.IInput> wrapContent;

        // <!-- Initialize --!>
        private int[] conditionValues;

        // <!-- Temp --!>
        private RemainTime remainTime;
        private int rewardStep;
        private bool isFinished;
        private bool isNeedRefresh;
        private bool isInitialize;
        private int arenaPoint;

        public event UserModel.UserInfoEvent OnSelectUserInfo;
        public event System.Action OnSelectEntry;
        public event System.Action OnSelectBuyFlag;
        public event System.Action<int> OnReward;
        public event System.Action OnSelectRanking;
        public event System.Action OnRefresh;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnEntry.OnClick, OnClickedBtnEntry);
            EventDelegate.Add(btnBuyFlag.OnClick, OnClickedBtnBuyFlag);
            EventDelegate.Add(btnReward.OnClick, OnClickedBtnReward);
            EventDelegate.Add(btnRank.OnClick, OnClickedBtnRank);

            wrapContent = wrapper.Initialize<UIDuelArenaHistoryElement, UIDuelArenaHistoryElement.IInput>(element);
            foreach (var item in wrapContent)
            {
                item.OnSelectUserInfo += OnUserInfo;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnEntry.OnClick, OnClickedBtnEntry);
            EventDelegate.Remove(btnBuyFlag.OnClick, OnClickedBtnBuyFlag);
            EventDelegate.Remove(btnReward.OnClick, OnClickedBtnReward);
            EventDelegate.Remove(btnRank.OnClick, OnClickedBtnRank);

            foreach (var item in wrapContent)
            {
                item.OnSelectUserInfo -= OnUserInfo;
            }
        }

        void OnClickedBtnEntry()
        {
            if (isFinished)
                return;

            OnSelectEntry?.Invoke();
        }

        void OnClickedBtnBuyFlag()
        {
            if (isFinished)
                return;

            OnSelectBuyFlag?.Invoke();
        }

        void OnClickedBtnReward()
        {
            OnReward?.Invoke(rewardStep);
        }

        void OnClickedBtnRank()
        {
            OnSelectRanking?.Invoke();
        }

        void OnUserInfo(int uid, int cid)
        {
            OnSelectUserInfo?.Invoke(uid, cid);
        }

        protected override void OnLocalize()
        {
            curArena.SetTitleKey(LocalizeKey._47870); // 현재 아레나
            nextArena.SetTitleKey(LocalizeKey._47871); // 다음 아레나
            ownFlagCount.TitleKey = LocalizeKey._47867; // 보유 아레나 깃발
            btnEntry.LocalKey = LocalizeKey._47868; // 입장
            btnBuyFlag.LocalKey = LocalizeKey._47869; // 아레나 깃발 구매
            labelDescription.LocalKey = LocalizeKey._47873; // 하루에 한 번 [C][DA5050]MVP[-][/C}를 처치하여 아레나 깃발을 획득할 수 있습니다.
            winCount.TitleKey = LocalizeKey._47874; // 깃발 개수
            btnReward.LocalKey = LocalizeKey._47875; // 받기
            labelCalculateNotice.LocalKey = LocalizeKey._47876; // 아레나\n준비중
            btnRank.LocalKey = LocalizeKey._47846; // 순위
            remainDate.TitleKey = LocalizeKey._47843; // 종료일까지 남은 시간
            openRemainDate.TitleKey = LocalizeKey._47850; // 시작일까지 남은 시간
            labelNoData.LocalKey = LocalizeKey._47802; // 듀얼 내역이 없습니다.

            for (int i = 0; i < labelServerRewards.Length; i++)
            {
                labelServerRewards[i].text = LocalizeKey._47834.ToText().Replace(ReplaceKey.VALUE, i + 1); // {VALUE}회
            }
        }

        public override void Show()
        {
            base.Show();

            Refresh();
        }

        public override void Hide()
        {
            base.Hide();

            isNeedRefresh = false;
        }

        public void Initialize(RewardData[] arrReward, int[] conditionValues, int arenaFlagCatCoin)
        {
            if (isInitialize)
                return;

            isInitialize = true;
            int dataCount = arrReward == null ? 0 : arrReward.Length;
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(i < dataCount ? arrReward[i] : null);
            }

            this.conditionValues = conditionValues;
            btnBuyFlag.SetCostCount(arenaFlagCatCoin);
        }

        public void SetData(long remainTime, UIDuelArenaHistoryElement.IInput[] inputs)
        {
            this.remainTime = remainTime;
            isFinished = false;
            isNeedRefresh = true;

            int length = inputs == null ? 0 : inputs.Length;
            labelNoData.SetActive(length == 0);
            wrapContent.SetData(inputs);

            Refresh();
        }

        public void ShowCalculateNotice(long remainTime)
        {
            this.remainTime = remainTime;
            isFinished = true;
            isNeedRefresh = true;

            Refresh();
        }

        public void SetArena(int arenaPoint, UIDuelArenaInfo.IInput cur, UIDuelArenaInfo.IInput next)
        {
            this.arenaPoint = arenaPoint;

            // 종료
            if (isFinished)
                return;

            bool hasArenaPoint = this.arenaPoint > 0;
            ownFlagCount.Value = this.arenaPoint.ToString();
            btnEntry.SetActive(hasArenaPoint);
            btnBuyFlag.SetActive(!hasArenaPoint);
            curArena.SetData(cur);
            nextArena.SetData(next);
        }

        public void SetRewardStep(int rewardStep)
        {
            this.rewardStep = rewardStep;
            RefreshRewardStep();
        }

        private void Refresh()
        {
            Timing.RunCoroutineSingleton(YieldUpdateRemainTime().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private void RefreshRewardStep()
        {
            winCount.Value = GetArenaPoint();
            btnReward.IsEnabled = CanRecieveReward(); // 보상 여부
            for (int i = 0; i < completeMarks.Length; i++)
            {
                NGUITools.SetActive(completeMarks[i], i < rewardStep);
            }
        }

        private string GetArenaPoint()
        {
            int conditionClunt = conditionValues == null ? 0 : conditionValues.Length;
            if (rewardStep >= 0 && rewardStep < conditionClunt)
                return string.Concat(arenaPoint, "/", conditionValues[rewardStep]);

            return arenaPoint.ToString();
        }

        private bool CanRecieveReward()
        {
            // 이긴 횟수가 없으면 무조건 false
            if (arenaPoint == 0)
                return false;

            int conditionClunt = conditionValues == null ? 0 : conditionValues.Length;
            if (rewardStep >= 0 && rewardStep < conditionClunt)
                return arenaPoint >= conditionValues[rewardStep];

            return false;
        }

        private bool UpdateRemainTime(float remainMilliseconds)
        {
            bool hasRemainTime = remainMilliseconds > 0f;

            string text;
            if (hasRemainTime)
            {
                var timeSpan = remainMilliseconds.ToTimeSpan();
                text = LocalizeKey._47844.ToText() // {DAYS}일 {HOURS}:{MINUTES}:{SECONDS}
                    .Replace(ReplaceKey.DAYS, timeSpan.Days.ToString())
                    .Replace(ReplaceKey.HOURS, timeSpan.Hours.ToString("00"))
                    .Replace(ReplaceKey.MINUTES, timeSpan.Minutes.ToString("00"))
                    .Replace(ReplaceKey.SECONDS, timeSpan.Seconds.ToString("00"));
            }
            else
            {
                text = LocalizeKey._47844.ToText() // {DAYS}일 {HOURS}:{MINUTES}:{SECONDS}
                    .Replace(ReplaceKey.DAYS, "0")
                    .Replace(ReplaceKey.HOURS, "00")
                    .Replace(ReplaceKey.MINUTES, "00")
                    .Replace(ReplaceKey.SECONDS, "00");
            }

            if (isFinished)
            {
                openRemainDate.Value = text;
            }
            else
            {
                remainDate.Value = text;
            }

            return hasRemainTime;
        }

        IEnumerator<float> YieldUpdateRemainTime()
        {
            UpdateFinished();
            while (UpdateRemainTime(remainTime.ToRemainTime()))
            {
                yield return Timing.WaitForSeconds(1f);
            }

            // 완료되지 않았을 경우에는 클라에서 미리 막음
            if (!isFinished)
            {
                isFinished = true;
                UpdateRemainTime(remainTime.ToRemainTime());
                UpdateFinished();
            }

            if (isNeedRefresh)
            {
                isNeedRefresh = false;
                OnRefresh?.Invoke(); // Refresh 필요
            }
        }

        private void UpdateFinished()
        {
            goArena.SetActive(!isFinished);
            goCalculateInfo.SetActive(isFinished);
        }

        bool IInspectorFinder.Find()
        {
#if UNITY_EDITOR
            Transform child = transform.Find("Arena/Rewards");
            if (child == null)
                return false;

            rewards = child.GetComponentsInChildren<UIRewardHelper>();

            UnityEditor.ArrayUtility.Clear(ref labelServerRewards);
            UnityEditor.ArrayUtility.Clear(ref completeMarks);

            for (int i = 0; i < rewards.Length; i++)
            {
                UnityEditor.ArrayUtility.Add(ref labelServerRewards, rewards[i].transform.Find("Deco/RewardOrderLabel").GetComponent<UILabel>());
                UnityEditor.ArrayUtility.Add(ref completeMarks, rewards[i].transform.Find("Complete").gameObject);
            }
#endif

            return true;
        }
    }
}