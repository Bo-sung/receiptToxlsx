using Ragnarok.View.Skill;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UISkillSelect"/>
    /// </summary>
    public sealed class SkillSelectPresenter : ViewPresenter
    {
        private readonly ISkillSelectCanvas canvas;
        private readonly SkillDataManager skillDataRepo;
        private readonly CharacterModel characterModel;
        private readonly Buffer<SkillData> skillBuffer;
        /******************** Pool ********************/
        private readonly Dictionary<int, SkillGroupInfo> skillGropuInfoDic;

        public SkillSelectPresenter(ISkillSelectCanvas canvas)
        {
            this.canvas = canvas;
            skillDataRepo = SkillDataManager.Instance;
            characterModel = Entity.player.Character;
            skillBuffer = new Buffer<SkillData>();

            skillGropuInfoDic = new Dictionary<int, SkillGroupInfo>(IntEqualityComparer.Default);
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
            skillGropuInfoDic.Clear();
        }

        /// <summary>
        /// 해당 스킬에 해당하는 스킬 목록 반환
        /// </summary>
        public UISkillInfoSelect.IInfo[] GetSkills(int skillId)
        {
            UISkillInfoSelect.IInfo skillData = skillDataRepo.Get(skillId, level: 1);

            if (skillData == null)
                return null;

            switch (skillData.SkillType)
            {
                case SkillType.SummonBall:
                    List<int> summonBallKeyList = BasisType.SUMMON_BALL_SKILL_ID.GetKeyList();
                    for (int i = 0; i < summonBallKeyList.Count; i++)
                    {
                        int tempSkillId = BasisType.SUMMON_BALL_SKILL_ID.GetInt(summonBallKeyList[i]);
                        if (tempSkillId == 0)
                            continue;

                        SkillData tempSkillData = skillDataRepo.Get(tempSkillId, level: 1);
                        if (tempSkillData == null)
                            continue;

                        skillBuffer.Add(tempSkillData);
                    }
                    break;

                case SkillType.RuneMastery:
                    List<int> RuneMasteryKeyList = BasisType.RUNE_MASTERY_SKILL_IDS.GetKeyList();
                    for (int i = 0; i < RuneMasteryKeyList.Count; i++)
                    {
                        int tempSkillId = BasisType.RUNE_MASTERY_SKILL_IDS.GetInt(RuneMasteryKeyList[i]);
                        if (tempSkillId == 0)
                            continue;

                        SkillData tempSkillData = skillDataRepo.Get(tempSkillId, level: 1);
                        if (tempSkillData == null)
                            continue;

                        skillBuffer.Add(tempSkillData);
                    }
                    break;
            }

            return skillBuffer.GetBuffer(isAutoRelease: true);
        }

        /// <summary>
        /// 특정 스킬 정보
        /// </summary>
        public ISkillViewInfo GetSkillInfo(int skillId)
        {
            return GetSkillGroupInfo(skillId);
        }

        private SkillGroupInfo GetSkillGroupInfo(int skillId)
        {
            if (skillId == 0)
                return null;

            if (!skillGropuInfoDic.ContainsKey(skillId))
                skillGropuInfoDic.Add(skillId, new SkillGroupInfo(skillId, skillDataRepo));

            return skillGropuInfoDic[skillId];
        }

        private class SkillGroupInfo : ISkillViewInfo
        {
            private readonly int skillId;
            private readonly SkillDataManager.ISkillDataRepoImpl skillDataManagerImpl;

            public SkillGroupInfo(int skillId, SkillDataManager.ISkillDataRepoImpl skillDataManagerImpl)
            {
                this.skillId = skillId;
                this.skillDataManagerImpl = skillDataManagerImpl;
            }

            public int GetSkillId()
            {
                return skillId;
            }

            public int GetSkillLevel()
            {
                return 0;
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