using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class PanelBuffData : IData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt group;
        public readonly ObscuredInt rate;
        public readonly ObscuredInt battle_option_type;
        public readonly ObscuredInt value1;
        public readonly ObscuredInt value2;

        public PanelBuffData(IList<MessagePackObject> data)
        {
            id                 = data[0].AsInt32();
            group              = data[1].AsInt32();
            rate               = data[2].AsInt32();
            battle_option_type = data[3].AsInt32();
            value1             = data[4].AsInt32();
            value2             = data[5].AsInt32();
        }
    }
}