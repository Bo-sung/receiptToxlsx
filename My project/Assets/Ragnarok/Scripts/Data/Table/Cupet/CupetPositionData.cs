using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public class CupetPositionData : IData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt cupet_type_id;
        public readonly ObscuredShort guide_str;
        public readonly ObscuredShort guide_agi;
        public readonly ObscuredShort guide_vit;
        public readonly ObscuredShort guide_int;
        public readonly ObscuredShort guide_dex;
        public readonly ObscuredShort guide_luk;

        public CupetPositionData(IList<MessagePackObject> data)
        {
            id            = data[0].AsInt32();
            cupet_type_id = data[1].AsInt32();
            guide_str     = data[2].AsInt16();
            guide_agi     = data[3].AsInt16();
            guide_vit     = data[4].AsInt16();
            guide_int     = data[5].AsInt16();
            guide_dex     = data[6].AsInt16();
            guide_luk     = data[7].AsInt16();
        }
    }
}