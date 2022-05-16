using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 아이템 대분류 타입
    /// </summary>
    public enum ItemGroupType : byte
    {
        None = 0,

        /// <summary>장비</summary>
        Equipment = 1,

        /// <summary>카드</summary>
        Card = 2,

        /// <summary>제작 재료</summary>
        ProductParts = 3,

        /// <summary>소모품</summary>
        ConsumableItem = 4,

        /// <summary>코스튬</summary>
        Costume = 5,

        /// <summary>몬스터 조각</summary>
        MonsterPiece = 6,
    }

    public static class ItemGroupTypeExtensions
    {
        public static string ToText(this ItemGroupType type)
        {
            switch (type)
            {
                case ItemGroupType.Equipment:
                    return LocalizeKey._53000.ToText(); // 장비 아이템

                case ItemGroupType.Card:
                    return LocalizeKey._53002.ToText(); // 몬스터 카드

                case ItemGroupType.ProductParts:
                    return LocalizeKey._53004.ToText(); // 제작 재료

                case ItemGroupType.ConsumableItem:
                    return LocalizeKey._53001.ToText(); // 소모 아이템

                case ItemGroupType.Costume:
                    return LocalizeKey._6017.ToText(); // 코스튬

                case ItemGroupType.MonsterPiece:
                    return LocalizeKey._53005.ToText(); // 몬스터 조각

                default:
                    Debug.LogError($"[올바르지 않은 {nameof(ItemType)}] {nameof(type)} = {type}");
                    return string.Empty;
            }
        }
    }
}