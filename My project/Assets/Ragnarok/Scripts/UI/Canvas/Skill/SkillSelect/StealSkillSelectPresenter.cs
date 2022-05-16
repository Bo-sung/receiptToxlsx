using Ragnarok.View;
using Ragnarok.View.Skill;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIStealSkillSelect"/>
    /// </summary>
    public sealed class StealSkillSelectPresenter : ViewPresenter
    {
        private readonly IStealSkillSelectCanvas canvas;
        private readonly JobDataManager jobDataRepo;
        private readonly SkillDataManager skillDataRepo;
        private readonly CharacterModel characterModel;
        private readonly Buffer<SkillData> skillBuffer;
        /******************** Pool ********************/
        private readonly Dictionary<int, SkillGroupInfo> skillGropuInfoDic;

        public StealSkillSelectPresenter(IStealSkillSelectCanvas canvas)
        {
            this.canvas = canvas;
            jobDataRepo = JobDataManager.Instance;
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
        /// 해당 직업 차수에 해당하는 직업 목록 반환 (본인 제외)
        /// </summary>
        public JobSelectView.IInfo[] GetJobInfos(int skillId)
        {
            SkillData skillData = skillDataRepo.Get(skillId, level: 1);
            int grade = GetJobGrade(skillData);
            return jobDataRepo.GetSkillIds(grade, characterModel.Job);
        }

        /// <summary>
        /// 해당 직업에 해당하는 스킬 목록 반환
        /// </summary>
        public UISkillInfoSelect.IInfo[] GetSkills(Job job)
        {
            JobData jobData = jobDataRepo.Get((int)job);

            if (jobData == null)
                return null;

            for (int i = 0; i < JobData.MAX_SKILL_COUNT; i++)
            {
                int skillId = jobData.GetSkillId(i);
                if (skillId == 0)
                    continue;

                SkillData skillData = skillDataRepo.Get(skillId, level: 1);
                if (skillData == null)
                    continue;

                skillBuffer.Add(skillData);
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

        private int GetJobGrade(UISkillInfoSelect.IInfo info)
        {
            if (info == null)
                return -1;

            switch (info.SkillType)
            {
                case SkillType.Plagiarism:
                    return 1; // 1차 스킬 훔쳐 배우기

                case SkillType.Reproduce:
                    return 3; // 3차 스킬 훔쳐 배우기

                default:
                    Debug.LogError($"[올바르지 않은 {nameof(info)}] {nameof(info)} = {info}");
                    break;
            }

            return -1;
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