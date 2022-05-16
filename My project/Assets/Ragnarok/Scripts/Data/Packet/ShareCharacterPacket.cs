using Ragnarok.View.CharacterShare;
using UnityEngine;

namespace Ragnarok
{
    public class ShareCharacterPacket : IPacket<Response>, UISimpleCharacterShareBar.IInput, System.IComparable<ShareCharacterPacket>
    {
        public int Cid { get; private set; }
        public int Uid { get; private set; }
        public string Name { get; private set; }
        public int JobLevel { get; private set; }
        public byte Job { get; private set; }
        public int Power { get; private set; }
        private int[] skill_ids;
        public byte Gender { get; private set; }
        private int chapter;
        private int rank;
        private int profileId;
        public string ProfileName { get; private set; }
        public string ThumbnailName { get; private set; }

        // 클라이언트 전용
        private string[] skillIcons;
        private SkillType[] skillTypes;

        public SharingModel.SharingCharacterType SharingCharacterType => SharingModel.SharingCharacterType.Normal;
        public virtual SharingModel.CloneCharacterType CloneCharacterType => default;

        void IInitializable<Response>.Initialize(Response response)
        {
            Cid = response.GetInt("1");
            Uid = response.GetInt("2");
            Name = response.GetUtfString("3");
            JobLevel = response.GetShort("4");
            Job = response.GetByte("5");
            Power = response.GetInt("6");
            skill_ids = response.GetIntArray("7");
            Gender = response.GetByte("8");
            chapter = response.GetInt("9");
            rank = response.GetInt("10");
            profileId = response.GetInt("11");
        }

        public void Initialize(SkillDataManager.ISkillDataRepoImpl skillDataRepoImpl)
        {
            int skillSize = skill_ids == null ? 0 : skill_ids.Length;
            skillIcons = new string[skillSize];
            skillTypes = new SkillType[skillSize];

            for (int i = 0; i < skillSize; i++)
            {
                int skillId = skill_ids[i];
                int level = 1; // 스킬 레벨은 1로 고정
                SkillData skillData = skillDataRepoImpl.Get(skillId, level);
                if (skillData == null)
                {
                    skillIcons[i] = string.Empty;
                    skillTypes[i] = (SkillType)(-1);
#if UNITY_EDITOR
                    Debug.LogError($"skillData is Null: {nameof(Cid)} = {Cid}, {nameof(Uid)} = {Uid}, {nameof(Name)} = {Name}, {nameof(skillId)} = {skillId}");
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

        int System.IComparable<ShareCharacterPacket>.CompareTo(ShareCharacterPacket other)
        {
            int result = other.chapter.CompareTo(chapter); // 1순위: chapter 높은 순 (큰 챕터가 좋은것)
            return result == 0 ? rank.CompareTo(other.rank) : result; // 2순위: rank 낮은 순 (낮은 순위가 좋은거)
        }
    }
}