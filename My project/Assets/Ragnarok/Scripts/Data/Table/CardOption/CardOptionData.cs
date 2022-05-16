using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="CardOptionDataManager"/>
    /// </summary>
    public sealed class CardOptionData : IData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt battle_option_type;
        public readonly ObscuredInt option_value_1_rate;
        public readonly ObscuredInt option_value_2_rate;
        public readonly ObscuredInt level_division;
        public readonly ObscuredInt level_mod;
        public readonly ObscuredInt marking_level;
        public readonly ObscuredInt amplification_value;
        public readonly ObscuredInt increase_value;
        public readonly ObscuredInt value_1_default_min;
        public readonly ObscuredInt value_1_default_max;
        public readonly ObscuredInt value_1_prob;
        public readonly ObscuredInt value_2_default_min;
        public readonly ObscuredInt value_2_default_max;
        public readonly ObscuredInt value_2_prob;
        public readonly ObscuredInt period_min;
        public readonly ObscuredInt period_max;
        public readonly ObscuredInt period_prob;

        public CardOptionData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id                  = data[index++].AsInt32();
            battle_option_type  = data[index++].AsInt32();
            option_value_1_rate = data[index++].AsInt32();
            option_value_2_rate = data[index++].AsInt32();
            level_division      = data[index++].AsInt32();
            level_mod           = data[index++].AsInt32();
            marking_level       = data[index++].AsInt32();
            amplification_value = data[index++].AsInt32();
            increase_value      = data[index++].AsInt32();
            value_1_default_min = data[index++].AsInt32();
            value_1_default_max = data[index++].AsInt32();
            value_1_prob        = data[index++].AsInt32();
            value_2_default_min = data[index++].AsInt32();
            value_2_default_max = data[index++].AsInt32();
            value_2_prob        = data[index++].AsInt32();
            period_min          = data[index++].AsInt32();
            period_max          = data[index++].AsInt32();
            period_prob         = data[index++].AsInt32();
        }
    }
}