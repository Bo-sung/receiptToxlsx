namespace Ragnarok
{
    /// <summary>
    /// 코스튬 타입
    /// </summary>
    [System.Flags]
    public enum CostumeType : int
    {
        None = 0,
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
        /// 모자
        /// </summary>
        Hat = 1 << 6, // 64

        /// <summary>
        /// 얼굴 장식
        /// </summary>
        Face = 1 << 7, // 128

        /// <summary>
        /// 망토
        /// </summary>
        Garment = 1 << 8, // 256

        /// <summary>
        /// 펫
        /// </summary>
        Pet = 1 << 9, // 512

        /// <summary>
        /// 전신
        /// </summary>
        Body = 1 << 10, // 1024

        /// <summary>
        /// 칭호
        /// </summary>
        Title = 1 << 11, // 2048

        /// <summary>
        /// 무기 타입
        /// </summary>
        Weapon = OneHandedSword | OneHandedStaff | Dagger | Bow | TwoHandedSword | TwoHandedSpear,
    }

    public static class CostumeTypeExtensions
    {
        public static EquipmentClassType ToWeaponType(this CostumeType type)
        {
            switch (type)
            {
                case CostumeType.OneHandedSword:
                    return EquipmentClassType.OneHandedSword;

                case CostumeType.OneHandedStaff:
                    return EquipmentClassType.OneHandedStaff;

                case CostumeType.Dagger:
                    return EquipmentClassType.Dagger;

                case CostumeType.Bow:
                    return EquipmentClassType.Bow;

                case CostumeType.TwoHandedSword:
                    return EquipmentClassType.TwoHandedSword;

                case CostumeType.TwoHandedSpear:
                    return EquipmentClassType.TwoHandedSpear;
            }

            throw new System.ArgumentException($"[올바르지 않은 {nameof(type)}] {nameof(type)} = {type}");
        }

        public static string GetIconName(this CostumeType type)
        {
            switch (type)
            {
                case CostumeType.Weapon:
                    return "OneHandedSword";
            }

            return type.ToString();
        }

        public static int ToLocalizeKey(this CostumeType type)
        {
            switch (type)
            {
                case CostumeType.OneHandedSword:
                    return LocalizeKey._28006; // 한손검

                case CostumeType.OneHandedStaff:
                    return LocalizeKey._28007; // 지팡이

                case CostumeType.Dagger:
                    return LocalizeKey._28008; // 단검

                case CostumeType.Bow:
                    return LocalizeKey._28009; // 활

                case CostumeType.TwoHandedSword:
                    return LocalizeKey._28010; // 양손검

                case CostumeType.TwoHandedSpear:
                    return LocalizeKey._28011; // 양손창

                case CostumeType.Hat:
                    return LocalizeKey._40222; // 머리

                case CostumeType.Face:
                    return LocalizeKey._40223; //얼굴

                case CostumeType.Garment:
                    return LocalizeKey._40224; // 등    

                case CostumeType.Pet:
                    return LocalizeKey._40235; // 펫

                case CostumeType.Body:
                    return LocalizeKey._40241; // 몸

                case CostumeType.Title:
                    return LocalizeKey._40239; // 칭호
            }

            return default;
        }
    }
}