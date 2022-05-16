#undef USE_GUIDE

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIJobChangeMenu"/>
    /// </summary>
    public class JobChangeMenuPresenter : ViewPresenter
    {
        // <!-- Repositories --!>
#if USE_GUIDE
        private readonly GuideDataManager guideDataRepo;
#endif
        private readonly EventQuizDataManager eventQuizDataRepo;

        // <!-- Models --!>
        private readonly CharacterModel characterModel;
        private readonly QuestModel questModel;
        private readonly EventModel eventModel;

        public event System.Action OnChangeJobLevel;
        public event System.Action OnChangeJob;
        public event System.Action OnUpdateGuideQuest
        {
            add { questModel.OnUpdateMainQuest += value; }
            remove { questModel.OnUpdateMainQuest -= value; }
        }

        public event System.Action OnUpdateNewOpenContent
        {
            add { questModel.OnUpdateNewOpenContent += value; }
            remove { questModel.OnUpdateNewOpenContent -= value; }
        }

        public event System.Action OnUpdateQuizInfo
        {
            add { eventModel.OnUpdateQuizInfo += value; }
            remove { eventModel.OnUpdateQuizInfo -= value; }
        }

        public event System.Action OnCatCoinGiftReward
        {
            add { eventModel.OnUpdateCatCoinGiftInfo += value; }
            remove { eventModel.OnUpdateCatCoinGiftInfo -= value; }
        }

        /// <summary>
        /// 보상 받을수 있는 퀘스트 수 변경시 호출
        /// </summary>
        public event System.Action OnStandByReward
        {
            add { questModel.OnStandByReward += value; }
            remove { questModel.OnStandByReward -= value; }
        }

        public JobChangeMenuPresenter()
        {
#if USE_GUIDE
            guideDataRepo = GuideDataManager.Instance;
#endif
            eventQuizDataRepo = EventQuizDataManager.Instance;
            characterModel = Entity.player.Character;
            questModel = Entity.player.Quest;
            eventModel = Entity.player.Event;
        }

        public override void AddEvent()
        {
            characterModel.OnUpdateJobLevel += InvokeChangeJobLevel;
            characterModel.OnChangedJob += InvokeChangeJob;
        }

        public override void RemoveEvent()
        {
            characterModel.OnUpdateJobLevel -= InvokeChangeJobLevel;
            characterModel.OnChangedJob -= InvokeChangeJob;
        }

        /// <summary>
        /// 컨텐츠 오픈 여부
        /// </summary>
        public bool IsOpenContent(UIJobChangeMenu.MenuContent content)
        {
            switch (content)
            {
                case UIJobChangeMenu.MenuContent.JobChange:
                    return CanJobChange();

                case UIJobChangeMenu.MenuContent.SpecialEvent:
                    return IsShowSpecialEvent();

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIJobChangeMenu.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 알림 표시 여부
        /// </summary>
        public bool GetHasNotice(UIJobChangeMenu.MenuContent content)
        {
            switch (content)
            {
                case UIJobChangeMenu.MenuContent.JobChange:
                    return false;
                
                case UIJobChangeMenu.MenuContent.SpecialEvent:
                    // 퀴즈이벤트 활성화 중이거나 냥다래 보상을 받을 수 있을 때
                    return ActivationQuizEvent() || eventModel.CanRewardCatCoinGift(questModel.IsTotalClearQuest()) || questModel.CanRewardNormalQuest();

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIJobChangeMenu.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 신규 컨텐츠 여부
        /// </summary>
        public bool GetHasNewIcon(UIJobChangeMenu.MenuContent content)
        {
            switch (content)
            {
                case UIJobChangeMenu.MenuContent.JobChange:
                    return questModel.HasNewOpenContent(ContentType.JobChange);

                case UIJobChangeMenu.MenuContent.SpecialEvent:
                    return false;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIJobChangeMenu.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        void InvokeChangeJobLevel(int jobLevel)
        {
            OnChangeJobLevel?.Invoke();
        }

        void InvokeChangeJob(bool isInit)
        {
            OnChangeJob?.Invoke();
        }

        private bool CanJobChange()
        {
            return questModel.IsOpenContent(ContentType.JobChange, isShowPopup: false) && characterModel.CanChangeJob();
        }

        private bool IsShowSpecialEvent()
        {
            // 전직 가능할 경우에는 퀴즈 숨김
            if (CanJobChange())
                return false;

            // 스페셜이벤트 체크(퀴즈데이터, 냥다래받기 상태)
            if (ActivationQuizEvent())
                return true;

            if (eventModel.ActivationCatCoinGiftEvent())
                return true;

            if (questModel.HasRemainNormalQuest())
                return true;

            return false;
        }

        private bool ActivationQuizEvent()
        {
            var data = eventQuizDataRepo.Get(eventModel.EventQuizStartDate, eventModel.EventQuizSeqIndex);
            if (data == null)
                return false;

            return true;
        }

#if USE_GUIDE
        private bool IsShowGuide()
        {
            // 전직 가능할 경우에는 가이드 숨김
            if (CanJobChange())
                return false;

            IOpenConditional[] openConditionals = guideDataRepo.GetArrayData();
            if (openConditionals == null)
                return false;

            foreach (IOpenConditional openConditional in openConditionals)
            {
                // 오픈 조건을 만족하지 않음
                if (!dungeonModel.IsOpend(openConditional, isShowPopup: false))
                    return true;
            }

            return false;
        }
#endif
    }
}