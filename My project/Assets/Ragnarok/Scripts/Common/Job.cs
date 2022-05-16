namespace Ragnarok
{
    public enum Job : byte
    {
        /* 0차 직업 */
        Novice = 1,           // 노비스

        /* 1차 직업 */
        Swordman = 2,         // 검사
        Magician = 3,         // 마법사
        Thief = 4,            // 도둑
        Archer = 5,           // 궁수

        /* 2차 직업 */
        Knight = 6,           // 나이트
        Crusader = 7,         // 크루세이더
        Wizard = 8,           // 위저드
        Sage = 9,             // 세이지
        Assassin = 10,        // 어쌔신
        Rogue = 11,           // 로그
        Hunter = 12,          // 헌터
        Dancer = 13,          // 댄서

        /* 3차 직업 */
        LordKnight = 14,      // 로드 나이트
        Paladin = 15,         // 팔라딘
        HighWizard = 16,      // 하이 위저드
        Professor = 17,       // 프로페서
        AssassinCross = 18,   // 어쌔신 크로스
        Stalker = 19,         // 스토커
        Sniper = 20,          // 스나이퍼
        Clown = 21,           // 클로운

        /* 4차 직업 */
        RuneKnight = 22,      // 룬 나이트
        RoyalGuard = 23,      // 로얄 가드
        Warlock = 24,         // 워록
        Sorcerer = 25,        // 소서러
        GuillotineCross = 26, // 길로틴 크로스
        ShadowChaser = 27,    // 쉐도우 체이서
        Ranger = 28,          // 레인져
        Wanderer = 29,        // 윈더러
    }

    public static class JobExtention
    {
        /// <summary>
        /// 직업 이름
        /// </summary>
        public static string GetJobName(this Job job)
        {
            JobData jobData = JobDataManager.Instance.Get((int)job);
            return jobData == null ? string.Empty : jobData.name_id.ToText();
        }

        public static string GetJobName(this Job job, LanguageType languageType)
        {
            JobData jobData = JobDataManager.Instance.Get((int)job);
            return jobData == null ? string.Empty : jobData.name_id.ToText(languageType);
        }

        /// <summary>직업 아이콘 이름</summary>
        public static string GetJobIcon(this Job job)
        {
            return job.ToString();
        }

        public static string GetJobIllust(this Job job, Gender gender)
        {
            return string.Concat("JobIllust_", job.GetJobIcon(), "_", gender.ToString());
        }

        public static string GetJobSDName(this Job job, Gender gender)
        {
            return string.Concat("JobSD_", job.GetJobIcon(), "_", gender.ToString());
        }

        /// <summary>
        /// 작은 원형 일러스트
        /// </summary>
        public static string GetThumbnailName(this Job job, Gender gender)
        {
            string genderName = gender == Gender.Female ? "F" : "M";
            return string.Concat(job.ToString(), "_", genderName);
        }

        /// <summary>직업 프로필 이미지 이름</summary>
        public static string GetJobProfile(this Job job, Gender gender)
        {
            string genderName = gender == Gender.Female ? "F" : "M";
            return string.Concat("Info_", job.GetJobIcon(), "_", genderName);
        }

        public static string GetAgentName(this Job job, Gender gender)
        {
            string genderName = gender == Gender.Female ? "F" : "M";
            return string.Concat("Agent_", job.ToString(), "_", genderName);
        }

        public static EquipmentClassType GetJobAppropriateEquipmentClassTypes(this Job job)
        {
            return JobDataManager.Instance.Get((int)job).appropriate_class_bit_type.ToEnum<EquipmentClassType>();
        }

        public static JobFilter ToJobFilter(this Job job)
        {
            switch (job)
            {
                case Job.Novice:
                    return JobFilter.None;

                case Job.Swordman:
                    return JobFilter.Knight | JobFilter.Crusader;

                case Job.Magician:
                    return JobFilter.Wizard | JobFilter.Sage;

                case Job.Thief:
                    return JobFilter.Assassin | JobFilter.Rogue;

                case Job.Archer:
                    return JobFilter.Hunter | JobFilter.Dancer;

                case Job.Knight:
                case Job.LordKnight:
                case Job.RuneKnight:
                    return JobFilter.Knight;

                case Job.Crusader:
                case Job.Paladin:
                case Job.RoyalGuard:
                    return JobFilter.Crusader;

                case Job.Wizard:
                case Job.HighWizard:
                case Job.Warlock:
                    return JobFilter.Wizard;

                case Job.Sage:
                case Job.Professor:
                case Job.Sorcerer:
                    return JobFilter.Sage;

                case Job.Assassin:
                case Job.AssassinCross:
                case Job.GuillotineCross:
                    return JobFilter.Assassin;

                case Job.Rogue:
                case Job.Stalker:
                case Job.ShadowChaser:
                    return JobFilter.Rogue;

                case Job.Hunter:
                case Job.Sniper:
                case Job.Ranger:
                    return JobFilter.Hunter;

                case Job.Dancer:
                case Job.Clown:
                case Job.Wanderer:
                    return JobFilter.Dancer;

            }

            return default;
        }
    }
}