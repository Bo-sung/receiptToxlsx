using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class BattlePassiveBuffSkillInfo : List<BattleOption>
    {
        private readonly SkillDataManager skillDataRepo;
        private readonly BetterList<SkillInfo> buffSkillList;

        public BattlePassiveBuffSkillInfo()
        {
            skillDataRepo = SkillDataManager.Instance;
            buffSkillList = new BetterList<SkillInfo>();
        }

        /// <summary>
        /// 정보 리셋
        /// </summary>
        public new void Clear()
        {
            base.Clear();

            buffSkillList.Release();
        }

        public void Initialize(ISkillDataKey[] buffSkills)
        {
            buffSkillList.Clear();

            if (buffSkills == null)
                return;

            foreach (var item in buffSkills)
            {
                AddBuff(item);
            }
        }

        /// <summary>
        /// 버프 스킬 추가 (only 패시브스킬)
        /// </summary>
        private void AddBuff(ISkillDataKey buffSkill)
        {
            if (buffSkill == null)
                return;

            AddBuff(buffSkill.Id, buffSkill.Level);
        }

        /// <summary>
        /// 버프 스킬 추가 (only 패시브스킬)
        /// </summary>
        private void AddBuff(int skillId, int skillLevel)
        {
            if (skillId == 0 || skillLevel == 0)
                return;

            SkillData skillData = skillDataRepo.Get(skillId, skillLevel);
            if (skillData == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"스킬 데이터가 존재하지 않습니다: {nameof(skillId)} = {skillId}, {nameof(skillLevel)} = {skillLevel}");
#endif
                return;
            }

            SkillType skillType = skillData.skill_type.ToEnum<SkillType>();
            switch (skillType)
            {
                case SkillType.Plagiarism:
                case SkillType.Reproduce:
                case SkillType.SummonBall:
                case SkillType.Passive:
                case SkillType.RuneMastery:
                    PassiveSkill skillInfo = new PassiveSkill();
                    skillInfo.SetIsInPossession();
                    skillInfo.SetData(skillData);
                    buffSkillList.Add(skillInfo);

                    AddRange(skillInfo); // 버프 스킬 적용
                    break;

                default:
                case SkillType.Active:
                case SkillType.BasicActiveSkill:
#if UNITY_EDITOR
                    Debug.LogError($"{skillType} 타입의 버프: {nameof(skillId)} = {skillId}, {nameof(skillLevel)} = {skillLevel}");
#endif
                    return;
            }
        }

        /// <summary>
        /// 버프 스킬 반환
        /// </summary>
        public SkillInfo[] GetBuffSkills()
        {
            return buffSkillList.ToArray();
        }
    }
}