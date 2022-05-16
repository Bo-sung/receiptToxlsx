using Ragnarok.View.Skill;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UISkillTooltip"/>
    /// </summary>
    public sealed class SkillTooltipPresenter : ViewPresenter
    {
        private readonly ISkillTooltipCanvas canvas;
        private readonly SkillDataManager skillDataRepo;

        public SkillTooltipPresenter(ISkillTooltipCanvas canvas)
        {
            this.canvas = canvas;
            skillDataRepo = SkillDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public ISkillViewInfo GetSkill(int skillId, int skillLevel)
        {
            return new SkillGroupInfo(skillId, skillLevel, skillDataRepo);
        }

        private class SkillGroupInfo : ISkillViewInfo
        {
            private readonly int skillId;
            private readonly int skillLevel;
            private readonly SkillDataManager.ISkillDataRepoImpl skillDataManagerImpl;

            public SkillGroupInfo(int skillId, int skillLevel, SkillDataManager.ISkillDataRepoImpl skillDataManagerImpl)
            {
                this.skillId = skillId;
                this.skillLevel = skillLevel;
                this.skillDataManagerImpl = skillDataManagerImpl;
            }

            public int GetSkillId()
            {
                return skillId;
            }

            public int GetSkillLevel()
            {
                return skillLevel;
            }

            public bool HasSkill(int level)
            {
                return skillDataManagerImpl.Get(skillId, level) != null;
            }

            public SkillData.ISkillData GetSkillData(int level)
            {
                return skillDataManagerImpl.Get(skillId, level);
            }

            public int GetSkillLevelNeedPoint(int plusLevel)
            {
                return 0;
            }

            public int GetSkillLevelUpNeedRank()
            {
                return 0;
            }
        }
    }
}