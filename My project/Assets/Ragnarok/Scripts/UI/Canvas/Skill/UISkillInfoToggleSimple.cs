using UnityEngine;

namespace Ragnarok.View.Skill
{
    public class UISkillInfoToggleSimple<T> : UISkillInfoSelect<T>
        where T : UISkillInfoSelect.IInfo
    {
        [SerializeField] GameObject select;

        public virtual bool SetSelect(int selectedSkillId)
        {
            if (info == null)
                return false;

            bool isSelect = info.SkillId == selectedSkillId;
            select.SetActive(isSelect);
            return isSelect;
        }
    }

    public class UISkillInfoToggleSimple : UISkillInfoToggleSimple<UISkillInfoSelect.IInfo>
    {
    }
}