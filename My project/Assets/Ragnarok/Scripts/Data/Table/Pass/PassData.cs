using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="PassDataManager"/>
    /// </summary>
    public class PassData : IData, IPassData
    {
        private readonly ObscuredInt id;
        private readonly ObscuredInt pass_lv;
        private readonly ObscuredInt need_exp;
        private readonly RewardData[] freeRewards;
        private readonly RewardData[] payRewards;

        public int Id => id;
        public int PassLevel => pass_lv;
        public int NeedExp => need_exp;

        public PassData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id = data[index++].AsInt32();
            pass_lv = data[index++].AsInt32();
            need_exp = data[index++].AsInt32();
            int free_reward_type = data[index++].AsInt32();
            int free_reward_value = data[index++].AsInt32();
            int free_reward_count = data[index++].AsInt32();
            int pay_reward_type = data[index++].AsInt32();
            int pay_reward_value = data[index++].AsInt32();
            int pay_reward_count = data[index++].AsInt32();

            freeRewards = new RewardData[1]
            {
                new RewardData(free_reward_type, free_reward_value, free_reward_count),
            };

            payRewards = new RewardData[1]
            {
                new RewardData(pay_reward_type, pay_reward_value, pay_reward_count),
            };
        }

        public RewardData[] GetFreeRewards()
        {
            return freeRewards;
        }

        public RewardData[] GetPayRewards()
        {
            return payRewards;
        }
    }
}