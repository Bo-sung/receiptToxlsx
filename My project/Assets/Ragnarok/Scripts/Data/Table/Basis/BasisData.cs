using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public class BasisData : IData
    {
        public readonly int id;
        public readonly int seq;
        public readonly DataUnit unit;
        public readonly string data;

        public BasisData(IList<MessagePackObject> data, bool isDetail = false)
        {
            if (!isDetail)
            {
                id = data[0].AsInt32();
                unit = (DataUnit)data[1].AsByte();
                this.data = data[2].AsString();
            }
            else
            {
                id = data[0].AsInt32();
                seq = data[1].AsInt32();
                unit = (DataUnit)data[2].AsByte();
                this.data = data[3].AsString();
            }
        }
    }
}
