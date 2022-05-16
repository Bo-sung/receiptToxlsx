using UnityEngine;

namespace Ragnarok.View.Skill
{
    public class UISkillLevelInfoSelect<T> : UISkillInfoSelect<T>
        where T : UISkillLevelInfoSelect.IInfo
    {
        [SerializeField] UISkillLevel skillLevel;

        protected override void Refresh(bool isAsync)
        {
            base.Refresh(isAsync);

            skillLevel.SetData(info);
        }
    }

    public class UISkillLevelInfoSelect : UISkillLevelInfoSelect<UISkillLevelInfoSelect.IInfo>
    {
        public interface IInfo : UISkillInfoSelect.IInfo
        {
            int SkillLevel { get; }
            bool IsMaxLevel { get; }
        }
    }
}