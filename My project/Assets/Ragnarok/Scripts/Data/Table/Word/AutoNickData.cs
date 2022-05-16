using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public class AutoNickData : IData
    {
        public readonly int id;
        public readonly string word;
        public readonly int language_type;

        public AutoNickData(IList<MessagePackObject> data)
        {
            id = data[0].AsInt32();
            word = data[1].AsString();
            language_type = data[2].AsInt32();
        }
    }
}