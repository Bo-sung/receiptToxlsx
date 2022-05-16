using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIEventBanner"/>
    /// </summary>
    public class EventBannerPresenter : ViewPresenter, IEventQuestGroupInfoOnBuffImpl
    {
        public interface IView
        {
            void UpdateView();
            void CloseUI();
        }

        private readonly IView view;
        private readonly QuestModel questModel;
        private readonly BingoModel bingoModel;
        private readonly EventModel eventModel;
        private readonly InventoryModel inventoryModel;
        private readonly CharacterModel characterModel;

        /// <summary>
        /// 룰렛 뽑기 제한 레벨
        /// </summary>
        private int jobLevelLimit;

        public EventBannerPresenter(IView view)
        {
            this.view = view;
            questModel = Entity.player.Quest;
            bingoModel = Entity.player.Bingo;
            eventModel = Entity.player.Event;
            inventoryModel = Entity.player.Inventory;
            characterModel = Entity.player.Character;
            jobLevelLimit = BasisType.SPECIAL_ROULETTE_JOB_LEVEL_LIMIT.GetInt();
        }

        public override void AddEvent()
        {
            questModel.OnEventQuest += view.UpdateView;
            questModel.OnStandByReward += view.UpdateView;
            characterModel.OnUpdateJobLevel += OnUpdateView;
            eventModel.OnUpdateWordCollectionItemCount += view.UpdateView;
        }

        public override void RemoveEvent()
        {
            questModel.OnEventQuest -= view.UpdateView;
            questModel.OnStandByReward -= view.UpdateView;
            characterModel.OnUpdateJobLevel -= OnUpdateView;
            eventModel.OnUpdateWordCollectionItemCount -= view.UpdateView;
        }

        /// <summary>
        /// 이벤트 퀘스트 그룹 반환
        /// </summary>
        public EventQuestGroupInfo[] GetEventQuestGroupInfos()
        {
            return questModel.GetEventQuestGroupInfos();
        }

        public async void RequestEventQuest()
        {
            await questModel.RequestEventQuest();
            await RequestOnBuffTotalRemainPoint();
            view.UpdateView();
        }

        public void CloseUI()
        {
            view.CloseUI();
        }

        public void ShowCouponPopup()
        {
            UI.Show<UICouponPopup>();
        }

        /// <summary>
        /// 빙고 퀘스트 보상 받을수 있는지 여부
        /// </summary>
        /// <returns></returns>
        public bool IsBingoQuestStandByReward()
        {
            return bingoModel.IsBingoQuestStandByReward();
        }

        /// <summary>
        /// 스페셜 룰렛 뽑기 가능 여부
        /// </summary>
        public bool IsSpecialRouletteNotice()
        {
            if (IsJobLevelLimit())
                return false;

            if (!eventModel.IsRemainTimeRoulette())
                return false;

            if (eventModel.IsSpecialRouletteMaxUsed())
                return false;

            int count = inventoryModel.GetItemCount(eventModel.SpecialRouletteItemId);

            return count >= eventModel.GetSpecialRouletteNeedCount();
        }

        /// <summary>
        /// 스페셜 룰렛 직업 레벨 제한 여부
        /// </summary>
        private bool IsJobLevelLimit()
        {
            return characterModel.JobLevel < jobLevelLimit;
        }

        /// <summary>
        /// [출석체크 이벤트] 받을 보상이 있는지 여부
        /// </summary>
        public bool IsAttendEventStandByReward()
        {
            EventQuestGroupInfo eventData = questModel.GetEventQuestByShortCut(ShortCutType.AttendEvent);
            if (eventData == null || eventData.RemainTime.ToRemainTime() <= 0)
                return false;

            return eventModel.IsAttendEventStandByReward();
        }

        /// <summary>
        /// [단어수집 이벤트] 받을 보상이 있는지 여부
        /// </summary>
        public bool IsWordCollectionStandByReward()
        {
            EventQuestGroupInfo eventData = questModel.GetEventQuestByShortCut(ShortCutType.WordCollectionEvent);
            if (eventData == null || eventData.RemainTime.ToRemainTime() <= 0)
                return false;

            return eventModel.IsWordCollectionStandByReward();
        }

        private void OnUpdateView(int joblevel)
        {
            view.UpdateView();
        }

        /// <summary>
        /// [온버프] 이벤트 진행중 여부
        /// </summary>
        private bool IsOnBuffEvent()
        {
            if (!GameServerConfig.IsOnBuff())
                return false;

            EventQuestGroupInfo eventData = questModel.GetEventQuestByShortCut(ShortCutType.OnBuffEvent);

            if (eventData == null || eventData.RemainTime.ToRemainTime() <= 0)
                return false;

            return true;
        }

        /// <summary>
        /// [온버프] 전체 이벤트 온버프 포인트 남은 잔여량 조회
        /// </summary>
        public async Task RequestOnBuffTotalRemainPoint()
        {
            if (!IsOnBuffEvent())
                return;

            await eventModel.RequestOnBuffTotalRemainPoint();
        }

        /// <summary>
        /// [온버프] 전체 이벤트 온버프 포인트 남은 잔여량
        /// </summary>
        public long GetOnBuffTotalRemainPoint()
        {
            return eventModel.OnBuffTotalRemainPoint;
        }
    }
}