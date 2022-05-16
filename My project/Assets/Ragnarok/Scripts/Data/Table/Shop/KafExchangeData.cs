using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="KafExchangeDataManager"/>
    /// </summary>
    public sealed class KafExchangeData : IData
    {
        public readonly int id;
        public readonly int sort;
        public readonly RewardData material;
        public readonly RewardData result;
        public readonly int type; // 0: 카프라 교환소, 1:귀금속 전달, 2:긴급 도움 요청

        public KafExchangeData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id                 = data[index++].AsInt32();
            sort               = data[index++].AsInt32();
            int meterial_type  = data[index++].AsInt32();
            int meterial_value = data[index++].AsInt32();
            int meterial_count = data[index++].AsInt32();
            int result_type    = data[index++].AsInt32();
            int result_value   = data[index++].AsInt32();
            int result_count   = data[index++].AsInt32();
            type               = data[index++].AsInt32();

            material = new RewardData(meterial_type, meterial_value, meterial_count);
            result = new RewardData(result_type, result_value, result_count);
        }
    }
}