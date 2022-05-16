using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class CrowdControlData : IData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredString name_id;
        public readonly ObscuredString icon_name;
        public readonly ObscuredInt overlap_count;
        public readonly ObscuredInt duration;
        public readonly ObscuredByte cannot_move;
        public readonly ObscuredByte cannot_use_basic_attack;
        public readonly ObscuredByte cannot_use_skill;
        public readonly ObscuredByte cannot_flee;
        public readonly ObscuredInt dot_damage_rate;
        public readonly ObscuredInt def_decrease_rate;
        public readonly ObscuredInt mdef_decrease_rate;
        public readonly ObscuredInt total_dmg_decrease_rate;
        public readonly ObscuredInt cri_rate_decrease_rate;
        public readonly ObscuredInt move_spd_decrease_rate;
        public readonly ObscuredInt atk_spd_decreaase_rate;

        public CrowdControlData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id                      = data[index++].AsInt32();
            name_id                 = data[index++].AsString();
            icon_name               = data[index++].AsString();
            overlap_count           = data[index++].AsInt32();
            duration                = data[index++].AsInt32();
            cannot_move             = data[index++].AsByte();
            cannot_use_basic_attack = data[index++].AsByte();
            cannot_use_skill        = data[index++].AsByte();
            cannot_flee             = data[index++].AsByte();
            dot_damage_rate         = data[index++].AsInt32();
            def_decrease_rate       = data[index++].AsInt32();
            mdef_decrease_rate      = data[index++].AsInt32();
            total_dmg_decrease_rate = data[index++].AsInt32();
            cri_rate_decrease_rate  = data[index++].AsInt32();
            move_spd_decrease_rate  = data[index++].AsInt32();
            atk_spd_decreaase_rate  = data[index++].AsInt32();
        }
    }
}
