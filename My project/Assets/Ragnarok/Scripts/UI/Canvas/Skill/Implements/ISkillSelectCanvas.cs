using Ragnarok.View.Skill;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UISkillSelect"/>
    /// </summary>
    public interface ISkillSelectCanvas : ICanvas
        , SkillSelectView.IListener
        , ISkillViewListener
    {
        /// <summary>
        /// 새로고침
        /// </summary>
        void Refresh();
    }
}