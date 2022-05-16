using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public class WordData : IData
    {
        public readonly int id;
        public readonly string word;

        public WordData(IList<MessagePackObject> data)
        {
            id = data[0].AsInt32();
            word = data[1].AsString();
        }
    }
}