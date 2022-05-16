using UnityEngine;

namespace Ragnarok
{
    public class SlotMachineSpinSlot : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UITextureHelper skillIcon;
        [SerializeField] UITextureHelper skillTypeIcon;

        public void SetData(SkillData skillData)
        {
            skillIcon.Set(skillData.icon_name);
            SkillType skillType = skillData.skill_type.ToEnum<SkillType>();
            skillTypeIcon.Set(skillType.GetIconName());
        }
    }
}