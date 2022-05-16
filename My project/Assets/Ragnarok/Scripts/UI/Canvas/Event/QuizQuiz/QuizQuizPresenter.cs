using Ragnarok.View;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIQuizQuiz"/>
    /// </summary>
    public sealed class QuizQuizPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly EventModel eventModel;

        // <!-- Repositories --!>
        private readonly EventQuizDataManager eventQuizDataRepo;

        // <!-- Event --!>
        public event System.Action<bool, RewardData> OnQuizReward
        {
            add { eventModel.OnQuizReward += value; }
            remove { eventModel.OnQuizReward -= value; }
        }

        /// <summary>
        /// 진행 기간 초과
        /// </summary>
        public event System.Action OnNotInProgress;

        public QuizQuizPresenter()
        {
            eventModel = Entity.player.Event;
            eventQuizDataRepo = EventQuizDataManager.Instance;
        }

        public override void AddEvent()
        {
            eventModel.OnUpdateQuizInfo += OnUpdateQuizInfo;
        }

        public override void RemoveEvent()
        {
            eventModel.OnUpdateQuizInfo -= OnUpdateQuizInfo;
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
    }
}