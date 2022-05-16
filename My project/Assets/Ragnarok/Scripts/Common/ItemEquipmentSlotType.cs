namespace Ragnarok
{
    /// <summary>
    /// 장비 착용 위치 (코스튬 포함)
    /// </summary>
    public enum ItemEquipmentSlotType
    {
        /// <summary>
        /// 착용안됨
        /// </summary>
        None = 0,
        /// <summary>
        /// 투구
        /// </summary>
        HeadGear = 1,
        /// <summary>
        /// 외투
        /// </summary>
        Garment = 2,
        /// <summary>
        /// 갑옷
        /// </summary>
        Armor = 3,
        /// <summary>
        /// 무기
        /// </summary>
        Weapon = 4,
        /// <summary>
        /// 악세사리1
        /// </summary>
        Accessory1 = 5,
        /// <summary>
        /// 악세사리2
        /// </summary>
        Accessory2 = 6,
        /// <summary>
        /// 코스튬 무기
        /// </summary>
        CostumeWeapon = 7,
        /// <summary>
        /// 코스튬 모자
        /// </summary>
        CostumeHat = 8,
        /// <summary>
        /// 코스튬 얼굴
        /// </summary>
        CostumeFace = 9,
        /// <summary>
        /// 코스튬 망토
        /// </summary>
        CostumeCape = 10,
        /// <summary>
        /// 코스튬 펫
        /// </summary>
        CostumePet = 11,
        /// <summary>
        /// 코스튬 몸
        /// </summary>
        CostumeBody = 12,
        /// <summary>
        /// 코스튬 칭호
        /// </summary>
        CostumeTitle = 13,
        /// <summary>
        /// 쉐도우 투구
        /// </summary>
        ShadowHeadGear = 14,
        /// <summary>
        /// 쉐도우 외투
        /// </summary>
        ShadowGarment = 15,
        /// <summary>
        /// 쉐도우 갑옷
        /// </summary>
        ShadowArmor = 16,
        /// <summary>
        /// 쉐도우 무기
        /// </summary>
        ShadowWeapon = 17,
        /// <summary>
        /// 쉐도우 악세사리1
        /// </summary>
        ShadowAccessory1 = 18,
        /// <summary>
        /// 쉐도우 악세사리2
        /// </summary>
        ShadowAccessory2 = 19,
    }

    public static class ItemEquipmentSlotTypeExtensions
    {
        public static string ToText(this ItemEquipmentSlotType type)
        {
            switch (type)
            {
                case ItemEquipmentSlotType.None:
                    return LocalizeKey._57000.ToText(); // 전체
                case ItemEquipmentSlotType.HeadGear:
                    return LocalizeKey._57001.ToText(); // 투구
                case ItemEquipmentSlotType.Garment:
                    return LocalizeKey._57002.ToText(); // 외투
                case ItemEquipmentSlotType.Armor:
                    return LocalizeKey._57003.ToText(); // 갑옷
                case ItemEquipmentSlotType.Weapon:
                    return LocalizeKey._57004.ToText(); // 무기
                case ItemEquipmentSlotType.Accessory1:
                    return LocalizeKey._57005.ToText(); // 악세사리1
                case ItemEquipmentSlotType.Accessory2:
                    return LocalizeKey._57006.ToText(); // 악세사리2
                case ItemEquipmentSlotType.ShadowHeadGear:
                case ItemEquipmentSlotType.ShadowGarment:
                case ItemEquipmentSlotType.ShadowArmor:
                case ItemEquipmentSlotType.ShadowWeapon:
                case ItemEquipmentSlotType.ShadowAccessory1:
                case ItemEquipmentSlotType.ShadowAccessory2:
                    return LocalizeKey._57007.ToText(); // 쉐도우
            }
            return string.Empty;
        }        
    }
}
