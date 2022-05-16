using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    public class ItemData : IData, BoxRewardItemView.IInput
    {
        public readonly ObscuredInt id;
        public readonly ObscuredByte item_type;
        public readonly ObscuredInt name_id;
        public readonly ObscuredInt des_id;
        public readonly ObscuredInt price;
        public readonly ObscuredInt weight;
        public readonly ObscuredLong cooldown;
        public readonly ObscuredInt event_id; // item_type이 소모품 = 박스테이블ID, 몬스터카드 = 몬스터조각ID, 몬스터조각 = 큐펫테이블ID, 재료(element_type이 1이상인 재료) = 속성석의 레벨
        /// <summary>
        /// 소모품 (1: 확성기, 2: 성전환 물약)
        /// 재료 (어둠의 나무 포인트)
        /// </summary>
        public readonly ObscuredInt skill_rate;
        public readonly ObscuredInt skill_id; // item_type이 몬스터조각 = 몬스터카드ID
        public readonly ObscuredInt class_type;
        public readonly ObscuredByte element_type;
        public readonly ObscuredInt rating; // item_type이 몬스터카드 = 1:normal, 2:boss
        public readonly ObscuredInt atk_min; // item_type이 제작재료 [카드제련재료] 제련 경고 표시 레벨
        public readonly ObscuredInt atk_max; // item_type이 제작재료 [카드제련재료] 제련 최대 레벨
        public readonly ObscuredInt matk_min; // item_type이 제작재료 = 큐펫경험치
        public readonly ObscuredInt matk_max; // item_type이 제작재료 = 길드전_버프경험치
        public readonly ObscuredInt def_min;
        public readonly ObscuredInt def_max;
        public readonly ObscuredInt mdef_min;
        public readonly ObscuredInt mdef_max;
        public readonly ObscuredLong duration; // 장비,카드의 경우 쉐도우 구분 용도( 0 : 일반, 1 : 쉐도우 )
        public readonly ObscuredInt battle_option_type_1;
        public readonly ObscuredInt value1_b1;
        public readonly ObscuredInt value2_b1;
        public readonly ObscuredInt battle_option_type_2;
        public readonly ObscuredInt value1_b2;
        public readonly ObscuredInt value2_b2;
        public readonly ObscuredInt battle_option_type_3;
        public readonly ObscuredInt value1_b3;
        public readonly ObscuredInt value2_b3;
        public readonly ObscuredInt battle_option_type_4;
        public readonly ObscuredInt value1_b4;
        public readonly ObscuredInt value2_b4;
        public readonly ObscuredString icon_name;
        public readonly ObscuredString prefab_name;
        public readonly ObscuredInt point_value; // 소모 될 RoPoint
        public readonly ObscuredLong item_get_bit_type;
        public readonly ObscuredLong item_use_bit_type;
        public readonly ObscuredByte item_group_type;
        public readonly ObscuredString effect_name; // 장비(무기), 코스튬 이펙트 이름
        public readonly ObscuredInt dic_id;
        public readonly ObscuredInt dic_order;
        public readonly ObscuredInt price_max_limit;
        public readonly ObscuredInt book_type;

        public ItemData(IList<MessagePackObject> data)
        {
            id                   = data[0].AsInt32();
            item_type            = data[1].AsByte();
            name_id              = data[2].AsInt32();
            des_id               = data[3].AsInt32();
            price                = data[4].AsInt32();
            weight               = data[5].AsInt32();
            cooldown             = data[6].AsInt64();
            event_id             = data[7].AsInt32();
            skill_rate           = data[8].AsInt32();
            skill_id             = data[9].AsInt32();
            class_type           = data[10].AsInt32();
            element_type         = data[11].AsByte();
            rating               = data[12].AsInt32();
            atk_min              = data[13].AsInt32();
            atk_max              = data[14].AsInt32();
            matk_min             = data[15].AsInt32();
            matk_max             = data[16].AsInt32();
            def_min              = data[17].AsInt32();
            def_max              = data[18].AsInt32();
            mdef_min             = data[19].AsInt32();
            mdef_max             = data[20].AsInt32();
            duration             = data[21].AsInt64();
            battle_option_type_1 = data[22].AsInt32();
            value1_b1            = data[23].AsInt32();
            value2_b1            = data[24].AsInt32();
            battle_option_type_2 = data[25].AsInt32();
            value1_b2            = data[26].AsInt32();
            value2_b2            = data[27].AsInt32();
            battle_option_type_3 = data[28].AsInt32();
            value1_b3            = data[29].AsInt32();
            value2_b3            = data[30].AsInt32();
            battle_option_type_4 = data[31].AsInt32();
            value1_b4            = data[32].AsInt32();
            value2_b4            = data[33].AsInt32();
            icon_name            = data[34].AsString();
            // 35 is_log
            prefab_name          = data[36].AsString();
            point_value          = data[37].AsInt32();
            item_get_bit_type    = data[38].AsInt64();
            item_use_bit_type    = data[39].AsInt64();
            item_group_type      = data[40].AsByte();
            effect_name          = data[41].AsString();
            dic_id               = data[42].AsInt32();
            dic_order            = data[43].AsInt32();
            price_max_limit      = data[44].AsInt32();
            book_type            = data[45].AsInt32();
        }

        /// <summary>
        /// [장비] 장착 슬롯 타입 반환
        /// </summary>
        public ItemEquipmentSlotType GetEquipmentSlotType()
        {
            if (duration == 1)
                return GetShadowEquipmentSlotType();

            switch (class_type.ToEnum<EquipmentClassType>())
            {
                case EquipmentClassType.OneHandedSword:
                case EquipmentClassType.OneHandedStaff:
                case EquipmentClassType.Dagger:
                case EquipmentClassType.Bow:
                case EquipmentClassType.TwoHandedSword:
                case EquipmentClassType.TwoHandedSpear:
                    return ItemEquipmentSlotType.Weapon;

                case EquipmentClassType.Armor:
                    return ItemEquipmentSlotType.Armor;

                case EquipmentClassType.HeadGear:
                    return ItemEquipmentSlotType.HeadGear;

                case EquipmentClassType.Garment:
                    return ItemEquipmentSlotType.Garment;

                case EquipmentClassType.Accessory1:
                    return ItemEquipmentSlotType.Accessory1;

                case EquipmentClassType.Accessory2:
                    return ItemEquipmentSlotType.Accessory2;                
            }

            return ItemEquipmentSlotType.None;
        }

        /// <summary>
        /// [장비] 장착 슬롯 타입 반환
        /// </summary>
        public ItemEquipmentSlotType GetShadowEquipmentSlotType()
        {
            switch (class_type.ToEnum<EquipmentClassType>())
            {
                case EquipmentClassType.OneHandedSword:
                case EquipmentClassType.OneHandedStaff:
                case EquipmentClassType.Dagger:
                case EquipmentClassType.Bow:
                case EquipmentClassType.TwoHandedSword:
                case EquipmentClassType.TwoHandedSpear:
                    return ItemEquipmentSlotType.ShadowWeapon;

                case EquipmentClassType.Armor:
                    return ItemEquipmentSlotType.ShadowArmor;

                case EquipmentClassType.HeadGear:
                    return ItemEquipmentSlotType.ShadowHeadGear;

                case EquipmentClassType.Garment:
                    return ItemEquipmentSlotType.ShadowGarment;

                case EquipmentClassType.Accessory1:
                    return ItemEquipmentSlotType.ShadowAccessory1;

                case EquipmentClassType.Accessory2:
                    return ItemEquipmentSlotType.ShadowAccessory2;
            }

            return ItemEquipmentSlotType.None;
        }

        public ItemEquipmentSlotType GetCostumeSlotType()
        {
            switch (class_type.ToEnum<CostumeType>())
            {
                case CostumeType.OneHandedSword:
                case CostumeType.OneHandedStaff:
                case CostumeType.Dagger:
                case CostumeType.Bow:
                case CostumeType.TwoHandedSword:
                case CostumeType.TwoHandedSpear:
                    return ItemEquipmentSlotType.CostumeWeapon;

                case CostumeType.Hat:
                    return ItemEquipmentSlotType.CostumeHat;

                case CostumeType.Face:
                    return ItemEquipmentSlotType.CostumeFace;

                case CostumeType.Garment:
                    return ItemEquipmentSlotType.CostumeCape;

                case CostumeType.Pet:
                    return ItemEquipmentSlotType.CostumePet;

                case CostumeType.Body:
                    return ItemEquipmentSlotType.CostumeBody;

                case CostumeType.Title:
                    return ItemEquipmentSlotType.CostumeTitle;
            }
            return ItemEquipmentSlotType.None;
        }

        public ItemGroupType ItemGroupType => item_group_type.ToEnum<ItemGroupType>();

        public ItemType ItemType => GetItemType();

        private ItemType GetItemType()
        {
            ItemType itemType = item_type.ToEnum<ItemType>();

            // 박스 타입으로 변환
            if (itemType == ItemType.ConsumableItem && event_id > 0)
            {
                return ItemType.Box;
            }

            return itemType;
        }

        /// <summary>
        /// 속성석 레벨 반환
        /// </summary>
        public int GetElementStoneLevel()
        {
            if (ItemType != ItemType.ProductParts)
                return -1;

            if (element_type == 0)
                return -1;

            return event_id; // 속성석 레벨
        }

        /// <summary>
        /// 중첩 가능아이템 여부
        /// </summary>
        /// <returns></returns>
        public bool IsStackable()
        {
            switch (ItemGroupType)
            {
                case ItemGroupType.ProductParts:
                case ItemGroupType.ConsumableItem:
                case ItemGroupType.MonsterPiece:
                    return true;
            }
            return false;
        }

        public BookType BookType => book_type.ToEnum<BookType>();

        string BoxRewardItemView.IInput.IconName => icon_name;
        string BoxRewardItemView.IInput.Name => name_id.ToText();
        string BoxRewardItemView.IInput.Description => GetDescription();

        private string GetDescription()
        {
            // 거래 가능
            if (point_value > 0)
                return des_id.ToText();

            return StringBuilderPool.Get()
                .Append(des_id.ToText())
                .Append(LocalizeKey._30508.ToText()) // [c][cdcdcd] (거래 불가)[-][/c]
                .Release();
        }
    }
}