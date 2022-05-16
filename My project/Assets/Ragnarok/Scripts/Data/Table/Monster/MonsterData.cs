using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="MonsterDataManager"/>
    /// </summary>
    public class MonsterData : IData, IUnitSkillInfo
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt cost;
        public readonly ObscuredInt sensing;
        public readonly ObscuredInt monster_group; 
        public readonly ObscuredInt item_id;
        public readonly ObscuredInt name_id;
        public readonly ObscuredByte element_type;
        public readonly ObscuredInt move_spd;
        public readonly ObscuredInt atk_spd;
        public readonly ObscuredInt atk_range;
        public readonly ObscuredInt hp_coefficient;
        public readonly ObscuredInt atk_coefficient;
        public readonly ObscuredInt matk_coefficient;
        public readonly ObscuredInt def_coefficient;
        public readonly ObscuredInt mdef_coefficient;
        public readonly ObscuredInt base_str;
        public readonly ObscuredInt base_agi;
        public readonly ObscuredInt base_vit;
        public readonly ObscuredInt base_int;
        public readonly ObscuredInt base_dex;
        public readonly ObscuredInt base_luk;
        public readonly ObscuredInt skill_rate_1;
        public readonly ObscuredInt skill_rate_2;
        public readonly ObscuredInt skill_rate_3;
        public readonly ObscuredInt skill_rate_4;
        public readonly ObscuredInt skill_id_1;
        public readonly ObscuredInt skill_id_2;
        public readonly ObscuredInt skill_id_3;
        public readonly ObscuredInt skill_id_4;
        public readonly ObscuredString prefab_name;
        public readonly ObscuredInt basic_active_skill_id;
        public readonly ObscuredString icon_name;
        public readonly ObscuredInt pattern_type;
        public readonly ObscuredInt pattern_value1;
        public readonly ObscuredInt pattern_value2;
        public readonly ObscuredInt pattern_value3;
        public readonly ObscuredInt pattern_value4;
        public readonly ObscuredInt radius;
        public readonly ObscuredInt hud_y_value;
        public readonly ObscuredInt dic_id;
        public readonly ObscuredInt dic_order;
        public readonly ObscuredInt dic_lv;

        int IUnitSkillInfo.Skill_id_1 => skill_id_1;
        int IUnitSkillInfo.Skill_id_2 => skill_id_2;
        int IUnitSkillInfo.Skill_id_3 => skill_id_3;
        int IUnitSkillInfo.Skill_id_4 => skill_id_4;

        bool IInfo.IsInvalidData => default;
        public event System.Action OnUpdateEvent;


        public MonsterData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id                    = data[index++].AsInt32();
            cost                  = data[index++].AsInt32();
            sensing               = data[index++].AsInt32();
            monster_group         = data[index++].AsInt32(); 
            item_id               = data[index++].AsInt32();
            name_id               = data[index++].AsInt32();
            element_type          = data[index++].AsByte();
            move_spd              = data[index++].AsInt32();
            atk_spd               = data[index++].AsInt32();
            atk_range             = data[index++].AsInt32();
            hp_coefficient        = data[index++].AsInt32();
            atk_coefficient       = data[index++].AsInt32();
            matk_coefficient      = data[index++].AsInt32();
            def_coefficient       = data[index++].AsInt32();
            mdef_coefficient      = data[index++].AsInt32();
            base_str              = data[index++].AsInt32();
            base_agi              = data[index++].AsInt32();
            base_vit              = data[index++].AsInt32();
            base_int              = data[index++].AsInt32();
            base_dex              = data[index++].AsInt32();
            base_luk              = data[index++].AsInt32();
            skill_rate_1          = data[index++].AsInt32();
            skill_id_1            = data[index++].AsInt32();
            skill_rate_2          = data[index++].AsInt32();
            skill_id_2            = data[index++].AsInt32();
            skill_rate_3          = data[index++].AsInt32();
            skill_id_3            = data[index++].AsInt32();
            skill_rate_4          = data[index++].AsInt32();            
            skill_id_4            = data[index++].AsInt32();
            prefab_name           = data[index++].AsString();
            basic_active_skill_id = data[index++].AsInt32();
            icon_name             = data[index++].AsString();
            pattern_type          = data[index++].AsInt32();
            pattern_value1        = data[index++].AsInt32();
            pattern_value2        = data[index++].AsInt32();
            pattern_value3        = data[index++].AsInt32();
            pattern_value4        = data[index++].AsInt32();
            radius                = data[index++].AsInt32();
            hud_y_value           = data[index++].AsInt32();
            dic_id                = data[index++].AsInt32();
            dic_order             = data[index++].AsInt32();
            dic_lv                = data[index++].AsInt32();
        }

        public int GetSkillID(int index)
        {
            switch (index)
            {
                case 0: return skill_id_1;
                case 1: return skill_id_2;
                case 2: return skill_id_3;
                case 3: return skill_id_4;

                default:
                    Debug.LogError($"[올바르지 않은 {nameof(index)}] {nameof(index)} = {index}");
                    break;
            }

            return 0;
        }

        public int GetSkillRate(int index)
        {
            switch (index)
            {
                case 0: return skill_rate_1;
                case 1: return skill_rate_2;
                case 2: return skill_rate_3;
                case 3: return skill_rate_4;

                default:
                    Debug.LogError($"[올바르지 않은 {nameof(index)}] {nameof(index)} = {index}");
                    break;
            }

            return 0;
        }

        public float GetHudOffset()
        {
            return MathUtils.ToPercentValue(hud_y_value);
        }
    }
}