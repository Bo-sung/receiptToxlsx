using UnityEngine;

namespace Ragnarok.View
{
    public class SkillFreeFightHelpView : SimpleFreeFightHelpView
    {
        [SerializeField] UIEventMazeSkill[] skills;

        /// <summary>
        /// 초기화
        /// </summary>
        public void InitializeSkill(UIEventMazeSkill.IInput[] arrSkill)
        {
            // Skills
            int skillLength = arrSkill == null ? 0 : arrSkill.Length;
            for (int i = 0; i < skills.Length; i++)
            {
                skills[i].SetData(i < skillLength ? arrSkill[i] : null);
            }
        }

        public override bool Find()
        {
            base.Find();

            skills = GetComponentsInChildren<UIEventMazeSkill>();
            return true;
        }
    }
}