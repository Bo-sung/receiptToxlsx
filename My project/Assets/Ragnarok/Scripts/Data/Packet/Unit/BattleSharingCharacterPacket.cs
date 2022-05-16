using UnityEngine;

namespace Ragnarok
{
    public class BattleSharingCharacterPacket : BattleCharacterPacket, IBattleSharingCharacter
    {
        public int Uid => uid;
        public string ProfileName { get; private set; }
        public string ThumbnailName { get; private set; }

        // 클라이언트 전용
        private string[] skillIcons;
        private SkillType[] skillTypes;

        public SharingModel.SharingCharacterType SharingCharacterType => SharingModel.SharingCharacterType.Normal;
        public virtual SharingModel.CloneCharacterType CloneCharacterType => default;

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

        public void Initialize(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl)
        {
            ProfileName = GetProfileName(profileDataRepoImpl);
            ThumbnailName = GetThumbnailName(profileDataRepoImpl);
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

        private string GetProfileName(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl)
        {
            if (profileId > 0)
            {
                ProfileData profileData = profileDataRepoImpl.Get(profileId);
                if (profileData != null)
                    return profileData.ProfileName;
            }

            return Job.ToEnum<Job>().GetJobProfile(Gender.ToEnum<Gender>());
        }

        private string GetThumbnailName(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl)
        {
            if (profileId > 0)
            {
                ProfileData profileData = profileDataRepoImpl.Get(profileId);
                if (profileData != null)
                    return profileData.ThumbnailName;
            }

            return Job.ToEnum<Job>().GetThumbnailName(Gender.ToEnum<Gender>());
        }

        public override DamagePacket.UnitKey GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.SharingCharacter, cid, job_level);
        }
    }

    public class BattleCloneCharacterPacket_MyCharacter : BattleSharingCharacterPacket
    {
        public override SharingModel.CloneCharacterType CloneCharacterType => SharingModel.CloneCharacterType.MyCharacter;
    }

    public class BattleCloneCharacterPacket_GuildCharacter : BattleSharingCharacterPacket
    {
        public override SharingModel.CloneCharacterType CloneCharacterType => SharingModel.CloneCharacterType.GuildCharacter;
    }

    public class BattleCloneCharacterPacket_Friend : BattleSharingCharacterPacket
    {
        public override SharingModel.CloneCharacterType CloneCharacterType => SharingModel.CloneCharacterType.Friend;
    }
}