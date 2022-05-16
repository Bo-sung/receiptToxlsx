namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICharacterCreate"/>
    /// </summary>
    public class CharacterCreatePresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly CharacterListModel characterListModel;

        // <!-- Repositories --!>
        private readonly JobDataManager jobDataRepo;
        private readonly SkillDataManager skillDataRepo;

        // <!-- Event --!>

        // <!-- Data --!>
        public Gender Gender { get; private set; }
        public string CharacterName { get; private set; }
        private bool isSendCreateChacter;

        public CharacterCreatePresenter()
        {
            characterListModel = Entity.player.CharacterList;
            jobDataRepo = JobDataManager.Instance;
            skillDataRepo = SkillDataManager.Instance;
            Gender = UnityEngine.Random.value < 0.5f ? Gender.Male : Gender.Female;
            SetRandomCharacterName();
        }

        public override void AddEvent()
        {
            characterListModel.OnCreateCharacter += OnCreateCharacter;
        }

        public override void RemoveEvent()
        {
            characterListModel.OnCreateCharacter -= OnCreateCharacter;
        }

        public void SetGender(Gender gender)
        {
            Gender = gender;
        }

        public void SetRandomCharacterName()
        {
            CharacterName = FilterUtils.GetAutoNickname();
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

        public Job[] GetJobArray(Job job)
        {
            JobData data = jobDataRepo.Get((int)job);
            JobData[] jobDataArray = jobDataRepo.GetJobGradeWithPreviousIndex(data.grade, data.previous_index);
            Job[] Jobs = new Job[jobDataArray.Length];
            for (int i = 0; i < Jobs.Length; i++)
            {
                Jobs[i] = jobDataArray[i].id.ToEnum<Job>();
            }
            return Jobs;
        }

        public void CreateCharacter(string characterName)
        {
            // 중복 생성 방지
            if (isSendCreateChacter)
                return;

            isSendCreateChacter = true;
            characterListModel.RequestCreateCharacter(characterName, (byte)Gender).WrapNetworkErrors();
        }

        void OnCreateCharacter()
        {
            characterListModel.RequestGotoCharList().WrapNetworkErrors();
        }
    }
}