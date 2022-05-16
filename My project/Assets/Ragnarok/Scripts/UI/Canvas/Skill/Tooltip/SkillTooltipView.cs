using UnityEngine;

namespace Ragnarok.View.Skill
{
    public class SkillTooltipView : BaseSkillInfoView<ISkillViewListener>, IAutoInspectorFinder
    {
        [SerializeField] UITextureHelper skillIcon;
        [SerializeField] UISprite skillTypeIcon;

        protected override void RefreshLevel()
        {
            base.RefreshLevel();
            skillIcon.Set(currentData.SkillIcon);

            if (skillTypeIcon != null)
                skillTypeIcon.spriteName = currentData.SkillType.GetIconName();
        }
    }
}