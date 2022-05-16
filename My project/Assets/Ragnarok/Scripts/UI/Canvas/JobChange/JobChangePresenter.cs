namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIJobChange"/>
    /// </summary>
    public sealed class JobChangePresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly CharacterModel characterModel;
        private readonly QuestModel questModel;

        // <!-- Repositories --!>
        private readonly JobDataManager jobDataRepo;

        public Gender Gender { get; private set; }

        public event System.Action<bool> OnChangedJob
        {
            add { characterModel.OnChangedJob += value; }
            remove { characterModel.OnChangedJob -= value; }
        }

        public JobChangePresenter()
        {
            characterModel = Entity.player.Character;
            questModel = Entity.player.Quest;
            jobDataRepo = JobDataManager.Instance;

            Gender = characterModel.Gender;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public Job[] GetNextJobs()
        {
            var data = jobDataRepo.GetNextJobs(characterModel.Job);
            Job[] jobs = new Job[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                jobs[i] = data[i].id.ToEnum<Job>();
            }

            return jobs;
        }

        /// <summary>
        /// 전직 가능 레벨
        /// </summary>
        public int GetNeedJobLevel()
        {
            int nextGrade = characterModel.JobNextGrade();
            return BasisType.JOB_MAX_LEVEL.GetInt(nextGrade); // 전직 가능한 레벨
        }

        /// <summary>
        /// 신규 컨텐츠 플래그 제거
        /// </summary>
        public void RemoveNewOpenContentJobChange()
        {
            questModel.RemoveNewOpenContent(ContentType.JobChange); // 신규 컨텐츠 플래그 제거 (전직)
        }
    }
}