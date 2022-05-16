using UnityEngine;

namespace Ragnarok.View
{
    public abstract class GuildBattleBuffElement<T> : UIElement<T>
        where T : class, GuildBattleBuffElement.IInput
    {
        [SerializeField] UISkillInfo skillInfo;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelValue option;

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            skillInfo.Show(info.Skill);
            labelName.Text = info.SkillName;
            option.Title = info.OptionTitle;
            option.Value = info.OptionValue;
        }
    }

    public class GuildBattleBuffElement : GuildBattleBuffElement<GuildBattleBuffElement.IInput>
    {
        public interface IInput
        {
            int SkillId { get; }
            UISkillInfo.IInfo Skill { get; }
            string SkillName { get; }
            string OptionTitle { get; }
            string OptionValue { get; }
        }
    }
}