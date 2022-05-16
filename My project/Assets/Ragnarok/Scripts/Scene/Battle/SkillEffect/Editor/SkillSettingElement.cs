namespace Ragnarok
{
    public sealed class SkillSettingElement : TreeElement
    {
        public readonly SkillSetting setting;

        /// <summary>
        /// Root
        /// </summary>
        public SkillSettingElement()
            : base(id: -1, depth: -1, name: string.Empty)
        {
        }

        public SkillSettingElement(SkillSetting setting)
            : base(id: setting.id, depth: 0, name: string.Concat(setting.name, setting.id)) // id와 name 모두 SearchField 검색되기 위함
        {
            this.setting = setting;
        }
    }
}