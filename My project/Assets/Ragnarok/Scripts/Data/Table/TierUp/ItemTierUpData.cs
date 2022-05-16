using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ragnarok
{
    public class TierUpData : IData, IInfo
    {
        public readonly ObscuredInt tier;
        public readonly ObscuredInt class_bit_type;
        public readonly ObscuredInt cost;
        public readonly ObscuredInt equipment_item_count;
        public readonly ObscuredInt value_1;
        public readonly ObscuredInt count_1;
        public readonly ObscuredInt value_2;
        public readonly ObscuredInt count_2;
        public readonly ObscuredInt value_3;
        public readonly ObscuredInt count_3;
        public readonly ObscuredInt value_4;
        public readonly ObscuredInt count_4;
        public readonly ObscuredInt value_5;
        public readonly ObscuredInt count_5;
        
        public bool IsInvalidData => true;
        public event Action OnUpdateEvent;

        public TierUpData(IList<MessagePackObject> data)
        {
            int index = 0;
            tier = data[index++].AsInt32();
            class_bit_type = data[index++].AsInt32();
            cost = data[index++].AsInt32();
            equipment_item_count = data[index++].AsInt32();
            value_1 = data[index++].AsInt32();
            count_1 = data[index++].AsInt32();
            value_2 = data[index++].AsInt32();
            count_2 = data[index++].AsInt32();
            value_3 = data[index++].AsInt32();
            count_3 = data[index++].AsInt32();
            value_4 = data[index++].AsInt32();
            count_4 = data[index++].AsInt32();
            value_5 = data[index++].AsInt32();
            count_5 = data[index++].AsInt32();
        }

        public IEnumerable<(int, int)> GetMaterials()
        {
            if (value_1 != 0)
                yield return (value_1, count_1);
            if (value_2 != 0)
                yield return (value_2, count_2);
            if (value_3 != 0)
                yield return (value_3, count_3);
            if (value_4 != 0)
                yield return (value_4, count_4);
            if (value_5 != 0)
                yield return (value_5, count_5);
            yield break;
        }
    }
}
