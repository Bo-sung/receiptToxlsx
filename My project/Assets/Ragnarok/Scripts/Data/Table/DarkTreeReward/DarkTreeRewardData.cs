using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="DarkTreeRewardDataManager"/>
    /// </summary>
    public sealed class DarkTreeRewardData : IData, DarkTreeRewardElement.IInput
    {
        private readonly ObscuredInt id;
        private readonly ObscuredInt item_type;
        private readonly ObscuredInt item_value;
        private readonly ObscuredInt item_count;
        private readonly ObscuredInt required_item_point;
        public readonly ObscuredLong required_time;

        public int Id => id;

        public DarkTreeRewardData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id                  = data[index++].AsInt32();
            item_type           = data[index++].AsInt32();
            item_value          = data[index++].AsInt32();
            item_count          = data[index++].AsInt32();
            required_item_point = data[index++].AsInt32();
            required_time       = data[index++].AsInt64();
        }

        public RewardData GetReward()
        {
            return new RewardData(item_type, item_value, item_count);
        }

        public int GetMaxPoint()
        {
            return required_item_point;
        }

        public int GetTotalMinutes()
        {
            return (int)System.TimeSpan.FromMilliseconds(required_time).TotalMinutes;
        }
    }
}