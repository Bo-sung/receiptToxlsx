using UnityEngine;

namespace Ragnarok
{
    public enum CrowdControlType
    {
        /// <summary>
        /// 스턴
        /// </summary>
        Stun = 1,

        /// <summary>
        /// 침묵
        /// </summary>
        Silence = 2,

        /// <summary>
        /// 수면
        /// </summary>
        Sleep = 3,

        /// <summary>
        /// 환각
        /// </summary>
        Hallucination = 4,

        /// <summary>
        /// 출혈
        /// </summary>
        Bleeding = 5,

        /// <summary>
        /// 화상
        /// </summary>
        Burning = 6,

        /// <summary>
        /// 중독
        /// </summary>
        Poison = 7,

        /// <summary>
        /// 저주
        /// </summary>
        Curse = 8,

        /// <summary>
        /// 빙결
        /// </summary>
        Freezing = 9,

        /// <summary>
        /// 동빙
        /// </summary>
        Frozen = 10,
    }


    public static class CrowdControlTypeExtension
    {
        public static string GetIconName(this CrowdControlType type)
        {
            return type.ToString();
        }

        public static string ToText(this CrowdControlType type)
        {
            switch (type)
            {
                case CrowdControlType.Stun:
                    return LocalizeKey._50100.ToText(); // 스턴
                case CrowdControlType.Silence:
                    return LocalizeKey._50101.ToText(); // 침묵
                case CrowdControlType.Sleep:
                    return LocalizeKey._50102.ToText(); // 수면
                case CrowdControlType.Hallucination:
                    return LocalizeKey._50103.ToText(); // 환각
                case CrowdControlType.Bleeding:
                    return LocalizeKey._50104.ToText(); // 출혈
                case CrowdControlType.Burning:
                    return LocalizeKey._50105.ToText(); // 화상
                case CrowdControlType.Poison:
                    return LocalizeKey._50106.ToText(); // 중독
                case CrowdControlType.Curse:
                    return LocalizeKey._50107.ToText(); // 저주
                case CrowdControlType.Freezing:
                    return LocalizeKey._50108.ToText(); // 빙결
                case CrowdControlType.Frozen:
                    return LocalizeKey._50109.ToText(); // 동빙

                default:
                    Debug.LogError($"[올바르지 않은 {nameof(CrowdControlType)}] {nameof(type)} = {type}");
                    return string.Empty;
            }
        }

        public static string ToDescText(this CrowdControlType type)
        {
            switch (type)
            {
                case CrowdControlType.Stun:
                    return LocalizeKey._50200.ToText(); // 스턴 상태에 빠지면 ........
                case CrowdControlType.Silence:
                    return LocalizeKey._50201.ToText(); // 침묵 상태에 빠지면 ........
                case CrowdControlType.Sleep:
                    return LocalizeKey._50202.ToText(); // 수면 상태에 빠지면 ........
                case CrowdControlType.Hallucination:
                    return LocalizeKey._50203.ToText(); // 환각 상태에 빠지면 ........
                case CrowdControlType.Bleeding:
                    return LocalizeKey._50204.ToText(); // 출혈 상태에 빠지면 ........
                case CrowdControlType.Burning:
                    return LocalizeKey._50205.ToText(); // 화상 상태에 빠지면 ........
                case CrowdControlType.Poison:
                    return LocalizeKey._50206.ToText(); // 중독 상태에 빠지면 ........
                case CrowdControlType.Curse:
                    return LocalizeKey._50207.ToText(); // 저주 상태에 빠지면 ........
                case CrowdControlType.Freezing:
                    return LocalizeKey._50208.ToText(); // 빙결 상태에 빠지면 ........
                case CrowdControlType.Frozen:
                    return LocalizeKey._50209.ToText(); // 동빙 상태에 빠지면 ........

                default:
                    Debug.LogError($"[올바르지 않은 {nameof(CrowdControlType)}] {nameof(type)} = {type}");
                    return string.Empty;
            }
        }
    }
}