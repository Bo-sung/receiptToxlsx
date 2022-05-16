using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class BotCoefficientData : IData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt q1_value1;
        public readonly ObscuredInt q1_value2;
        public readonly ObscuredInt q2_value1;
        public readonly ObscuredInt q2_value2;
        public readonly ObscuredInt q5_value1;
        public readonly ObscuredInt q5_value2;
        public readonly ObscuredInt q10_value1;
        public readonly ObscuredInt q10_value2;

        public BotCoefficientData(IList<MessagePackObject> data)
        {
            id         = data[0].AsInt32();
            q1_value1  = data[1].AsInt32();
            q1_value2  = data[2].AsInt32();
            q2_value1  = data[3].AsInt32();
            q2_value2  = data[4].AsInt32();
            q5_value1  = data[5].AsInt32();
            q5_value2  = data[6].AsInt32();
            q10_value1 = data[7].AsInt32();
            q10_value2 = data[8].AsInt32();
        }
    }
}
