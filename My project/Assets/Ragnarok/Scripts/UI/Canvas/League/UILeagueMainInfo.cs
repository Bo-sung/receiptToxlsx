using System;
using UnityEngine;

namespace Ragnarok.View.League
{
    /// <summary>
    /// <see cref="LeagueModelView"/>
    /// </summary>
    public class UILeagueMainInfo : UIView, IAutoInspectorFinder
    {
        public interface IInput : UILeagueMyRank.IInput
        {
            int RankSize { get; }
            RemainTime SeasonCloseTime { get; }
            RemainTime SeasonOpenTime { get; }
            DateTime RankUpdateTime { get; }
            int TicketCount { get; }
            int FreeCount { get; }
            int FreeMaxCount { get; }
            UILeagueRankBar.IInput[] RankArray();
        }

        [SerializeField] UILeagueMyRank myRank;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UILeagueRankBar element;
        [SerializeField] UILabelHelper labelNoData;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UILabelValue ticketCount, freeCount;
        [SerializeField] UIButtonHelper btnEnter;
        [SerializeField] UIButtonHelper btnAgent;

        private SuperWrapContent<UILeagueRankBar, UILeagueRankBar.IInput> wrapContent;

        private IInput input;

        public event Action OnSelectEntry;
        public event Action OnSelectAgent;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UILeagueRankBar, UILeagueRankBar.IInput>(element);

            EventDelegate.Add(btnEnter.OnClick, OnClickedBtnEnter);
            EventDelegate.Add(btnAgent.OnClick, OnClickedBtnAgent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnEnter.OnClick, OnClickedBtnEnter);
            EventDelegate.Remove(btnAgent.OnClick, OnClickedBtnAgent);
        }

        protected override void OnLocalize()
        {
            ticketCount.TitleKey = LocalizeKey._47011; // 입장권
            freeCount.TitleKey = LocalizeKey._47014; // 무료 입장
            btnEnter.LocalKey = LocalizeKey._47013; // 대전 시작
            btnAgent.LocalKey = LocalizeKey._47032; // 동료 장착
        }       

        void OnClickedBtnEnter()
        {
            OnSelectEntry?.Invoke();
        }

        void OnClickedBtnAgent()
        {
            OnSelectAgent?.Invoke();
        }

        public void SetActiveAgentNotice(bool isNotice)
        {
            btnAgent.SetNotice(isNotice);
        }

        public void SetData(IInput input)
        {
            this.input = input;
            Refresh();
        }

        private void Refresh()
        {
            myRank.SetData(input);
            wrapContent.SetData(input.RankArray());

            bool isInClacScore = false;
            if (input.RankSize == 0)
            {
                labelNoData.SetActive(true);

                if (input.Ranking == LeaguePresenter.IN_CLAC_SCORE)
                {
                    isInClacScore = true;
                    labelNoData.Text = LocalizeKey._47010.ToText(); // 랭킹 집계중입니다.
                }
                else
                {
                    labelNoData.Text = LocalizeKey._47009.ToText(); // 랭킹 정보가 없습니다.
                }
            }
            else
            {
                labelNoData.SetActive(false);
            }

            if (isInClacScore)
            {
                labelDescription.Text = LocalizeKey._47010.ToText(); // 랭킹 집계중입니다.
            }
            else
            {
                DateTime rankUpdateTime = input.RankUpdateTime;
                labelDescription.Text = LocalizeKey._47029.ToText() // {MONTHS}월 {DAYS}일 {HOURS}시 {MINUTES}분에 집계된 랭킹 정보입니다.
                    .Replace(ReplaceKey.MONTHS, rankUpdateTime.Month)
                    .Replace(ReplaceKey.DAYS, rankUpdateTime.Day)
                    .Replace(ReplaceKey.HOURS, rankUpdateTime.Hour)
                    .Replace(ReplaceKey.MINUTES, rankUpdateTime.Minute);
            }

            ticketCount.Value = input.TicketCount.ToString();
            int leaguefreeCount = input.FreeCount;
            freeCount.Value = StringBuilderPool.Get()
                .Append(leaguefreeCount).Append("/").Append(input.FreeMaxCount)
                .Release();
        }
    }
}