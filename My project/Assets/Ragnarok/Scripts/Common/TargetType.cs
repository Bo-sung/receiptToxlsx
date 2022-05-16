namespace Ragnarok
{
    public enum TargetType
    {
        /// <summary>
        /// 아군
        /// </summary>
        Allies = 1,

        /// <summary>
        /// 아군 (캐릭터 only)
        /// </summary>
        AlliesCharacter = 2,

        /// <summary>
        /// 아군 (큐펫 only)
        /// </summary>
        AlliesCupet = 3,

        /// <summary>
        /// 적군
        /// </summary>
        Enemy = 4,

        /// <summary>
        /// 적군 (캐릭터 only)
        /// </summary>
        EnemyCharacter = 5,

        /// <summary>
        /// 적군 (큐펫 only)
        /// </summary>
        EnemyCupet = 6,
    }

    public static class TargetTypeExtension
    {
        public static string ToName(this TargetType targetType)
        {
            switch (targetType)
            {
                case TargetType.Allies:
                    return LocalizeKey._60000.ToText(); // 아군
                case TargetType.AlliesCharacter:
                    return LocalizeKey._60001.ToText(); // 자신
                case TargetType.AlliesCupet:
                    return LocalizeKey._60002.ToText(); // 큐펫
                case TargetType.Enemy:
                    return LocalizeKey._60003.ToText(); // 적군 전체
                case TargetType.EnemyCharacter:
                    return LocalizeKey._60004.ToText(); // 적군
                case TargetType.EnemyCupet:
                    return LocalizeKey._60005.ToText(); // 적군 큐펫
            }
            return default;
        }
    }

}