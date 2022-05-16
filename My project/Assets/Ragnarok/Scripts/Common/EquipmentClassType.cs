using UnityEngine;

namespace Ragnarok
{
    public enum BattleItemIndex
    {
        /// <summary>
        /// 아이템 속성이 없을 경우
        /// </summary>
        None = 0,
        OneHandedSword = 1,
        OneHandedStaff = 2,
        Dagger = 3,
        Bow = 4,
        TwoHandedSword = 5,
        TwoHandedSpear = 6,
        Armor = 7,
        HeadGear = 8,
        Garment = 9,
        Accessory1 = 10,
        Accessory2 = 11,
    }

    /// <summary>
    /// 장비 클래스 타입
    /// </summary>
    [System.Flags]
    public enum EquipmentClassType : int
    {
        All = -1,

        /// <summary>
        /// 한손검
        /// </summary>
        OneHandedSword = 1 << 0,  // 1
        /// <summary>
        /// 지팡이
        /// </summary>
        OneHandedStaff = 1 << 1,  // 2
        /// <summary>
        /// 단검
        /// </summary>
        Dagger = 1 << 2,  // 4
        /// <summary>
        /// 활
        /// </summary>
        Bow = 1 << 3,  // 8
        /// <summary>
        /// 양손검
        /// </summary>
        TwoHandedSword = 1 << 4,  // 16
        /// <summary>
        /// 양손창
        /// </summary>
        TwoHandedSpear = 1 << 5,  // 32
        /// <summary>
        /// 갑옷
        /// </summary>
        Armor = 1 << 6,  // 64
        /// <summary>
        /// 투구
        /// </summary>
        HeadGear = 1 << 7,  // 128
        /// <summary>
        /// 망토
        /// </summary>
        Garment = 1 << 8,  // 256
        /// <summary>
        /// 장신구1
        /// </summary>
        Accessory1 = 1 << 9,  // 512
        /// <summary>
        /// 장신구2
        /// </summary>
        Accessory2 = 1 << 10, // 1024      

        /// <summary>
        /// 무기 타입
        /// </summary>
        Weapon = OneHandedSword | OneHandedStaff | Dagger | Bow | TwoHandedSword | TwoHandedSpear,

        /// <summary>
        /// 방어구 & 악세
        /// </summary>
        AllArmor = Armor | HeadGear | Garment | Accessory1 | Accessory2,
    }

    public static class EquipmentClassTypeExtensions
    {
        public static string GetIconName(this EquipmentClassType type, ItemDetailType itemDetailType)
        {
            if (itemDetailType == ItemDetailType.Shadow)
                return GetShadowIconName(type);

            if (itemDetailType == ItemDetailType.Transcend)
            {
                if (type.HasFlag(EquipmentClassType.Weapon))
                    return BasisItem.BeyondSword.ToString();

                if (type.HasFlag(EquipmentClassType.AllArmor))
                    return BasisItem.BeyondArmor.ToString();
            }

            switch (type)
            {
                case EquipmentClassType.Weapon:
                    return "OneHandedSword";
            }

            return type.ToString();
        }

        private static string GetShadowIconName(EquipmentClassType type)
        {
            switch (type)
            {
                case EquipmentClassType.Weapon:
                    return "ShadowOneHandedSword";
            }

            return $"Shadow{type}";
        }

        public static BattleItemIndex ToBattleItemIndex(this EquipmentClassType type)
        {
            switch (type)
            {
                case EquipmentClassType.OneHandedSword:
                    return BattleItemIndex.OneHandedSword;

                case EquipmentClassType.OneHandedStaff:
                    return BattleItemIndex.OneHandedStaff;

                case EquipmentClassType.Dagger:
                    return BattleItemIndex.Dagger;

                case EquipmentClassType.Bow:
                    return BattleItemIndex.Bow;

                case EquipmentClassType.TwoHandedSword:
                    return BattleItemIndex.TwoHandedSword;

                case EquipmentClassType.TwoHandedSpear:
                    return BattleItemIndex.TwoHandedSpear;

                case EquipmentClassType.Armor:
                    return BattleItemIndex.Armor;

                case EquipmentClassType.HeadGear:
                    return BattleItemIndex.HeadGear;

                case EquipmentClassType.Garment:
                    return BattleItemIndex.Garment;

                case EquipmentClassType.Accessory1:
                    return BattleItemIndex.Accessory1;

                case EquipmentClassType.Accessory2:
                    return BattleItemIndex.Accessory2;
            }

            return default;
        }

        public static int ToLocalizeKey(this EquipmentClassType type)
        {
            switch (type)
            {
                case EquipmentClassType.All:
                    return LocalizeKey._40221; // 모든 부위

                case EquipmentClassType.OneHandedSword:
                    return LocalizeKey._28006; // 한손검

                case EquipmentClassType.OneHandedStaff:
                    return LocalizeKey._28007; // 지팡이

                case EquipmentClassType.Dagger:
                    return LocalizeKey._28008; // 단검

                case EquipmentClassType.Bow:
                    return LocalizeKey._28009; // 활

                case EquipmentClassType.TwoHandedSword:
                    return LocalizeKey._28010; // 양손검

                case EquipmentClassType.TwoHandedSpear:
                    return LocalizeKey._28011; // 양손창

                case EquipmentClassType.Armor:
                    return LocalizeKey._28012; // 갑옷

                case EquipmentClassType.HeadGear:
                    return LocalizeKey._28013; // 투구

                case EquipmentClassType.Garment:
                    return LocalizeKey._28014; // 망토

                case EquipmentClassType.Accessory1:
                    return LocalizeKey._28015; // 목걸이

                case EquipmentClassType.Accessory2:
                    return LocalizeKey._28016; // 반지
            }

            return default;
        }
    }
}