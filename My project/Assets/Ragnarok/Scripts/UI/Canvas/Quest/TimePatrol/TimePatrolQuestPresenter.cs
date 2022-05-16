namespace Ragnarok
{
    public class TimePatrolQuestPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly QuestModel questModel;
        private readonly SoundManager soundManager;

        // <!-- Repositories --!>

        private bool isPlayUISfs;

        // <!-- Event --!>
        public event System.Action<bool> OnReqeustReward
        {
            add { questModel.OnReqeustReward += value; }
            remove { questModel.OnReqeustReward -= value; }
        }

        public event System.Action OnUpdateTimePatrolQuest
        {
            add { questModel.OnUpdateTimePatrolQuest += value; }
            remove { questModel.OnUpdateTimePatrolQuest -= value; }
        }

        public TimePatrolQuestPresenter()
        {
            questModel = Entity.player.Quest;
            soundManager = SoundManager.Instance;
            isPlayUISfs = false;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public QuestInfo GetQuest()
        {
            return questModel.GetTimePatrolQuest();
        }

        public void PlayUISfx()
        {
            if (isPlayUISfs)
                return;

            isPlayUISfs = true;
            soundManager.PlayUISfx(Sfx.UI.ChangeCard);
        }

        public void OnClickedBtnQuest()
        {
            QuestInfo curQuest = GetQuest();
            if (curQuest is null)
                return;

            if (curQuest.IsInvalidData)
                return;

            bool isReward = curQuest.CompleteType == QuestInfo.QuestCompleteType.StandByReward;

            if (!isReward)
                return;

            questModel.RequestQuestRewardAsync(curQuest).WrapNetworkErrors();
        }
    }
}