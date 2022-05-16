namespace Ragnarok
{
    public enum SkillType
    {
        /// <summary>
        /// 패시브
        /// </summary>
        Passive = 1,

        /// <summary>
        /// 액티브
        /// </summary>
        Active = 2,

        /// <summary>
        /// 평타
        /// </summary>
        BasicActiveSkill = 3,

        /// <summary>
        /// 도착(스킬훔쳐배우기) - 1차
        /// </summary>
        Plagiarism = 4,

        /// <summary>
        /// 리프로듀스(스킬훔쳐배우기) - 3차
        /// </summary>
        Reproduce = 5,

        /// <summary>
        /// 서먼 볼
        /// </summary>
        SummonBall = 6,

        /// <summary>
        /// 룬 마스터리
        /// </summary>
        RuneMastery = 7,
    }

    public static class SkillTypeExtension
    {
        public static string GetIconName(this SkillType skillType)
        {
            switch (skillType)
            {
                case SkillType.Active:
                case SkillType.BasicActiveSkill:
                    return "Ui_Common_Icon_SkillActive";
            }

            return "Ui_Common_Icon_SkillPassive";
        }
    }
}