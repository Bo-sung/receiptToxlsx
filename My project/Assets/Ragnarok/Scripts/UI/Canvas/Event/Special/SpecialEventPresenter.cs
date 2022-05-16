using Ragnarok.View;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UISpecialEvent"/>
    /// </summary>
    public class SpecialEventPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly EventModel eventModel;
        private readonly QuestModel questModel;

        // <!-- Repositories --!>
        private readonly EventQuizDataManager eventQuizDataRepo;

        // <!-- Event --!>
        public event System.Action<bool, RewardData> OnQuizReward
        {
            add { eventModel.OnQuizReward += value; }
            remove { eventModel.OnQuizReward -= value; }
        }

        public event System.Action OnCatCoinGiftReward
        {
            add { eventModel.OnUpdateCatCoinGiftInfo += value; }
            remove { eventModel.OnUpdateCatCoinGiftInfo -= value; }
        }

        /// <summary>
        /// 진행 기간 초과
        /// </summary>
        public event System.Action OnNotInProgress;

        /// <summary>
        /// 보상 받을수 있는 퀘스트 수 변경시 호출
        /// </summary>
        public event System.Action OnStandByReward
        {
            add { questModel.OnStandByReward += value; }
            remove { questModel.OnStandByReward -= value; }
        }

        public SpecialEventPresenter()
        {
            eventModel = Entity.player.Event;
            questModel = Entity.player.Quest;
            eventQuizDataRepo = EventQuizDataManager.Instance;
        }

        public override void AddEvent()
        {
            eventModel.OnUpdateQuizInfo += OnUpdateQuizInfo;

            // 냥다래 보상 받기 이벤트
            foreach (var info in eventModel.GetCatCoinGiftInfos())
            {
                info.OnReward += OnRewardCatCoinGift;
            }
        }

        public override void RemoveEvent()
        {
            eventModel.OnUpdateQuizInfo -= OnUpdateQuizInfo;

            // 냥다래 보상 받기 이벤트
            foreach (var info in eventModel.GetCatCoinGiftInfos())
            {
                info.OnReward -= OnRewardCatCoinGift;
            }
        }

        void OnUpdateQuizInfo()
        {
            // 시퀀스가 초기화 됨
            if (eventModel.EventQuizSeqIndex == 0)
                OnNotInProgress?.Invoke();
        }

        public void RequestEventQuizReward(int id, byte answer, bool isCorrect)
        {
            eventModel.RequestEventQuizReward(id, answer, isCorrect).WrapNetworkErrors();
        }

        public QuizQuizView.IInput GetData()
        {
            return eventQuizDataRepo.Get(eventModel.EventQuizStartDate, eventModel.EventQuizSeqIndex);
        }

        public bool ActivationQuizEvent()
        {
            var data = GetData();
            if (data == null)
                return false;

            return true;
        }

        public CatCoinGiftData[] GetGiftData()
        {
            return eventModel.GetCatCoinGiftInfos(questModel.IsTotalClearQuest());
        }

        public bool ActivationCatCoinGiftEvent()
        {
            return eventModel.ActivationCatCoinGiftEvent();
        }

        public bool CanMultiMazeQuest()
        {
            return questModel.CanRewardNormalQuest();
        }

        public bool CanRewardCatCoinGift()
        {
            return eventModel.CanRewardCatCoinGift(questModel.IsTotalClearQuest());
        }

        private void OnRewardCatCoinGift(int id)
        {
            // 냥다래 보상 수령, id 정보는 필요없어도..?
            eventModel.RequestUserEventQuestReward().WrapNetworkErrors();
        }

        /// <summary>
        /// 미궁 정복자 활성화 여부
        /// </summary>
        public bool ActiveMultiMazeQuest()
        {
            foreach (var item in questModel.GetNormalQuests())
            {
                if (item.CompleteType != QuestInfo.QuestCompleteType.ReceivedReward)
                    return true;
            }
            return false;
        }

        public UIMultiMazeQuestElement.IInput[] GetArrayData()
        {
            return questModel.GetNormalQuests();
        }

        public void RequestMultiMazeReward(int id)
        {
            foreach (var item in questModel.GetNormalQuests())
            {
                if (item.ID == id)
                {
                    questModel.RequestQuestRewardAsync(item).WrapNetworkErrors();
                    break;
                }
            }
        }
    }
}