using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public class CardOptionProbData : IData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt split_value;
        public readonly ObscuredInt tab_order;
        public readonly ObscuredInt weight;

        public CardOptionProbData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id          = data[index++].AsInt32();
            split_value = data[index++].AsInt32();
            tab_order   = data[index++].AsInt32();
            weight      = data[index++].AsInt32();
        }
    }
}