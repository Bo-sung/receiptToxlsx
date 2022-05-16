using UnityEngine;

namespace Ragnarok.View.League
{
    public class LeagueModelView : UIView, IAutoInspectorFinder
    {
        public enum ViewType
        {
            Main,
            GradeReward,
            Rank,
            RankReward,
        }

        [SerializeField] UIToggleHelper toggleMain, toggleGradeReward, toggleRank, toggleRankReward;
        [SerializeField] UILeagueMainInfo leagueMainInfo;
        [SerializeField] UILeagueGradeRewardInfo leagueGradeRewardInfo;
        [SerializeField] UILeagueRankInfo leagueRankInfo;
        [SerializeField] UILeagueRankRewardInfo leagueRankRewardInfo;

        public event System.Action<ViewType> OnSelect;
        public event System.Action OnSelectEntry;
        public event System.Action OnSelectAgent;
        public event System.Action<int> OnSelectRankTab;
        public event System.Action OnShowNextRankingPage;
        public event System.Action<(int uid, int cide)> OnSelectProfile;
        public event System.Action<int> OnSelectRankRewardTab;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(toggleMain.OnChange, OnChangedToggleMain);
            EventDelegate.Add(toggleGradeReward.OnChange, OnChangedToggleGradeReward);
            EventDelegate.Add(toggleRank.OnChange, OnChangedToggleRank);
            EventDelegate.Add(toggleRankReward.OnChange, OnChangedToggleRankReward);

            leagueMainInfo.OnSelectEntry += InvokeOnSelectEntry;
            leagueMainInfo.OnSelectAgent += InvokeOnSelectAgent;
            leagueRankInfo.OnSelectTab += InvokeOnSelectRankTab;
            leagueRankInfo.OnShowNextPage += InvokeOnShowNextPage;
            leagueRankInfo.OnSelectProfile += InvokeOnSelectProfile;
            leagueRankRewardInfo.OnSelectTab += InvokeOnSelectRankRewardTab;

            toggleMain.SetNotice(false);
            toggleRank.SetNotice(false);
            toggleGradeReward.SetNotice(false);
            toggleRankReward.SetNotice(false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(toggleMain.OnChange, OnChangedToggleMain);
            EventDelegate.Remove(toggleGradeReward.OnChange, OnChangedToggleGradeReward);
            EventDelegate.Remove(toggleRank.OnChange, OnChangedToggleRank);
            EventDelegate.Remove(toggleRankReward.OnChange, OnChangedToggleRankReward);

            leagueMainInfo.OnSelectEntry -= InvokeOnSelectEntry;
            leagueMainInfo.OnSelectAgent -= InvokeOnSelectAgent;
            leagueRankInfo.OnSelectTab -= InvokeOnSelectRankTab;
            leagueRankInfo.OnShowNextPage -= InvokeOnShowNextPage;
            leagueRankInfo.OnSelectProfile -= InvokeOnSelectProfile;
            leagueRankRewardInfo.OnSelectTab -= InvokeOnSelectRankRewardTab;
        }

        public override void Show()
        {
            base.Show();

            toggleMain.Set(true); // 시작 토글 세팅
        }

        public override void Hide()
        {
            base.Hide();

            toggleMain.Set(false); // 시작 토글 세팅
        }

        protected override void OnLocalize()
        {
            toggleMain.LocalKey = LocalizeKey._47003; // 메인
            toggleGradeReward.LocalKey = LocalizeKey._47005; // 등급 보상
            toggleRank.LocalKey = LocalizeKey._47004; // 랭킹
            toggleRankReward.LocalKey = LocalizeKey._47006; // 순위 보상
        }

        void OnChangedToggleMain()
        {
            SelectToggle(ViewType.Main);
        }

        void OnChangedToggleGradeReward()
        {
            SelectToggle(ViewType.GradeReward);
        }

        void OnChangedToggleRank()
        {
            SelectToggle(ViewType.Rank);
        }        

        void OnChangedToggleRankReward()
        {
            SelectToggle(ViewType.RankReward);
        }

        void InvokeOnSelectEntry()
        {
            OnSelectEntry?.Invoke();
        }

        void InvokeOnSelectAgent()
        {
            OnSelectAgent?.Invoke();
        }

        void InvokeOnSelectRankTab(int index)
        {
            OnSelectRankTab?.Invoke(index);
        }

        void InvokeOnShowNextPage()
        {
            OnShowNextRankingPage?.Invoke();
        }

        void InvokeOnSelectProfile((int uid, int cid) info)
        {
            OnSelectProfile?.Invoke(info);
        }

        void InvokeOnSelectRankRewardTab(int index)
        {
            OnSelectRankRewardTab?.Invoke(index);
        }

        public void SetData(UILeagueMainInfo.IInput input)
        {
            leagueMainInfo.SetData(input);
        }

        public void SetData(UILeagueRankInfo.IInput input)
        {
            leagueRankInfo.SetData(input);
        }

        public void SetRewardInfo(UILeagueGradeRewardBar.IInput[] gradeRewardInfos)
        {
            leagueGradeRewardInfo.SetData(gradeRewardInfos);
        }

        public void SetRankRewardInfo(UILeagueRankRewardBar.IInput[] rankRewardInfo)
        {
            leagueRankRewardInfo.SetData(rankRewardInfo);
        }

        public void SetActiveAgentNotice(bool isNotice)
        {
            leagueMainInfo.SetActiveAgentNotice(isNotice);
        }

        private void SelectToggle(ViewType viewType)
        {
            if (!UIToggle.current.value)
                return;

            leagueMainInfo.SetActive(viewType == ViewType.Main);
            leagueGradeRewardInfo.SetActive(viewType == ViewType.GradeReward);
            leagueRankInfo.SetActive(viewType == ViewType.Rank);
            leagueRankRewardInfo.SetActive(viewType == ViewType.RankReward);

            OnSelect?.Invoke(viewType);
        }
    }
}