using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class RewardGroupData : IData, PackageAchieveSlot.IInput
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt group_id;
        public readonly ObscuredInt group_index;
        public readonly ObscuredInt condition_value;
        public readonly ObscuredByte goods_type;
        public readonly ObscuredInt goods_value;
        public readonly ObscuredInt goods_count;

        public RewardData rewardData;

        public RewardGroupData(IList<MessagePackObject> data)
        {
            id              = data[0].AsInt32();
            group_id        = data[1].AsInt32();
            group_index     = data[2].AsInt32();
            condition_value = data[3].AsInt32();
            goods_type      = data[4].AsByte();
            goods_value     = data[5].AsInt32();
            goods_count     = data[6].AsInt32();

            rewardData = new RewardData(goods_type, goods_value, goods_count);
        }

        public RewardData GetReward()
        {
            return new RewardData(goods_type, goods_value, goods_count);
        }    
        
        public int GetStep()
        {
            return group_index;
        }

        public int GetConditionValue()
        {
            return condition_value;
        }

        public int GetGroupId()
        {
            return group_id;
        }
    }
}
