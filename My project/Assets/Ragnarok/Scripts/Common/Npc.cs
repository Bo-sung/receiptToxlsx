using UnityEngine;

namespace Ragnarok
{
    public sealed class Npc : EnumBaseType<Npc, NpcType, string>
    {
        private enum SignColor
        {
            Blue = 1,
            Yellow,
        }

        // <!-- 대화 --!>
        public static readonly Npc HOLORUCHI     = new Npc(NpcType.Holoruchi, "홀로루치", LocalizeKey._26033, "NPC_01");
        public static readonly Npc DEVIRUCHI     = new Npc(NpcType.Deviruchi, "데비루치", LocalizeKey._26000, "NPC_02", string.Empty, "NpcDeviruchi", 0, default);
        public static readonly Npc NOVICE_MALE   = new Npc(NpcType.NoviceMale, "노비스-남", 0, "NPC_03");
        public static readonly Npc NOVICE_FEMALE = new Npc(NpcType.NoviceFemale, "노비스-여", 0, "NPC_04");
        public static readonly Npc PRUIT         = new Npc(NpcType.Pruit, "프룻", LocalizeKey._82002, "NPC_05");

        // <!-- 길드 로비 --!>
        public static readonly Npc W             = new Npc(NpcType.W, "더블류", LocalizeKey._38009, string.Empty, "Ui_NpcSign_Shop", "W", LocalizeKey._38004, SignColor.Blue);
        public static readonly Npc TAMAMI        = new Npc(NpcType.Tamami, "타마미", LocalizeKey._38010, string.Empty, "Ui_NpcSign_Taming", "Tamami", LocalizeKey._38005, SignColor.Blue);
        public static readonly Npc SPECIAL_AGENT = new Npc(NpcType.SpecialAgent, "특수요원", LocalizeKey._38011, string.Empty, "Ui_NpcSign_Raid", "SpecialAgent", LocalizeKey._38006, SignColor.Blue);
        public static readonly Npc SECRET_AGENT  = new Npc(NpcType.SecretAgent, "비밀요원", LocalizeKey._38012, string.Empty, "Ui_NpcSign_Cooperation", "SecretAgent", LocalizeKey._38007, SignColor.Blue);
        public static readonly Npc GUARD         = new Npc(NpcType.Guard, "경비대원", LocalizeKey._38013, string.Empty, "Ui_NpcSign_War", "Guard", LocalizeKey._38008, SignColor.Blue);
        public static readonly Npc Emperium      = new Npc(NpcType.Emperium, "엠펠리움", LocalizeKey._38015, string.Empty, "Ui_NpcSign_GuildAttack", "Emperium", LocalizeKey._38014, SignColor.Yellow);
        public static readonly Npc TAILING       = new Npc(NpcType.Tailing, "테일링", LocalizeKey._38018, string.Empty, "Ui_NpcSign_Exchange", "Tailing", LocalizeKey._38019, SignColor.Blue);
        public static readonly Npc SORIN         = new Npc(NpcType.Sorin, "소린", LocalizeKey._38020, "NPC_07", "Ui_NpcSign_Sorin", "Sorin", LocalizeKey._38021, SignColor.Blue);
        public static readonly Npc SORTIE        = new Npc(NpcType.Sortie, "소티", LocalizeKey._38022, "NPC_08", "Ui_NpcSign_Sortie", "Sortie", LocalizeKey._38023, SignColor.Blue);
        public static readonly Npc NYANKUN       = new Npc(NpcType.Nyankun, "냥쿤", LocalizeKey._38024, string.Empty, "Ui_NpcSign_Nyankun", "Nyankun", LocalizeKey._38025, SignColor.Blue);

        // <!-- 타임패트롤 --!>
        public static readonly Npc CLOCK_TOWER   = new Npc(NpcType.ClockTower, "시계탑 관리자", LocalizeKey._38017, "NPC_06", "Ui_NpcSign_ClockTower", "ClockTower", LocalizeKey._38017, SignColor.Yellow);

        // <!-- BG_COLOR --!>
        private readonly static Color COLOR_BG_BLUE = new Color32(0xBB, 0xDE, 0xFF, 0xFF); // BBDEFFFF
        private readonly static Color COLOR_LABEL_BLUE = new Color32(0x26, 0x66, 0xFF, 0xFF); // 2666FFFF

        // <!-- Yellow --!>
        private readonly static Color COLOR_BG_YELLOW = new Color32(0xFF, 0xDC, 0x63, 0xFF); // FFDC63FF
        private readonly static Color COLOR_LABEL_YELLOW = new Color32(0x54, 0x2D, 0x00, 0xFF); // 542d00FF

        public readonly int nameLocalKey;
        public readonly string imageName;
        public readonly string spriteName;
        public readonly string prefabName;
        public readonly int contentLocalKey;
        private readonly SignColor signColor;

        /// <summary>
        /// 대화 전용 (imageName : texture)
        /// </summary>
        private Npc(NpcType key, string value, int nameLocalKey, string imageName)
            : this(key, value, nameLocalKey, imageName, string.Empty, "Empty", 0, default)
        {

        }

        /// <summary>
        /// [길드 로비, 타임패트롤] (imageName : texture, imageName : sprite)
        /// </summary>
        private Npc(NpcType key, string value, int nameLocalKey, string imageName, string spriteName, string prefabName, int contentLocalKey, SignColor signColor)
            : base(key, value)
        {
            this.nameLocalKey = nameLocalKey;
            this.imageName = imageName;
            this.spriteName = spriteName;
            this.prefabName = prefabName;
            this.contentLocalKey = contentLocalKey;
            this.signColor = signColor;
        }

        public Color GetBackgroundColor()
        {
            switch (signColor)
            {
                case SignColor.Blue:
                    return COLOR_BG_BLUE;

                case SignColor.Yellow:
                    return COLOR_BG_YELLOW;
            }

            return Color.white;
        }

        public Color GetLabelColor()
        {
            switch (signColor)
            {
                case SignColor.Blue:
                    return COLOR_LABEL_BLUE;

                case SignColor.Yellow:
                    return COLOR_LABEL_YELLOW;
            }

            return Color.black;
        }

        public static Npc GetByKey(NpcType key)
        {
            return GetBaseByKey(key);
        }
    }
}