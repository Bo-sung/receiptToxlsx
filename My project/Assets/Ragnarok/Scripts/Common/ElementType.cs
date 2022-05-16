using UnityEngine;

namespace Ragnarok
{
    public enum ElementType
    {
        /// <summary>
        /// 속성 없음 (속성이 아예 음슴)
        /// </summary>
        None = 0,

        /// <summary>
        /// 무속성
        /// </summary>
        Neutral = 1,

        /// <summary>
        /// 화속성
        /// </summary>
        Fire = 2,

        /// <summary>
        /// 수속성
        /// </summary>
        Water = 3,

        /// <summary>
        /// 풍속성
        /// </summary>
        Wind = 4,

        /// <summary>
        /// 지속성
        /// </summary>
        Earth = 5,

        /// <summary>
        /// 독속성
        /// </summary>
        Poison = 6,

        /// <summary>
        /// 성속성
        /// </summary>
        Holy = 7,

        /// <summary>
        /// 암속성
        /// </summary>
        Shadow = 8,

        /// <summary>
        /// 염속성
        /// </summary>
        Ghost = 9,

        /// <summary>
        /// 사속성
        /// </summary>
        Undead = 10,
    }

    public static class ElementTypeExtensions
    {
        public static string GetIconName(this ElementType type)
        {
            return type.ToString();
        }

        public static string ToText(this ElementType type)
        {
            switch (type)
            {
                case ElementType.None:
                    return string.Empty;

                case ElementType.Neutral:
                    return LocalizeKey._50000.ToText(); // 무속성

                case ElementType.Fire:
                    return LocalizeKey._50001.ToText(); // 화속성

                case ElementType.Water:
                    return LocalizeKey._50002.ToText(); // 수속성

                case ElementType.Wind:
                    return LocalizeKey._50003.ToText(); // 풍속성

                case ElementType.Earth:
                    return LocalizeKey._50004.ToText(); // 지속성

                case ElementType.Poison:
                    return LocalizeKey._50005.ToText(); // 독속성

                case ElementType.Holy:
                    return LocalizeKey._50006.ToText(); // 성속성

                case ElementType.Shadow:
                    return LocalizeKey._50007.ToText(); // 암속성

                case ElementType.Ghost:
                    return LocalizeKey._50009.ToText(); // 염속성

                case ElementType.Undead:
                    return LocalizeKey._50008.ToText(); // 언속성

                default:
                    Debug.LogError($"[올바르지 않은 {nameof(ElementType)}] {nameof(type)} = {type}");
                    return string.Empty;
            }
        }

        public static int GetElementFactor(this ElementType skillElementType, ElementType targetElementType)
        {
            if (targetElementType == ElementType.None)
                return 10000; // 속성이 없을 경우 속성간 대미지 배율: 10000

            int targetElementValue = targetElementType.ToIntValue();
            switch (skillElementType)
            {
                default:
                case ElementType.None: // 속성이 아예 음슴
                    return 10000; // 속성이 없을 경우 속성간 대미지 배율: 10000

                case ElementType.Neutral: // 공격한 스킬이 무속성 공격
                    return BasisType.NEUTRAL_ENEMENT_DAMAGE.GetInt(targetElementValue);

                case ElementType.Fire: // 공격한 스킬이 화속성 공격
                    return BasisType.FIRE_ENEMENT_DAMAGE.GetInt(targetElementValue);

                case ElementType.Water: // 공격한 스킬이 수속성 공격
                    return BasisType.WATER_ENEMENT_DAMAGE.GetInt(targetElementValue);

                case ElementType.Wind: // 공격한 스킬이 풍속성 공격
                    return BasisType.WIND_ENEMENT_DAMAGE.GetInt(targetElementValue);

                case ElementType.Earth: // 공격한 스킬이 지속성 공격
                    return BasisType.EARTH_ENEMENT_DAMAGE.GetInt(targetElementValue);

                case ElementType.Poison: // 공격한 스킬이 독속성 공격
                    return BasisType.POISON_ENEMENT_DAMAGE.GetInt(targetElementValue);

                case ElementType.Holy: // 공격한 스킬이 성속성 공격
                    return BasisType.HOLY_ENEMENT_DAMAGE.GetInt(targetElementValue);

                case ElementType.Shadow: // 공격한 스킬이 암속성 공격
                    return BasisType.SHADOW_ENEMENT_DAMAGE.GetInt(targetElementValue);

                case ElementType.Ghost: // 공격한 스킬이 염속성 공격
                    return BasisType.GHOST_ENEMENT_DAMAGE.GetInt(targetElementValue);

                case ElementType.Undead: // 공격한 스킬이 사속성 공격
                    return BasisType.UNDEAD_ENEMENT_DAMAGE.GetInt(targetElementValue);
            }
        }
    }
}