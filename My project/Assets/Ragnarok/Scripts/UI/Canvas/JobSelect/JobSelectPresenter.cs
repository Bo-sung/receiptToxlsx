using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIJobSelect"/>
    /// </summary>
    public class JobSelectPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly CharacterModel characterModel;

        // <!-- Repositories --!>
        private readonly JobDataManager jobDataRepo;
        private readonly SkillDataManager skillDataRepo;

        // <!-- Event --!>
        public event System.Action OnFinishedJobChange;
        public event System.Action OnFinishedJobReplace;

        public JobSelectPresenter()
        {
            characterModel = Entity.player.Character;
            jobDataRepo = JobDataManager.Instance;
            skillDataRepo = SkillDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public Gender GetGender()
        {
            return characterModel.Gender;
        }

        public Job GetJob()
        {
            return characterModel.Job;
        }

        public string GetJobDescription(Job job)
        {
            JobData jobData = jobDataRepo.Get((int)job);
            if (jobData == null)
                return string.Empty;

            return jobData.des_id.ToText();
        }

        public UISkillPreview.IInput[] GetSkillPreviews(Job job)
        {
            JobData jobData = jobDataRepo.Get((int)job);
            if (jobData == null)
                return null;

            int[] skillIds = jobData.GetSkillIDs();
            SkillData[] skillPreviews = new SkillData[skillIds.Length];
            for (int i = 0; i < skillPreviews.Length; i++)
            {
                SkillData skillData = skillDataRepo.Get(skillIds[i], level: 1);
                if (skillData == null)
                    continue;

                skillPreviews[i] = skillData;
            }

            return skillPreviews;
        }

        public float[] GetProgressStatus(Job job)
        {
            float[] result = new float[6];
            JobData jobData = jobDataRepo.Get((int)job);
            if (jobData == null)
                return result;

            float totalGuide = (jobData.guide_str + jobData.guide_agi + jobData.guide_vit + jobData.guide_int + jobData.guide_dex + jobData.guide_luk) / 2f;
            result[0] = jobData.guide_str / totalGuide;
            result[1] = jobData.guide_agi / totalGuide;
            result[2] = jobData.guide_vit / totalGuide;
            result[3] = jobData.guide_int / totalGuide;
            result[4] = jobData.guide_dex / totalGuide;
            result[5] = jobData.guide_luk / totalGuide;
            return result;
        }

        public Job[] GetJobArray(UIJobSelect.State state)
        {
            switch (state)
            {
                default:
                case UIJobSelect.State.JobChange:
                    {
                        JobData[] arrData = jobDataRepo.GetNextJobs(characterModel.Job);
                        return System.Array.ConvertAll(arrData, a => a.id.ToEnum<Job>());
                    }
                case UIJobSelect.State.JobReplace:
                    {
                        int jobGrade = characterModel.JobGrade();
                        JobData[] arrData = jobDataRepo.GetJobs(jobGrade);
                        return System.Array.ConvertAll(arrData, a => a.id.ToEnum<Job>());
                    }
            }
        }

        public Job[] GetJobArray(Job job)
        {
            JobData data = jobDataRepo.Get((int)job);
            JobData[] arrData = jobDataRepo.GetJobGradeWithPreviousIndex(data.grade, data.previous_index);
            return System.Array.ConvertAll(arrData, a => a.id.ToEnum<Job>());
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
        /// 전직 요청
        /// </summary>
        public async Task RequestCharacterChangeJob(Job job)
        {
            int jobLevel = characterModel.JobLevel;
            int nextGrade = characterModel.JobNextGrade();
            int needJobLevel = BasisType.JOB_MAX_LEVEL.GetInt(nextGrade); // 전직 가능한 레벨

            if (jobLevel < needJobLevel)
            {
                string message = LocalizeKey._30002.ToText() // {GRADE}차 전직은 {LEVEL}레벨에 가능합니다.
                    .Replace(ReplaceKey.GRADE, nextGrade.ToString())
                    .Replace(ReplaceKey.LEVEL, needJobLevel.ToString());

                UI.ShowToastPopup(message);
                return;
            }

            bool isSuccess = await characterModel.RequestCharacterChangeJob(job);
            if (!isSuccess)
                return;

            UIJobReward.attackPowerInfoDelay = 3f;

            // 보상 팝업 보여주기
            UI.Show<UIJobReward>(new UIJobReward.Input { job = job });

            OnFinishedJobChange?.Invoke();           
        }

        /// <summary>
        /// 직업 변경 요청
        /// </summary>
        public async Task RequestJobChangeTicket(Job job)
        {
            if (characterModel.Job == job)
            {
                UI.ShowToastPopup(LocalizeKey._22004.ToText()); // 같은 직업으로 변경할 수 없습니다.
                return;
            }

            bool isSuccess = await characterModel.RequestJobChangeTicket(job);
            if (!isSuccess)
                return;

            UI.Close<UIConsumableInfo>();
            UI.Close<UIInven>();
            OnFinishedJobReplace?.Invoke();
        }
    }
}