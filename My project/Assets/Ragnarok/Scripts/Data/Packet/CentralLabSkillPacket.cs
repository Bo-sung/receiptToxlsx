using Ragnarok.View;
using Ragnarok.View.Skill;
using UnityEngine;

namespace Ragnarok
{
    public class CentralLabSkillPacket : IPacket<Response>, UISkillLevelInfoSelect.IInfo, SkillModel.ISkillValue, SkillModel.ISlotValue
    {
        public int skillId;
        public int skillLevel;
        public bool isMaxLevel;

        private SkillData skillData;

        void IInitializable<Response>.Initialize(Response response)
        {
            skillId = response.GetInt("1");
            skillLevel = response.GetInt("2");
            isMaxLevel = response.GetBool("3");
        }

        public void Initialize(SkillDataManager.ISkillDataRepoImpl skillDataRepoImpl)
        {
            skillData = skillDataRepoImpl.Get(skillId, skillLevel);

            if (skillData == null)
                Debug.LogError($"존재하지 않는 스킬데이터: {nameof(skillId)} = {skillId}, {nameof(skillLevel)} = {skillLevel}");
        }

        int UISkillInfoSelect.IInfo.SkillId => skillData.id;
        int UISkillLevelInfoSelect.IInfo.SkillLevel => skillData.lv;
        bool UISkillLevelInfoSelect.IInfo.IsMaxLevel => isMaxLevel;
        string UISkillInfo.IInfo.SkillIcon => skillData.icon_name;
        SkillType UISkillInfo.IInfo.SkillType => skillData.skill_type.ToEnum<SkillType>();

        bool SkillModel.ISkillValue.IsInPossession => true;
        long SkillModel.ISkillValue.SkillNo => skillData.id;
        int SkillModel.ISkillValue.SkillId => skillData.id;
        int SkillModel.ISkillValue.SkillLevel => skillData.lv;
        int SkillModel.ISkillValue.OrderId => 0;
        int SkillModel.ISkillValue.ChangeSkillId => 0;

        long SkillModel.ISlotValue.SlotNo => skillData.id;
        long SkillModel.ISlotValue.SkillNo => skillData.id;
        int SkillModel.ISlotValue.SlotIndex => 0;
        bool SkillModel.ISlotValue.IsAutoSkill => true;

        bool UISkillInfo.IInfo.IsAvailableWeapon(EquipmentClassType weaponType)
        {
            return skillData.IsAvailableWeapon(weaponType);
        }
    }
}