namespace Ragnarok
{
    /// <summary>
    /// 타임패틑롤 존 타입
    /// </summary>
    public enum TImePatrolZoneType
    {
        ZoneA = 1,
        ZoneB = 2,
        ZoneC = 3,
        ZoneD = 4,
        ZoneE = 5,
        ZoneF = 6,
    }

    public static class TImePatrolZoneTypeExtension
    {
        /// <summary>
        /// 존 이름
        /// </summary>
        public static string GetName(this TImePatrolZoneType type)
        {
            switch (type)
            {
                case TImePatrolZoneType.ZoneA:
                    return LocalizeKey._48243.ToText(); // A

                case TImePatrolZoneType.ZoneB:
                    return LocalizeKey._48244.ToText(); // B

                case TImePatrolZoneType.ZoneC:
                    return LocalizeKey._48245.ToText(); // C

                case TImePatrolZoneType.ZoneD:
                    return LocalizeKey._48246.ToText(); // D

                case TImePatrolZoneType.ZoneE:
                    return LocalizeKey._48247.ToText(); // E

                case TImePatrolZoneType.ZoneF:
                    return LocalizeKey._48248.ToText(); // F

                default:
                    return default;
            }
        }
    }
}
