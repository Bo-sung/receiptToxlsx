namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIJobReplace"/>
    /// </summary>
    public sealed class JobReplacePresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly CharacterModel characterModel;

        // <!-- Repositories --!>
        private readonly JobDataManager jobDataRepo;

        public Gender Gender { get; private set; }

        public JobReplacePresenter()
        {
            characterModel = Entity.player.Character;
            jobDataRepo = JobDataManager.Instance;

            Gender = characterModel.Gender;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public Job[] GetReplaceJobs()
        {
            int jobGrade = characterModel.JobGrade();
            JobData[] arrData = jobDataRepo.GetJobs(jobGrade);
            return System.Array.ConvertAll(arrData, a => a.id.ToEnum<Job>());
        }      
    }
}