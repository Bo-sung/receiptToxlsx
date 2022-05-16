namespace Ragnarok
{
    /// <summary>
    /// 보유 스킬 정보 (패시브 포함)
    /// <see cref="BattleCharacterPacket"/>
    /// </summary>
    public class SkillSimpleValuePacket : IPacket<Response>, SkillModel.ISkillSimpleValue
    {
        public int skill_id;
        public int skill_level;

        int SkillModel.ISkillSimpleValue.SkillId => skill_id;
        int SkillModel.ISkillSimpleValue.SkillLevel => skill_level;
        SkillType SkillModel.ISkillSimpleValue.SkillType => default;
        string SkillModel.ISkillSimpleValue.GetIconName() => SkillDataManager.Instance.Get(skill_id, skill_level).icon_name;

        bool IInfo.IsInvalidData => false;
        event System.Action IInfo.OnUpdateEvent { add { } remove { } }

        void IInitializable<Response>.Initialize(Response response)
        {
            skill_id = response.GetInt("1");
            skill_level = response.GetInt("2");
        }
    }
}