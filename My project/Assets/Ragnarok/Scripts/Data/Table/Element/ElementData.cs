using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="ElementDataManager"/>
    /// </summary>
    public sealed class ElementData : IData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt attacker_type;
        public readonly ObscuredInt attacker_level;
        public readonly ObscuredInt defender_type;
        public readonly ObscuredInt defender_level;
        public readonly ObscuredInt damage_value;

        public ElementData(IList<MessagePackObject> data)
        {
            int index = 0;
            id = data[index++].AsInt32();
            attacker_type = data[index++].AsInt32();
            attacker_level = data[index++].AsInt32();
            defender_type = data[index++].AsInt32();
            defender_level = data[index++].AsInt32();
            damage_value = data[index++].AsInt32();
        }
    }
}