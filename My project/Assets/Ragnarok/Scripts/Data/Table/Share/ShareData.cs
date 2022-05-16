
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;

namespace Ragnarok
{
    public class ShareData : IData
    {
        public readonly ObscuredInt level;
        public readonly ObscuredInt need_exp;
        public readonly ObscuredInt max_battle_score;
        public readonly ObscuredLong need_time;

        public ShareData(IList<MessagePackObject> data)
        {
            byte index = 0;
            level            = data[index++].AsInt32();
            need_exp         = data[index++].AsInt32();
            max_battle_score = data[index++].AsInt32();
            need_time        = data[index++].AsInt64();
        }
    }
}