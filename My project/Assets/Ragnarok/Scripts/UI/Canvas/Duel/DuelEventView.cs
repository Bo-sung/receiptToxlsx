using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class DuelEventView : UIView
    {
        private enum State
        {
            InProgress = 1,
            Calculate,
            Finished,
        }

        [SerializeField] UILabelHelper labelEventTitle;
        [SerializeField] UIDuelServer myServer;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UIRewardHelper[] rewards;
        [SerializeField] UILabel[] labelServerRewards;
        [SerializeField] GameObject[] completeMarks;
        [SerializeField] UILabelValue winCount;
        [SerializeField] UIButtonWithEffect btnReward;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UILabelValue remainDate;
        [SerializeField] UIButtonHelper btnGift, btnRank;
        [SerializeField] UITextureHelper iconProfile;
        [SerializeField] UITextureHelper jobIcon;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelValue myRank;
        [SerializeField] UIGrid rewardGrid;
        [SerializeField] UIRewardHelper[] rankRewards;
        [SerializeField] GameObject goServerInfo, goCalculateInfo, goResultInfo;
        [SerializeField] UILabelHelper labelCalculateNotice;
        [SerializeField] UIDuelBuffReward perfect, normal;
        [SerializeField] UIButtonHelper btnFacebook;

        // <!-- Initialize --!>
        private int[] eventConditionValues;

        // <!-- Temp --!>
        private DuelServerPacket[] packets;
        private int seasonSeq;
        private RemainTime remainTime;
        private RemainTime nextRemainTime;
        private int myRankValue;
        private int winCountValue;
        private RewardData[] serverRewards;
        private int rewardStep;
        private bool isFinished;
        private bool isNeedRefresh;
        private bool isInitialize;

        public event System.Action<DuelServerPacket> OnSelect;
        public event System.Action<int> OnReward;
        public event System.Action OnRefresh;
        public event System.Action OnSelectGift, OnSelectRanking;

        protected override void Awake()
        {
            base.Awake();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            EventDelegate.Add(btnReward.OnClick, OnClickedBtnReward);
            EventDelegate.Add(btnGift.OnClick, OnClickedBtnGift);
            EventDelegate.Add(btnRank.OnClick, OnClickedBtnRank);
            EventDelegate.Add(btnFacebook.OnClick, OnClickedBtnFacebook);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnReward.OnClick, OnClickedBtnReward);
            EventDelegate.Remove(btnGift.OnClick, OnClickedBtnGift);
            EventDelegate.Remove(btnRank.OnClick, OnClickedBtnRank);
            EventDelegate.Remove(btnFacebook.OnClick, OnClickedBtnFacebook);
        }

        void OnItemRefresh(GameObject go, int index)
        {
            UIDuelServer ui = go.GetComponent<UIDuelServer>();
            ui.SetData(packets[index + 1], OnSelectServer); // 첫번째 내 서버 제외
        }

        void OnClickedBtnReward()
        {
            OnReward?.Invoke(rewardStep);
        }

        void OnClickedBtnGift()
        {
            OnSelectGift?.Invoke();
        }

        void OnClickedBtnRank()
        {
            OnSelectRanking?.Invoke();
        }

        void OnClickedBtnFacebook()
        {
            // 상세보상
            BasisUrl baseUrl = GameServerConfig.IsKorea() ? BasisUrl.KoreanEventDuelReward : BasisUrl.EventDuelReward;
            baseUrl.OpenUrl();
        }

        protected override void OnLocalize()
        {
            winCount.TitleKey = LocalizeKey._47840; // 승리 횟수
            btnReward.LocalKey = LocalizeKey._47841; // 보상 받기
            labelDescription.LocalKey = LocalizeKey._47842; // 듀얼을 통해, 다른 서버의 듀얼 조각을 획득할 수 있습니다.\n보유 듀얼 조각이 없는 서버는 탈락하게 됩니다.

            btnGift.LocalKey = LocalizeKey._47845; // 보상
            btnRank.LocalKey = LocalizeKey._47846; // 순위
            btnFacebook.LocalKey = LocalizeKey._47865; // 상세 보상 확인
            myRank.TitleKey = LocalizeKey._47847; // 나의 순위
            labelCalculateNotice.LocalKey = LocalizeKey._47861; // 랭킹 정산 중입니다.

            for (int i = 0; i < labelServerRewards.Length; i++)
            {
                labelServerRewards[i].text = LocalizeKey._47834.ToText().Replace(ReplaceKey.VALUE, i + 1);
            }

            UpdateLocalize();
        }

        public override void Show()
        {
            base.Show();

            Refresh();
            SetState(default);
        }

        public override void Hide()
        {
            base.Hide();

            isNeedRefresh = false;
        }

        public void Initialize(RewardData[] eventRewards, int[] eventConditionValues)
        {
            if (isInitialize)
                return;

            isInitialize = true;
            int dataCount = eventRewards == null ? 0 : eventRewards.Length;
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(i < dataCount ? eventRewards[i] : null);
            }

            this.eventConditionValues = eventConditionValues;
        }

        public void SetCharacterData(Job job, Gender gender, string profileName, string name, string hexCid)
        {
            const string NAME_FORMAT = "[5A575B]{NAME}[-] [BEBEBE]({VALUE})[-]";

            iconProfile.Set(profileName);
            jobIcon.Set(job.GetJobIcon());
            labelName.Text = NAME_FORMAT
                .Replace(ReplaceKey.NAME, name)
                .Replace(ReplaceKey.VALUE, hexCid);
        }

        public void SetData(int seasonSeq, long remainTime, long nextRemainTime, int myRankValue, int winCountValue, RewardData[] serverRewards)
        {
            this.seasonSeq = seasonSeq;
            this.remainTime = remainTime;
            this.nextRemainTime = nextRemainTime;
            this.myRankValue = myRankValue;
            this.winCountValue = winCountValue;
            this.serverRewards = serverRewards;

            // 다음 행동 남은 시간이 존재할 경우에만 처리
            // nextRemainTime 이 0으로 왔을 경우에 서버에 무한 요청 방지
            isNeedRefresh = nextRemainTime > 0;

            Refresh();
            UpdateLocalize();
        }

        public void SetRewardStep(int rewardStep)
        {
            this.rewardStep = rewardStep;
            RefreshRewardStep();
        }

        public void ShowServers(DuelServerPacket[] packets)
        {
            this.packets = packets;

            SetState(State.InProgress);
            RefreshServer();
        }

        public void ShowCalculateNotice()
        {
            SetState(State.Calculate);
        }

        public void ShowRank(UIDuelBuffReward.IInput perfectRank, UIDuelBuffReward.IInput normalRank)
        {
            SetState(State.Finished);
            perfect.SetData(perfectRank, isResult: true);
            normal.SetData(normalRank, isResult: true);
        }

        private void SetState(State state)
        {
            NGUITools.SetActive(goServerInfo, state == State.InProgress);
            NGUITools.SetActive(goCalculateInfo, state == State.Calculate);
            NGUITools.SetActive(goResultInfo, state == State.Finished);
        }

        private void Refresh()
        {
            int serverRewardCount = serverRewards == null ? 0 : serverRewards.Length;
            for (int i = 0; i < rankRewards.Length; i++)
            {
                rankRewards[i].SetData(i < serverRewardCount ? serverRewards[i] : null);
            }
            rewardGrid.Reposition();
            RefreshRewardStep();

            Timing.RunCoroutineSingleton(YieldUpdateRemainTime().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private void RefreshServer()
        {
            int length = packets == null ? 0 : packets.Length;
            if (length == 0)
            {
                myServer.SetData(null, null);
                wrapper.Resize(0);
            }
            else
            {
                myServer.SetData(packets[0], null); // 첫번째 패킷은 내 서버
                wrapper.Resize(length - 1); // 내 서버 뺀 것 만큼
            }
        }

        private void RefreshRewardStep()
        {
            winCount.Value = GetWinCount();
            btnReward.IsEnabled = CanRecieveReward(); // 보상 여부
            for (int i = 0; i < completeMarks.Length; i++)
            {
                NGUITools.SetActive(completeMarks[i], i < rewardStep);
            }
        }

        private void OnSelectServer(DuelServerPacket packet)
        {
            if (isFinished)
            {
                string notice = LocalizeKey._47851.ToText(); // 이벤트가 종료되었습니다.
                UI.ShowToastPopup(notice);
                return;
            }

            if (!myServer.HaveDuelCount())
            {
                string notice = LocalizeKey._47864.ToText(); // 탈락한 서버는 다른 서버를 공격 할 수 없습니다.
                UI.ShowToastPopup(notice);
                return;
            }

            OnSelect?.Invoke(packet);
        }

        private void UpdateLocalize()
        {
            labelEventTitle.Text = LocalizeKey._47838.ToText() // 제 {INDEX} 회 최강 서버 결정전
                .Replace(ReplaceKey.INDEX, seasonSeq);

            if (myRankValue > 0)
            {
                myRank.Value = LocalizeKey._47848.ToText() // {RANK}위
                    .Replace(ReplaceKey.RANK, myRankValue);
            }
            else
            {
                myRank.Value = "-";
            }
        }

        private string GetWinCount()
        {
            int conditionClunt = eventConditionValues == null ? 0 : eventConditionValues.Length;
            if (rewardStep >= 0 && rewardStep < conditionClunt)
                return string.Concat(winCountValue, "/", eventConditionValues[rewardStep]);

            return winCountValue.ToString();
        }

        private bool CanRecieveReward()
        {
            // 이긴 횟수가 없으면 무조건 false
            if (winCountValue == 0)
                return false;

            int conditionClunt = eventConditionValues == null ? 0 : eventConditionValues.Length;
            if (rewardStep >= 0 && rewardStep < conditionClunt)
                return winCountValue >= eventConditionValues[rewardStep];

            return false;
        }

        private bool UpdateRemainTime(float remainMilliseconds)
        {
            if (remainMilliseconds <= 0f)
                return false;

            var timeSpan = remainMilliseconds.ToTimeSpan();
            remainDate.Value = LocalizeKey._47844.ToText() // {DAYS}일 {HOURS}:{MINUTES}:{SECONDS}
                .Replace(ReplaceKey.DAYS, timeSpan.Days.ToString("00"))
                .Replace(ReplaceKey.HOURS, timeSpan.Hours.ToString("00"))
                .Replace(ReplaceKey.MINUTES, timeSpan.Minutes.ToString("00"))
                .Replace(ReplaceKey.SECONDS, timeSpan.Seconds.ToString("00"));
            return true;
        }

        IEnumerator<float> YieldUpdateRemainTime()
        {
            isFinished = false;
            remainDate.TitleKey = LocalizeKey._47843; // 종료일까지 남은 시간
            while (UpdateRemainTime(remainTime.ToRemainTime()))
            {
                yield return Timing.WaitForSeconds(1f);
            }

            isFinished = true;
            remainDate.TitleKey = LocalizeKey._47850; // 시작일까지 남은 시간
            while (UpdateRemainTime(nextRemainTime.ToRemainTime()))
            {
                yield return Timing.WaitForSeconds(1f);
            }

            if (isNeedRefresh)
            {
                isNeedRefresh = false;
                OnRefresh?.Invoke(); // Refresh 필요
            }
        }
    }
}