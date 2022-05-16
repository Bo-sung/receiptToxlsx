using System;

namespace Ragnarok
{
    /// <summary>
    /// 파견 타입
    /// </summary>
    [Flags]
    public enum ExploreType
    {
        None = 0,
        /// <summary>
        /// 교역
        /// </summary>
        Trade = 1 << 0, // 1

        /// <summary>
        /// 채집
        /// </summary>
        Collect = 1 << 1, // 2

        /// <summary>
        /// 발굴
        /// </summary>
        Dig = 1 << 2, // 4

        /// <summary>
        /// 생상
        /// </summary>
        Production = 1 << 3, // 8
    }

    public static class ExploreTypeExtension
    {
        public static string ToSpriteName(this ExploreType type)
        {
            switch (type)
            {
                case ExploreType.Trade:
                    return "Ui_Common_BG_Trade";

                case ExploreType.Collect:
                    return "Ui_Common_BG_Collect";

                case ExploreType.Dig:
                    return "Ui_Common_BG_Dig";

                case ExploreType.Production:
                    return "Ui_Common_BG_Production";

                default:
                    return string.Empty;
            }
        }

        public static string ToExploreName(this ExploreType type)
        {
            switch (type)
            {
                case ExploreType.Trade:
                    return LocalizeKey._47300.ToText(); // 교역

                case ExploreType.Collect:
                    return LocalizeKey._47301.ToText(); // 채집

                case ExploreType.Dig:
                    return LocalizeKey._47302.ToText(); // 발굴   

                case ExploreType.Production:
                    return LocalizeKey._47349.ToText(); // 생산

                default:
                    return string.Empty;
            }
        }

        public static string ToExploreLoadingName(this ExploreType type)
        {
            switch (type)
            {
                case ExploreType.Trade:
                    return LocalizeKey._47343.ToText(); // 교역 중...

                case ExploreType.Collect:
                    return LocalizeKey._47344.ToText(); // 채집 중...

                case ExploreType.Dig:
                    return LocalizeKey._47345.ToText(); // 발굴 중...

                case ExploreType.Production:
                    return LocalizeKey._47350.ToText(); // 생산 중...

                default:
                    return string.Empty;
            }
        }
    }
}