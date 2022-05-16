using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public class SmeltCoefficientData : IData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt seq;
        public readonly ObscuredInt q1;
        public readonly ObscuredInt q2;
        public readonly ObscuredInt q5;
        public readonly ObscuredInt q10;

        public SmeltCoefficientData(IList<MessagePackObject> data)
        {
            id  = data[0].AsInt32();
            seq = data[1].AsInt32();
            q1  = data[2].AsInt32();
            q2  = data[3].AsInt32();
            q5  = data[4].AsInt32();
            q10 = data[5].AsInt32();
        }
    }
}