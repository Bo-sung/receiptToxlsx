using Ragnarok.View;

namespace Ragnarok
{
    public sealed class ForestMazeSkill : EnumBaseType<ForestMazeSkill>, UISkillInfo.IInfo
    {
        public static readonly ForestMazeSkill SKILL_1 = new ForestMazeSkill(LocalizeKey._58700, "초승달반지", LocalizeKey._58701, LocalizeKey._58702, "ForestMazeSkill01", SkillType.Passive, "UI_Texture_Forest_Maze_01");
        public static readonly ForestMazeSkill SKILL_2 = new ForestMazeSkill(LocalizeKey._58703, "마제스틱 고우트", LocalizeKey._58704, LocalizeKey._58705, "ForestMazeSkill02", SkillType.Passive, "UI_Texture_Forest_Maze_02");
        public static readonly ForestMazeSkill SKILL_3 = new ForestMazeSkill(LocalizeKey._58706, "이그드라실 씨앗", LocalizeKey._58707, LocalizeKey._58708, "ForestMazeSkill03", SkillType.Active, "UI_Texture_Forest_Maze_03");
        public static readonly ForestMazeSkill SKILL_4 = new ForestMazeSkill(LocalizeKey._58709, "악마의 뿔", LocalizeKey._58710, LocalizeKey._58711, "ForestMazeSkill04", SkillType.Passive, "UI_Texture_Forest_Maze_04");

        public int NameId { get; }
        public int DescId { get; }
        public int MessageId { get; }
        public string SkillIcon { get; private set; }
        public SkillType SkillType { get; private set; }
        public string DungeonSkillIcon { get; private set; }

        public ForestMazeSkill(int key, string value, int descId, int messageId, string skillIcon, SkillType skillType, string dungeonSkillIcon) : base(key, value)
        {
            NameId = key;
            DescId = descId;
            MessageId = messageId;
            SkillIcon = skillIcon;
            SkillType = skillType;
            DungeonSkillIcon = dungeonSkillIcon;
        }

        public bool IsAvailableWeapon(EquipmentClassType weaponType)
        {
            return true; // 무조건 사용 가능
        }
    }
}