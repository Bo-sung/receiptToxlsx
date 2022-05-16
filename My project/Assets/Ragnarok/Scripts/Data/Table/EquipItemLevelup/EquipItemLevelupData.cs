using System.Collections.Generic;

using CodeStage.AntiCheat.ObscuredTypes;

using MsgPack;

namespace Ragnarok
{
    public class EquipItemLevelupData : IData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt type; // 1: 무기 2: 방어구
        public readonly ObscuredInt rating;
        public readonly ObscuredInt smelt_level;
        public readonly ObscuredInt tier_per;
        public readonly ObscuredInt resource_type;
        public readonly ObscuredInt resource_value;
        public readonly ObscuredInt resource_count;

        public EquipItemLevelupData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id             = data[index++].AsInt32();
            type           = data[index++].AsInt32();
            rating         = data[index++].AsInt32();
            smelt_level    = data[index++].AsInt32();
            tier_per       = data[index++].AsInt32();
            resource_type  = data[index++].AsInt32();
            resource_value = data[index++].AsInt32();
            resource_count = data[index++].AsInt32();
        }
    }
}