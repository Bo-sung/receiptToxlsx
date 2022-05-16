using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public class StatData : IData
    {
        public readonly ObscuredInt base_lv;
        public readonly ObscuredInt stat;
        public readonly ObscuredInt transmission_stat;

        public StatData(IList<MessagePackObject> data)
        {
            base_lv           = data[0].AsInt32();
            stat              = data[1].AsInt32();
            transmission_stat = data[2].AsInt32();
        }
    }
}