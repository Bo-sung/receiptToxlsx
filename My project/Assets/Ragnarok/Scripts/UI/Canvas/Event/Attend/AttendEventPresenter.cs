namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIAttendEvent"/>
    /// </summary>
    public class AttendEventPresenter : ViewPresenter
    {
        private const int ATTEND_EVENT_GROUP_ID = 999; // 출석 체크 이벤트 그룹 ID

        // <!-- Models --!>
        private readonly EventModel eventModel;
        private readonly QuestModel questModel;

        // <!-- Repositories --!>
        private readonly EventLoginBonusDataManager eventLoginBonusDataRepo;

        // <!-- Data --!>
        private BetterList<AttendEventElement> attendEventElements;
        private EventQuestGroupInfo eventQuestGroupInfo;

        // <!-- Event --!>
        public event System.Action OnUpdateAttendEventReward
        {
            add { eventModel.OnUpdateAttendEventReward += value; }
            remove { eventModel.OnUpdateAttendEventReward -= value; }
        }

        public AttendEventPresenter()
        {
            eventModel = Entity.player.Event;
            questModel = Entity.player.Quest;
            eventLoginBonusDataRepo = EventLoginBonusDataManager.Instance;
            Initialize();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
            if (attendEventElements == null)
                return;

            attendEventElements.Clear();
        }

        public void Initialize()
        {
            eventQuestGroupInfo = questModel.GetEventQuestByShortCut(ShortCutType.AttendEvent);
        }

        public bool HasEventData()
        {
            return eventQuestGroupInfo != null && eventQuestGroupInfo.RemainTime.ToRemainTime() > 0;
        }

        public UIBannerElement.IInput GetData()
        {
            return eventQuestGroupInfo;
        }

        /// <summary>
        /// 출석 체크 보상 목록
        /// </summary>
        public UIAttendEventElement.IInput[] GetArrayData()
        {
            if (attendEventElements == null)
            {
                attendEventElements = new BetterList<AttendEventElement>();

                foreach (EventLoginBonusData item in eventLoginBonusDataRepo.Get(ATTEND_EVENT_GROUP_ID).OrEmptyIfNull())
                {
                    AttendEventElement temp = new AttendEventElement(item, CheckState);
                    attendEventElements.Add(temp);
                }
            }
            return attendEventElements.ToArray();
        }

        /// <summary>
        /// 출석 보상 받기 요청
        /// </summary>
        public void RequestAttendEventReward()
        {
            if (!HasEventData()) // 이벤트 종료
            {
                UI.ShowToastPopup(LocalizeKey._90254.ToText()); // 이벤트 기간이 아닙니다.
                return;
            }
            eventModel.RequestAttendEventReward().WrapNetworkErrors();
        }

        /// <summary>
        /// 보상 수령 상태 체크
        /// </summary>
        private UIAttendEventElement.State CheckState(int day)
        {
            // 보상 수령 불가능 (출석 일수 미도달)
            if (day > eventModel.AttendEventDay)
                return UIAttendEventElement.State.ProgressWait;

            // 보상 완료
            if (day <= eventModel.AttendEventRewardStep)
                return UIAttendEventElement.State.ReceivedReward;

            // 출석 일수까지 보상 완료
            if (eventModel.AttendEventDay == eventModel.AttendEventRewardStep)
                return UIAttendEventElement.State.ReceivedReward;

            // 보상 수령 가능
            if (day == eventModel.AttendEventRewardStep + 1)
                return UIAttendEventElement.State.StandByReward;

            // 이전 보상 수령 대기
            return UIAttendEventElement.State.StandByRewardWait;
        }

        private class AttendEventElement : UIAttendEventElement.IInput
        {
            public int Day { get; }
            public RewardData Reward { get; }
            public UIAttendEventElement.State State => checkState(Day);

            private readonly System.Func<int, UIAttendEventElement.State> checkState;

            public AttendEventElement(EventLoginBonusData data, System.Func<int, UIAttendEventElement.State> checkState)
            {
                Day = data.day;
                Reward = data.GetRewardData();
                this.checkState = checkState;
            }
        }
    }
}