using UnityEngine;

namespace Ragnarok
{
    public class JobInfoPreviewPresenter : ViewPresenter
    {
        public interface IView
        {
            void RefreshView();
        }

        /******************** Repositories ********************/
        private readonly JobDataManager jobDataRepo;
        private readonly SkillDataManager skillDataRepo;

        private IView view;

        private Job rootJob; // 보여줄 직업들의 부모 Job (노비스일 경우 -> 법사, 전사, 도적, 아처 등을 보여줌)
        private Gender gender;

        private Job currentJob;
        private Job previousJob;
        private Job nextJob;

        public Job CurrentJob => currentJob;
        public Job PreviousJob => previousJob;
        public Job NextJob => nextJob;
        public Gender Gender => gender;
        public JobData JobData => jobDataRepo.Get(currentJob.ToIntValue());

        public JobInfoPreviewPresenter(IView view)
        {
            this.view = view;

            jobDataRepo = JobDataManager.Instance;
            skillDataRepo = SkillDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void SetData(Job job, Gender gender, Job selectJob)
        {
            this.rootJob = job;
            this.gender = gender;
            SelectJob(selectJob);
        }

        public void SetData(Job job, Gender gender, int index)
        {
            this.rootJob = job;
            this.gender = gender;
            SelectJob(GetNextJobs()[index]);
        }

        public UISkillPreview.IInput[] GetSkillPreviews()
        {
            int[] skillIds = JobData.GetSkillIDs();
            SkillPreviewInput[] skillPreviews = new SkillPreviewInput[skillIds.Length];
            for (int i = 0; i < skillPreviews.Length; i++)
            {
                SkillData skillData = skillDataRepo.Get(skillIds[i], level: 1);
                if (skillData == null)
                    continue;

                skillPreviews[i] = new SkillPreviewInput(skillIds[i], skillData.icon_name);
            }

            return skillPreviews;
        }

        public void SelectJob(Job job)
        {
            this.currentJob = job;
            var jobs = GetNextJobs();
            if (jobs.Length == 1)
            {
                previousJob = nextJob = job;
                return;
            }

            for (int index = 0; index < jobs.Length; index++)
            {
                if (currentJob == jobs[index])
                {
                    int prev = index > 0 ? index - 1 : jobs.Length - 1;
                    int next = index < jobs.Length - 1 ? index + 1 : 0;
                    previousJob = jobs[prev];
                    nextJob = jobs[next];
                    break;
                }
            }

            view.RefreshView();
        }

        public void SelectPreviousJob()
        {
            SelectJob(PreviousJob);
        }

        public void SelectNextJob()
        {
            SelectJob(NextJob);
        }

        public Job[] GetNextJobs()
        {
            var data = jobDataRepo.GetNextJobs(rootJob);
            Job[] jobs = new Job[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                jobs[i] = data[i].id.ToEnum<Job>();
            }
            return jobs;
        }

        public bool IsCurrentLangEnglish()
        {
            return Language.Current == LanguageType.ENGLISH;
        }

        private class SkillPreviewInput : UISkillPreview.IInput
        {
            public int SkillId { get; private set; }
            public string IconName { get; private set; }

            public SkillPreviewInput(int skillId, string iconName)
            {
                SkillId = skillId;
                IconName = iconName;
            }
        }
    }
}