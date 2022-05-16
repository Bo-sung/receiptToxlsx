using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public class SmeltRateData : IData
    {
        public readonly ObscuredInt lv;
        public readonly ObscuredInt[] rating_success_rate;       
        public readonly ObscuredInt protect_rate;
        public readonly ObscuredInt down1_rate;
        public readonly ObscuredInt down2_rate;
        public readonly ObscuredInt destroy_rate;

        public SmeltRateData(IList<MessagePackObject> data)
        {
            lv                   = data[0].AsInt32();
            rating_success_rate  = new ObscuredInt[]
            {
                data[1].AsInt32(),
                data[2].AsInt32(),
                data[3].AsInt32(),
                data[4].AsInt32(),
                data[5].AsInt32(),
                data[6].AsInt32(),
            };            
            protect_rate         = data[7].AsInt32();
            down1_rate           = data[8].AsInt32();
            down2_rate           = data[9].AsInt32();
            destroy_rate         = data[10].AsInt32();

        }
    }
}
