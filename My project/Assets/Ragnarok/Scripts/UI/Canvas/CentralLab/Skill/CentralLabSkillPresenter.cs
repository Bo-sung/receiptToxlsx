namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICentralLabSkill"/>
    /// </summary>
    public class CentralLabSkillPresenter : ViewPresenter
    {
        // <!-- Repositories --!>
        private readonly SkillDataManager skillDataRepo;

        public CentralLabSkillPresenter()
        {
            skillDataRepo = SkillDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public int GetSkillNameId(int skillId, int skillLevel)
        {
            SkillData data = skillDataRepo.Get(skillId, skillLevel);

            if (data == null)
                return 0;

            return data.name_id;
        }

        public int GetSkillDescriptionId(int skillId, int skillLevel)
        {
            SkillData data = skillDataRepo.Get(skillId, skillLevel);

            if (data == null)
                return 0;

            return data.des_id;
        }
    }
}