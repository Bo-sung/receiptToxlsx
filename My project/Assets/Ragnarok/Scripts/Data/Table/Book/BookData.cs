using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ragnarok
{
    public class BookData : IData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt type;
        public readonly ObscuredInt score;
        public readonly ObscuredInt battle_option_type;
        public readonly ObscuredInt value;
        public readonly ObscuredString level_img;

        public int Level { get { return id % 100; } }

        public BookData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id = data[index++].AsInt32();
            type = data[index++].AsInt32();
            score = data[index++].AsInt32();
            battle_option_type = data[index++].AsInt32();
            value = data[index++].AsInt32();
            level_img = data[index++].AsString();
        }

        public BattleOption GetOption()
        {
            return new BattleOption(battle_option_type, value, 0);
        }
    }
}
