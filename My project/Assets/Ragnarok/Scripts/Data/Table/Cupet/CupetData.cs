using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class CupetData : IData
    {
        /// <summary>
        /// 큐펫 스킬 개수
        /// </summary>
        public const int CUPET_SKILL_SIZE = 4;

        public readonly ObscuredInt id;
        public readonly ObscuredString prefab_name;
        public readonly ObscuredInt piece_id;
        public readonly ObscuredInt name_id;
        public readonly ObscuredByte element_type;
        public readonly ObscuredInt move_spd;
        public readonly ObscuredInt atk_spd;
        public readonly ObscuredInt atk_range;
        public readonly ObscuredInt cupet_rating;
        public readonly ObscuredInt hp_coefficient;
        public readonly ObscuredInt atk_coefficient;
        public readonly ObscuredInt matk_coefficient;
        public readonly ObscuredInt def_coefficient;
        public readonly ObscuredInt mdef_coefficient;
        public readonly ObscuredInt add_str;
        public readonly ObscuredInt add_agi;
        public readonly ObscuredInt add_vit;
        public readonly ObscuredInt add_int;
        public readonly ObscuredInt add_dex;
        public readonly ObscuredInt add_luk;
        public readonly ObscuredByte cupet_type;
        public readonly ObscuredInt skill_rate_1;
        public readonly ObscuredInt skill_id_1;
        public readonly ObscuredInt skill_rate_2;
        public readonly ObscuredInt skill_id_2;
        public readonly ObscuredInt skill_rate_3;
        public readonly ObscuredInt skill_id_3;
        public readonly ObscuredInt skill_rate_4;
        public readonly ObscuredInt skill_id_4;
        public readonly ObscuredInt cost;
        public readonly ObscuredInt basic_active_skill_id;
        public readonly ObscuredString icon_name;

        public CupetData(IList<MessagePackObject> data)
        {
            id                    = data[0].AsInt32();
            prefab_name           = data[1].AsString();
            piece_id              = data[2].AsInt32();
            name_id               = data[3].AsInt32();
            element_type          = data[4].AsByte();
            move_spd              = data[5].AsInt32();
            atk_spd               = data[6].AsInt32();
            atk_range             = data[7].AsInt32();
            cupet_rating          = data[8].AsInt32();
            hp_coefficient        = data[9].AsInt32();
            atk_coefficient       = data[10].AsInt32();
            matk_coefficient      = data[11].AsInt32();
            def_coefficient       = data[12].AsInt32();
            mdef_coefficient      = data[13].AsInt32();
            add_str               = data[14].AsInt32();
            add_agi               = data[15].AsInt32();
            add_vit               = data[16].AsInt32();
            add_int               = data[17].AsInt32();
            add_dex               = data[18].AsInt32();
            add_luk               = data[19].AsInt32();
            cupet_type            = data[20].AsByte();
            skill_rate_1          = data[21].AsInt32();
            skill_id_1            = data[22].AsInt32();
            skill_rate_2          = data[23].AsInt32();
            skill_id_2            = data[24].AsInt32();
            skill_rate_3          = data[25].AsInt32();
            skill_id_3            = data[26].AsInt32();
            skill_rate_4          = data[27].AsInt32();
            skill_id_4            = data[28].AsInt32();
            // 29 is_log
            cost                  = data[30].AsInt32();
            basic_active_skill_id = data[31].AsInt32();
            icon_name             = data[32].AsString();
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
    }
}