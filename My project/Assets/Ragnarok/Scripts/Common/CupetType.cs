using UnityEngine;

namespace Ragnarok
{
    public enum CupetType
    {
        /// <summary>
        /// 돌진형 (물리)
        /// </summary>
        Rush = 1,

        /// <summary>
        /// 돌진형 (마법)
        /// </summary>
        MRush,

        /// <summary>
        /// 공격형 (물리)
        /// </summary>
        Damage,

        /// <summary>
        /// 공격형 (마법)
        /// </summary>
        MDamage,
        
        /// <summary>
        /// 방어형 (물리)
        /// </summary>
        Defense,

        /// <summary>
        /// 방어형 (마법)
        /// </summary>
        MDefense,

        /// <summary>
        /// 지원형 (물리)
        /// </summary>
        Support,

        /// <summary>
        /// 지원형 (마법)
        /// </summary>
        MSupport
    }

    public static class CupetTypeExtensions
    {
        public static string GetIconName(this CupetType type)
        {
            if (type == default)
                return string.Empty;

            return string.Concat("CupetType", type.ToString());
        }

        public static string ToText(this CupetType type)
        {
            switch (type)
            {
                case CupetType.Rush:
                    return LocalizeKey._55000.ToText(); // 물리 돌진형

                case CupetType.MRush:
                    return LocalizeKey._55001.ToText(); // 마법 돌진형

                case CupetType.Damage:
                    return LocalizeKey._55002.ToText(); // 물리 공격형

                case CupetType.MDamage:
                    return LocalizeKey._55003.ToText(); // 마법 공격형

                case CupetType.Defense:
                    return LocalizeKey._55004.ToText(); // 물리 방어형

                case CupetType.MDefense:
                    return LocalizeKey._55005.ToText(); // 마법 방어형

                case CupetType.Support:
                    return LocalizeKey._55006.ToText(); // 물리 지원형

                case CupetType.MSupport:
                    return LocalizeKey._55007.ToText(); // 마법 지원형

                default:
                    Debug.LogError($"[올바르지 않은 {nameof(ElementType)}] {nameof(type)} = {type}");
                    return string.Empty;
            }
        }
    }
}