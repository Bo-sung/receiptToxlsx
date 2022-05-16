namespace Ragnarok
{
    public class JobGrowthPresenter : ViewPresenter
    {
        // <!-- Models --!>
        CharacterModel characterModel;
        QuestModel questModel;

        // <!-- Repositories --!>
        ScenarioMazeDataManager mazeRepo;
        JobDataManager jobRepo;
        JobLevelRewardDataManager jobLevelRewardRepo;
        QuestDataManager questDataRepo;

        // <!-- Event --!>

        public JobGrowthPresenter()
        {
            characterModel = Entity.player.Character;
            questModel = Entity.player.Quest;

            mazeRepo = ScenarioMazeDataManager.Instance;
            jobRepo = JobDataManager.Instance;
            jobLevelRewardRepo = JobLevelRewardDataManager.Instance;
            questDataRepo = QuestDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 셰어바이스 레벨업에 필요한 미로 클리어 조건
        /// </summary>
        public int GetLevelUpShareviceId()
        {
            return mazeRepo.GetEventContentUnlock(ContentType.ShareLevelUp).name_id;
        }

        /// <summary>
        /// 내 캐릭터 클론 셰어에 필요한 미로 클리어 조건
        /// </summary>
        public int GetShareCloneId()
        {
            return mazeRepo.GetEventContentUnlock(ContentType.ShareHope).name_id;
        }

        public int GetJobGrade()
        {
            return characterModel.JobGrade();
        }

        public int GetJobLevel()
        {
            return characterModel.JobLevel;
        }

        public Gender GetGender()
        {
            return characterModel.Gender;
        }

        public Job[] GetJobGradeCharacters(int targetJobGrade)
        {
            return jobRepo.GetJobGradeCharacters(characterModel.Job, targetJobGrade);
        }

        /// <summary>
        /// 스킬포인트 획득과 연관된 스테이트 그룹
        /// 그룹 내의 스테이트들은 스킬포인트 획득이 끝나기 전에 완료가 가능
        /// </summary>
        public int[] GetRewardJobLevelsByGroup(JobGrowthState state)
        {
            switch (state)
            {
                case JobGrowthState.SkillPoint1:
                case JobGrowthState.Job2:
                case JobGrowthState.Sharevice1:
                    return jobLevelRewardRepo.GetRewardJobLevelsByGroup(1);

                case JobGrowthState.SkillPoint2:
                case JobGrowthState.Job3:
                    return jobLevelRewardRepo.GetRewardJobLevelsByGroup(2);

                case JobGrowthState.SkillPoint3:
                case JobGrowthState.GuildSquare:
                case JobGrowthState.ShareClone:
                    return jobLevelRewardRepo.GetRewardJobLevelsByGroup(3);

                default:
                    return default;
            }
        }

        public JobGrowthState? GetCurrentGrowthState()
        {
            var jobGrade = GetJobGrade();
            var jobLevel = GetJobLevel();

            if (jobGrade == 1 && GetMaxJobLevelByGroup(JobGrowthState.Job2) <= jobLevel)
                return JobGrowthState.Job2;

            if (jobGrade == 2 && GetMaxJobLevelByGroup(JobGrowthState.Job3) <= jobLevel &&
                        Entity.player.Quest.IsOpenContent(ContentType.ShareLevelUp, false)) // 컨텐츠 해금 후
                return JobGrowthState.Job3;

            if (jobGrade == 3 &&
                        BasisType.GUILD_SQUARE_LIMIT_JOB_LEVEL.GetInt() <= jobLevel &&
                        Entity.player.Quest.IsOpenContent(ContentType.ShareLevelUp, false) && // 컨텐츠 해금 후
                        Entity.player.Quest.IsOpenContent(ContentType.ShareHope, false))
                return JobGrowthState.Job4;

            /*if (jobGrade == 4 &&
                        ActiveShareForce()) // 2세대 쉐어바이스가 활성화 되어있을 때
                return JobGrowthState.Job5;*/

            if (jobGrade >= 2 && // 2차 전직 후에..
                        !Entity.player.Quest.IsOpenContent(ContentType.ShareLevelUp, false)) // 컨텐츠 해금 전 이라면..
                return JobGrowthState.Sharevice1;

            if (jobGrade == 3 && GetMaxJobLevelByGroup(JobGrowthState.GuildSquare) <= jobLevel &&
                        BasisType.GUILD_SQUARE_LIMIT_JOB_LEVEL.GetInt() > jobLevel &&
                        Entity.player.Quest.IsOpenContent(ContentType.ShareLevelUp, false)) // 컨텐츠 해금 후
                return JobGrowthState.GuildSquare;

            if (jobGrade == 3 &&
                        BasisType.GUILD_SQUARE_LIMIT_JOB_LEVEL.GetInt() <= jobLevel &&
                        Entity.player.Quest.IsOpenContent(ContentType.ShareLevelUp, false) && // 컨텐츠 해금 후
                        !Entity.player.Quest.IsOpenContent(ContentType.ShareHope, false))
                return JobGrowthState.ShareClone;

            if (jobGrade >= 4 && // 4차 전직 후에..
                        !ActiveShareForce()) // 2세대 쉐어바이스가 활성화 되지 않은상태면
                return JobGrowthState.Sharevice2;

            if (jobGrade == 1 && GetMaxJobLevelByGroup(JobGrowthState.SkillPoint1) > jobLevel)
                return JobGrowthState.SkillPoint1;

            if (jobGrade == 2 && GetMaxJobLevelByGroup(JobGrowthState.SkillPoint2) > jobLevel)
                return JobGrowthState.SkillPoint2;

            if (jobGrade == 3 && GetMaxJobLevelByGroup(JobGrowthState.SkillPoint3) > jobLevel)
                return JobGrowthState.SkillPoint3;

            return null;
        }

        public int GetRewardSkillPoint(int group)
        {
            return jobLevelRewardRepo.GetRewardSkillPoint(group);
        }

        public bool ActiveShareForce()
        {
            // 첫번째 조각이 있으면, 활성화된 상태
            return characterModel.HasShareForce(ShareForceType.ShareForce1);
        }

        public QuestData GetShareForceQuest()
        {
            // 첫번째 조각을 얻을 때, 쉐어포스가 활성화 됨.
            int seq = questModel.OpenShareForceQuestSeq(ShareForceType.ShareForce1);
            return questDataRepo.GetTimePatrolQuest(seq);
        }

        private int GetMaxJobLevelByGroup(JobGrowthState state)
        {
            var jobLevelArray = GetRewardJobLevelsByGroup(state);
            return jobLevelArray[jobLevelArray.Length - 1];
        }
    }
}