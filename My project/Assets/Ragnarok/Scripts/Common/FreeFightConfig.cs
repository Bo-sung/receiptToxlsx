namespace Ragnarok
{
    /// <summary>
    /// id (FreeFightEventType)
    /// name_id (이름)
    /// image_name (이미지이름)
    /// open_time_1 (오픈시간1)
    /// open_time_2 (오픈시간2)
    /// play_time (진행시간)
    /// round_count (라운드)
    /// scene_name (씬이름)
    /// bgm (bgm이름)
    /// skill_1 (스킬1)
    /// skill_2 (스킬2)
    /// skill_3 (스킬3)
    /// skill_4 (스킬4)
    /// trap_effect (장애물이펙트)
    /// crowd_control_type (장애물상태이상)
    /// </summary>
    public sealed class FreeFightConfig : EnumBaseType<FreeFightConfig, FreeFightEventType, int>
    {
        /// <summary>
        /// 난전
        /// </summary>
        public static readonly FreeFightConfig NORMAL = new FreeFightConfig(FreeFightEventType.Normal, LocalizeKey._58600, BasisType.FREE_FIGHT_OPEN_HOUR, BasisType.FF_PLAY_TIME, null, "UI_Texture_Brawl_Normal", ContentType.FreeFight);
        /// <summary>
        /// 이벤트난전-물폭탄
        /// </summary>
        public static readonly FreeFightConfig WATER_BOMB = new FreeFightConfig(FreeFightEventType.WaterBomb, LocalizeKey._58601, BasisType.EVENT_FREE_FIGHT_OPEN_HOUR, BasisType.EVENT_FF_PLAY_TIEM, new int[4] { 67026, 67027, 67028, 67029 }, "UI_Texture_Brawl_Water_Bomb");
        /// <summary>
        /// 이벤트난전-눈싸움
        /// </summary>
        public static readonly FreeFightConfig CHRISTMAS = new FreeFightConfig(FreeFightEventType.Christmas, LocalizeKey._58602, BasisType.EVENT_FREE_FIGHT_OPEN_HOUR, BasisType.EVENT_FF_PLAY_TIEM, new int[4] { 67021, 67022, 67023, 67024 });

        public readonly BasisType openTimeBasisType;
        public readonly BasisType playTimeBasisType;
        public readonly int[] useSkills;
        public readonly string imageName;
        public readonly ContentType openContentType;

        public int NameId => Value;

        public FreeFightConfig(FreeFightEventType key, int value, BasisType openTimeBasisType, BasisType playTimeBasisType, int[] useSkills, string imageName = null, ContentType openContentType = default)
            : base(key, value)
        {
            this.openTimeBasisType = openTimeBasisType;
            this.playTimeBasisType = playTimeBasisType;
            this.useSkills = useSkills;
            this.imageName = imageName;
            this.openContentType = openContentType;
        }

        public static FreeFightConfig GetByKey(FreeFightEventType key)
        {
            return GetBaseByKey(key);
        }
    }
}