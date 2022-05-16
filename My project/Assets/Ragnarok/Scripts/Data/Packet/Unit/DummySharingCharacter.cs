using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

namespace Ragnarok
{
    public class DummySharingCharacter : AgentCharacterPacket, IBattleSharingCharacter
    {
        public readonly int agentId;
        public readonly ObscuredInt chapter;
        private readonly string name;
        private readonly ObscuredInt battleScore;

        /// <summary>
        /// AgentId 의 마이너스 값
        /// 동일한 CId의 경우에는 BattleManager Add 추가가 되지 않음
        /// </summary>
        public override int Cid => -agentId;
        public override string Name => name;
        public int Uid => 0;
        public int Power => battleScore;
        public SharingModel.SharingCharacterType SharingCharacterType => SharingModel.SharingCharacterType.Dummy;
        public SharingModel.CloneCharacterType CloneCharacterType => default;
        public string ProfileName => GetProfileName();
        public string ThumbnailName => GetThumbnailName();

        // 클라이언트 전용
        private string[] skillIcons;
        private SkillType[] skillTypes;

        public DummySharingCharacter(AgentData agentData) : base(agentData)
        {
            agentId = agentData.id;
            chapter = agentData.dummy_chapter;
            battleScore = agentData.dummy_battle_score;

#if UNITY_EDITOR
            name = string.Concat("[", agentId, "] chapter:", chapter);
#else
            name = FilterUtils.GetAutoNickname(GameServerConfig.CountryDefaultLanguage);
#endif
        }

        public void Initialize(SkillDataManager.ISkillDataRepoImpl skillDataRepoImpl)
        {
            int skillSize = Skills == null ? 0 : Skills.Length;
            skillIcons = new string[skillSize];
            skillTypes = new SkillType[skillSize];

            for (int i = 0; i < skillSize; i++)
            {
                int skillId = Skills[i].SkillId;
                int skillLevel = Skills[i].SkillLevel;
                SkillData skillData = skillDataRepoImpl.Get(skillId, skillLevel);
                if (skillData == null)
                {
                    skillIcons[i] = string.Empty;
                    skillTypes[i] = (SkillType)(-1);
#if UNITY_EDITOR
                    Debug.LogError($"skillData is Null: {nameof(Cid)} = {Cid}, {nameof(Name)} = {Name}, {nameof(skillId)} = {skillId}, {nameof(skillLevel)} = {skillLevel}");
#endif
                }
                else
                {
                    skillIcons[i] = skillData.icon_name;
                    skillTypes[i] = skillData.skill_type.ToEnum<SkillType>();
                }
            }
        }

        public string GetSkillIcon(int index)
        {
            if (skillIcons == null)
                return string.Empty;

            if (index < 0 || index >= skillIcons.Length)
                return string.Empty;

            return skillIcons[index];
        }

        public SkillType GetSkillType(int index)
        {
            if (skillTypes == null)
                return default;

            if (index < 0 || index >= skillIcons.Length)
                return default;

            return skillTypes[index];
        }

        private string GetProfileName()
        {
            return Job.ToEnum<Job>().GetJobProfile(Gender.ToEnum<Gender>());
        }

        private string GetThumbnailName()
        {
            return Job.ToEnum<Job>().GetThumbnailName(Gender.ToEnum<Gender>());
        }

        public override DamagePacket.UnitKey GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.DummySharingCharacter, agentId, jobLevel);
        }
    }
}