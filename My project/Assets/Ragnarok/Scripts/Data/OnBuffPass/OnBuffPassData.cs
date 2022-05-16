using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="OnBuffPassDataManager"/>
    /// </summary>
    public class OnBuffPassData : IData, IPassData
    {
        private readonly ObscuredInt id;
        private readonly ObscuredInt pass_lv;
        private readonly ObscuredInt need_exp;
        private readonly RewardData[] freeRewards;
        private readonly RewardData[] payRewards;

        public int Id => id;
        public int PassLevel => pass_lv;
        public int NeedExp => need_exp;

        public OnBuffPassData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id = data[index++].AsInt32();
            pass_lv = data[index++].AsInt32();
            need_exp = data[index++].AsInt32();
            int free_reward_type = data[index++].AsInt32();
            int free_reward_value = data[index++].AsInt32();
            int free_reward_count = data[index++].AsInt32();
            int pay_reward_type_1 = data[index++].AsInt32();
            int pay_reward_value_1 = data[index++].AsInt32();
            int pay_reward_count_1 = data[index++].AsInt32();
            int pay_reward_type_2 = data[index++].AsInt32();
            int pay_reward_value_2 = data[index++].AsInt32();
            int pay_reward_count_2 = data[index++].AsInt32();

            freeRewards = new RewardData[1]
            {
                new RewardData(free_reward_type, free_reward_value, free_reward_count),
            };

            payRewards = new RewardData[2]
            {
                new RewardData(pay_reward_type_1, pay_reward_value_1, pay_reward_count_1),
                new RewardData(pay_reward_type_2, pay_reward_value_2, pay_reward_count_2),
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