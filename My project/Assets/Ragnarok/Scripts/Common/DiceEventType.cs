using UnityEngine;

namespace Ragnarok
{
    public enum DiceEventType
    {
        /// <summary>
        /// 이동
        /// </summary>
        Move = 0,

        /// <summary>
        /// 보상 타일 변경
        /// </summary>
        ChangeBoardTile = 1,

        /// <summary>
        /// 이벤트주화 획득
        /// </summary>
        GainEventCoin = 2,

        /// <summary>
        /// 마지막으로 이동
        /// </summary>
        MoveToHome = 3,

        /// <summary>
        /// 처음으로 이동
        /// </summary>
        ReturnToHome = 4,
    }

    public static class DiceEventTypeExtensions
    {
        public static string ToText(this DiceEventType type, int value)
        {
            switch (type)
            {
                case DiceEventType.Move:
                    const int MOVE_FORWARD_LOCAL_KEY = LocalizeKey._11312; // {VALUE}칸 전진!
                    const int MOVE_BACKWARD_LOCAL_KEY = LocalizeKey._11313; // {VALUE}칸 후진!
                    return (value < 0 ? MOVE_BACKWARD_LOCAL_KEY : MOVE_FORWARD_LOCAL_KEY).ToText().Replace(ReplaceKey.VALUE, MathUtils.Abs(value));

                case DiceEventType.ChangeBoardTile:
                    return LocalizeKey._11314.ToText(); // 보두 구성 변경!

                case DiceEventType.GainEventCoin:
                    return LocalizeKey._11315.ToText().Replace(ReplaceKey.VALUE, value); // 이벤트 주화 {VALUE}개 획득!

                case DiceEventType.MoveToHome:
                    return LocalizeKey._11316.ToText(); // 집까지 전진!

                case DiceEventType.ReturnToHome:
                    return LocalizeKey._11317.ToText(); // 집으로 복귀!
            }

            Debug.LogError($"[올바르지 않은 {nameof(DiceEventType)}] {nameof(type)} = {type}");
            return string.Empty;
        }
    }
}