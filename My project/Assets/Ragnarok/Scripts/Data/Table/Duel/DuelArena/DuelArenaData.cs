using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="DuelArenaDataManager"/>
    /// </summary>
    public class DuelArenaData : IData, UIDuelArenaInfo.IInput
    {
        public int NameId { get; }
        public int Start { get; }
        public RewardData[] Rewards { get; }

        /// <summary>
        /// 클라 전용
        /// </summary>
        public int Max { get; private set; }

        public DuelArenaData(IList<MessagePackObject> data)
        {
            byte index = 0;
            int id = data[index++].AsInt32();
            NameId = data[index++].AsInt32();
            int arena_type = data[index++].AsInt32();
            Start = data[index++].AsInt32();
            int end_rank = data[index++].AsInt32();
            int reward_type_1 = data[index++].AsInt32();
            int reward_value_1 = data[index++].AsInt32();
            int reward_count_1 = data[index++].AsInt32();
            int reward_type_2 = data[index++].AsInt32();
            int reward_value_2 = data[index++].AsInt32();
            int reward_count_2 = data[index++].AsInt32();

            Rewards = new RewardData[2];
            Rewards[0] = new RewardData(reward_type_1, reward_value_1, reward_count_1);
            Rewards[1] = new RewardData(reward_type_2, reward_value_2, reward_count_2);
        }

        public void SetMax(int max)
        {
            Max = max;
        }
    }
}