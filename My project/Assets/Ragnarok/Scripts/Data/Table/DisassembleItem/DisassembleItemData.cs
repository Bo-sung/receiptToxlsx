using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="DisassembleItemDataManager"/>
    /// </summary>
    public sealed class DisassembleItemData : IData
    {
        private const byte REWARD_TYPE = (byte)RewardType.Item;

        public readonly ObscuredInt id;
        public readonly ObscuredInt rating;
        public readonly ObscuredInt type;

        public readonly RewardData[] rewards;

        public DisassembleItemData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id         = data[index++].AsInt32();
            rating     = data[index++].AsInt32();
            int rate_1 = data[index++].AsInt32();
            int item_1 = data[index++].AsInt32();
            int min_1  = data[index++].AsInt32();
            int max_1  = data[index++].AsInt32();
            int rate_2 = data[index++].AsInt32();
            int item_2 = data[index++].AsInt32();
            int min_2  = data[index++].AsInt32();
            int max_2  = data[index++].AsInt32();
            int rate_3 = data[index++].AsInt32();
            int item_3 = data[index++].AsInt32();
            int min_3  = data[index++].AsInt32();
            int max_3  = data[index++].AsInt32();
            int rate_4 = data[index++].AsInt32();
            int item_4 = data[index++].AsInt32();
            int min_4  = data[index++].AsInt32();
            int max_4  = data[index++].AsInt32();
            type       = data[index++].AsByte();

            rewards = new RewardData[4];
            rewards[0] = item_1 == 0 ? null : new RewardData(REWARD_TYPE, item_1, min_1);
            rewards[1] = item_2 == 0 ? null : new RewardData(REWARD_TYPE, item_2, min_2);
            rewards[2] = item_3 == 0 ? null : new RewardData(REWARD_TYPE, item_3, min_3);
            rewards[3] = item_4 == 0 ? null : new RewardData(REWARD_TYPE, item_4, min_4);
        }
    }
}
