namespace Ragnarok.View.Skill
{
    public class UISkillInfoSelect<T> : UISkillInfo<T>
        where T : UISkillInfoSelect.IInfo
    {
        public event UISkillInfoSelect.SelectEvent OnSelect;

        void OnClick()
        {
            if (info == null)
                return;

            OnSelect?.Invoke(info.SkillId);
        }
    }

    public class UISkillInfoSelect : UISkillInfoSelect<UISkillInfoSelect.IInfo>
    {
        public delegate void SelectEvent(int skillId);

        public interface IInfo : UISkillInfo.IInfo
        {
            int SkillId { get; }
        }
    }
}