using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ragnarok
{
    public sealed class MvpRewardUIData : IData
    {
        public ObscuredInt id;
        public ObscuredInt chapter;
        public ObscuredInt markType;
        public ObscuredInt itemReward;

        public MvpRewardUIData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id = data[index++].AsInt32();
            chapter = data[index++].AsInt32();
            markType = data[index++].AsInt32();
            itemReward = data[index++].AsInt32();
        }
    }
}
