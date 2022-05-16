using Ragnarok.View;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIJobReward"/>
    /// </summary>
    public sealed class JobRewardPresenter : ViewPresenter
    {
        // <!-- Repositories --!>
        private readonly JobDataManager jobDataRepo;
        private readonly ItemDataManager itemDataRepo;
        private readonly SkillDataManager skillDataRepo;

        private Job job;

        public JobRewardPresenter()
        {
            jobDataRepo = JobDataManager.Instance;
            itemDataRepo = ItemDataManager.Instance;
            skillDataRepo = SkillDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void SetJob(Job job)
        {
            this.job = job;
        }

        /// <summary>
        /// 전직 보상
        /// </summary>
        public (string iconName, int itemNameId) GetJobRewardInfo()
        {
            int value = (int)job;
            int rewardId = BasisType.JOB_REWARD.GetInt(value);
            ItemData itemData = itemDataRepo.Get(rewardId);
            if (itemData == null)
                return (string.Empty, 0);

            return (itemData.icon_name, itemData.name_id);
        }

        /// <summary>
        /// 전직 새로운 스킬
        /// </summary>
        public UISkillInfo.IInfo[] GetSkills()
        {
            int value = (int)job;
            JobData data = jobDataRepo.Get(value);
            if (data == null)
                return null;

            Buffer<SkillData> buffer = new Buffer<SkillData>();
            int[] skillIds = data.GetSkillIDs();
            foreach (int skillId in skillIds)
            {
                if (skillId == 0)
                    continue;

                SkillData skillData = skillDataRepo.Get(skillId, level: 1);
                if (skillData == null)
                    continue;

                buffer.Add(skillData);
            }

            return buffer.GetBuffer(isAutoRelease: true);
        }

        public bool IsShowReviewPopup()
        {
            int value = (int)job;
            JobData data = jobDataRepo.Get(value);
            if (data == null)
                return false;

            return data.grade == 1;
        }
    }
}