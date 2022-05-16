using Ragnarok.View.Skill;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UISkillTooltip"/>
    /// </summary>
    public interface ISkillTooltipCanvas : ICanvas
        , ISkillViewListener
    {
        /// <summary>
        /// 새로고침
        /// </summary>
        void Refresh();
    }
}