using Ragnarok.View.CharacterShare;
using System;
using UnityEngine;

namespace Ragnarok
{
    public class SimpleCharacterPacket : IPacket<Response>, UISimpleCharacterShareBar.IInput
    {
        public int Uid { get; private set; }
        public int Cid { get; private set; }
        public string Name { get; private set; }
        public byte Job { get; private set; }
        public byte Gender { get; private set; }
        public short Level { get; private set; }
        public int JobLevel { get; private set; }        

        public bool IsDeleteWaiting { get; private set; }
        public RemainTime RemainTimeDeleteWaiting { get; private set; }  // 삭제가능까지 남은시간

        public int WeaponItemId { get; private set; } // 장착 중인 무기ID
        public bool IsGuild { get; private set; } // 길드 가입 여부
        public int Power { get; private set; } // 전투력
        public long DisconnectTime { get; private set; }  // 접속 종료한 시간 (밀리세컨드)
        public ItemInfo.IEquippedItemValue[] EquippedItems { get; private set; } // 장착 코스튬 정보 목록
        public int[] SkillIds { get; private set; } // 스킬 정보
        public int ProfileId { get; private set; } // 프로필 ID

        public string[] skillIcons { get; private set; }
        public SkillType[] skillTypes { get; private set; }

        public string ProfileName { get; private set; }
        public string ThumbnailName { get; private set; }
        public SharingModel.SharingCharacterType SharingCharacterType => default;
        public SharingModel.CloneCharacterType CloneCharacterType => SharingModel.CloneCharacterType.MyCharacter;

        public bool IsShare { get; private set; } // 쉐어 중인지 여부

        public void Initialize(Response response)
        {
            Cid = response.GetInt("1");
            Name = response.GetUtfString("2");
            Job = response.GetByte("3");
            Gender = response.GetByte("4");
            Level = response.GetShort("5");
            JobLevel = response.GetShort("10");

            long remainTime = response.GetLong("11"); // -1:정상캐릭, 0: 삭제 가능, 0이상 삭제가능까지 남은시간
            IsDeleteWaiting = remainTime != -1;
            RemainTimeDeleteWaiting = remainTime;

            WeaponItemId = response.GetInt("13");
            IsGuild = response.GetInt("15") != 0;
            Power = response.GetInt("16");
            DisconnectTime = response.GetLong("17");

            // 장착중인 아이템 정보
            string equippedItemIds = response.ContainsKey("18") ? response.GetUtfString("18") : string.Empty;
            string[] arrayEquippedItemIds = StringUtils.Split(equippedItemIds, StringUtils.SplitType.Comma);
            EquippedItems = new EquipmentValuePacket[arrayEquippedItemIds.Length];
            for (int i = 0; i < EquippedItems.Length; i++)
            {
                EquippedItems[i] = new EquipmentValuePacket(int.Parse(arrayEquippedItemIds[i]));
            }

            // 스킬 정보
            string skillIds = response.ContainsKey("19") ? response.GetUtfString("19") : string.Empty;
            string[] arraySkillIds = StringUtils.Split(skillIds, StringUtils.SplitType.Comma);
            SkillIds = Array.ConvertAll(arraySkillIds, a => int.Parse(a));

            ProfileId = response.GetInt("20");
            IsShare = response.GetBool("21");
        }

        public void Initialize(int uid)
        {
            Uid = uid;
        }

        public void Initialize(SkillDataManager.ISkillDataRepoImpl skillDataRepoImpl)
        {
            int skillSize = SkillIds == null ? 0 : SkillIds.Length;
            skillIcons = new string[skillSize];
            skillTypes = new SkillType[skillSize];

            for (int i = 0; i < skillSize; i++)
            {
                int skillId = SkillIds[i];
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
            if (ProfileId > 0)
            {
                ProfileData profileData = profileDataRepoImpl.Get(ProfileId);
                if (profileData != null)
                    return profileData.ProfileName;
            }

            return Job.ToEnum<Job>().GetJobProfile(Gender.ToEnum<Gender>());
        }

        private string GetThumbnailName(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl)
        {
            if (ProfileId > 0)
            {
                ProfileData profileData = profileDataRepoImpl.Get(ProfileId);
                if (profileData != null)
                    return profileData.ThumbnailName;
            }

            return Job.ToEnum<Job>().GetThumbnailName(Gender.ToEnum<Gender>());
        }

        public void SetIsDeleteWaiting(bool isDeleteWaiting)
        {
            IsDeleteWaiting = isDeleteWaiting;
        }
    }
}