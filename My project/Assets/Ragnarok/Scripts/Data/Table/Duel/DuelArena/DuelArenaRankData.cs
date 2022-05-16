using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="DuelArenaRankDataManager"/>
    /// </summary>
    public class DuelArenaRankData : IData, UIDuelReward.IInput
    {
        private readonly int name_id;
        public readonly int ranking_value;
        private readonly RewardData[] rewards;

        public DuelArenaRankData(IList<MessagePackObject> data)
        {
            byte index = 0;
            int id = data[index++].AsInt32();
            name_id = data[index++].AsInt32();
            ranking_value = data[index++].AsInt32();
            int reward_type_1 = data[index++].AsInt32();
            int reward_value_1 = data[index++].AsInt32();
            int reward_count_1 = data[index++].AsInt32();
            int reward_type_2 = data[index++].AsInt32();
            int reward_value_2 = data[index++].AsInt32();
            int reward_count_2 = data[index++].AsInt32();
            int reward_type_3 = data[index++].AsInt32();
            int reward_value_3 = data[index++].AsInt32();
            int reward_count_3 = data[index++].AsInt32();
            int reward_type_4 = data[index++].AsInt32();
            int reward_value_4 = data[index++].AsInt32();
            int reward_count_4 = data[index++].AsInt32();

            rewards = new RewardData[4]
            {
                new RewardData(reward_type_1, reward_value_1, reward_count_1),
                new RewardData(reward_type_2, reward_value_2, reward_count_2),
                new RewardData(reward_type_3, reward_value_3, reward_count_3),
                new RewardData(reward_type_4, reward_value_4, reward_count_4),
            };
        }

        /// <summary>
        /// 참가보상 유무
        /// </summary>
        public bool IsEntryReward()
        {
            return ranking_value <= 0; // 참가보상
        }

        public string GetTitle()
        {
            return name_id.ToText();
        }

        public RewardData[] GetRewards()
        {
            return rewards;
        }
    }
}