using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

namespace Ragnarok
{
    public enum UnitSizeType
    {
        /// <summary>
        /// 음슴 (몬스터 타입이 아닐 경우)
        /// </summary>
        None = 0,

        /// <summary>
        /// 소형
        /// </summary>
        Small = 1,

        /// <summary>
        /// 중형
        /// </summary>
        Medium = 2,

        /// <summary>
        /// 대형
        /// </summary>
        Large = 3,
    }

    public static class UnitSizeTypeExtensions
    {
        public static UnitSizeType ToUnitSizeType(this ObscuredInt cost)
        {
            if (cost == 1)
                return UnitSizeType.Small;

            if (cost == 2)
                return UnitSizeType.Medium;

            if (cost > 2)
                return UnitSizeType.Large;

            return default;
        }

        public static string ToText(this UnitSizeType type)
        {
            switch (type)
            {
                case UnitSizeType.Small:
                    return LocalizeKey._51000.ToText(); // 소형

                case UnitSizeType.Medium:
                    return LocalizeKey._51001.ToText(); // 중형

                case UnitSizeType.Large:
                    return LocalizeKey._51002.ToText(); // 대형

                default:
                    Debug.LogError($"[올바르지 않은 {nameof(UnitSizeType)}] {nameof(type)} = {type}");
                    return string.Empty;
            }
        }

        public static string ToSizeName(this UnitSizeType type)
        {
            switch (type)
            {
                case UnitSizeType.Small:
                    return LocalizeKey._51500.ToText(); // S

                case UnitSizeType.Medium:
                    return LocalizeKey._51501.ToText(); // M

                case UnitSizeType.Large:
                    return LocalizeKey._51502.ToText(); // L

                default:
                    Debug.LogError($"[올바르지 않은 {nameof(UnitSizeType)}] {nameof(type)} = {type}");
                    return string.Empty;
            }
        }
    }
}