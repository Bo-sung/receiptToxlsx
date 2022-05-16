using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 스킬 사용 시 말풍선
    /// </summary>
    public class SkillImageBalloon : SkillBalloon, IAutoInspectorFinder
    {
        [SerializeField] UITextureHelper texture;

        public override void Set(Mode mode, string iconName)
        {
            base.Set(mode, iconName);

            texture.SetSkill(iconName);
        }
    }
}