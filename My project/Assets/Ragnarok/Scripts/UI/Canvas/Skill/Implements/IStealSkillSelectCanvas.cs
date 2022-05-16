using Ragnarok.View;
using Ragnarok.View.Skill;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIStealSkillSelect"/>
    /// </summary>
    public interface IStealSkillSelectCanvas : ICanvas
        , JobSelectView.IListener
        , SkillSelectView.IListener
        , ISkillViewListener
    {
        /// <summary>
        /// 새로고침
        /// </summary>
        void Refresh();
    }
}