using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using Ragnarok.View.League;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="PvERankRewardDataManager"/>
    /// </summary>
    public class PvERankRewardData : IData, UILeagueRankRewardBar.IInput
    {
        public ObscuredInt id;
        public ObscuredInt ranking_value;
        public ObscuredInt name_id;
        public ObscuredInt group_id;
        public readonly RewardData[] rewards;

        public PvERankRewardData(IList<MessagePackObject> data)
        {
            int index = 0;
            id                = data[index++].AsInt32();
            ranking_value     = data[index++].AsInt32();
            int reward1_type  = data[index++].AsByte();
            int reward1_value = data[index++].AsInt32();
            int reward1_count = data[index++].AsInt32();
            int reward2_type  = data[index++].AsByte();
            int reward2_value = data[index++].AsInt32();
            int reward2_count = data[index++].AsInt32();
            int reward3_type  = data[index++].AsByte();
            int reward3_value = data[index++].AsInt32();
            int reward3_count = data[index++].AsInt32();
            int reward4_type  = data[index++].AsByte();
            int reward4_value = data[index++].AsInt32();
            int reward4_count = data[index++].AsInt32();
            name_id           = data[index++].AsInt32();
            group_id          = data[index++].AsInt32();

            rewards = new RewardData[4]
            {
                new RewardData(reward1_type, reward1_value, reward1_count),
                new RewardData(reward2_type, reward2_value, reward2_count),
                new RewardData(reward3_type, reward3_value, reward3_count),
                new RewardData(reward4_type, reward4_value, reward4_count),
            };
        }

        /// <summary>
        /// 참가보상 유무
        /// </summary>
        public bool IsEntryReward()
        {
            return ranking_value == 0; // 참가보상
        }

        public string GetName()
        {
            return name_id.ToText();
        }

        public RewardData GetRewardData(int index)
        {
            return rewards[index];
        }
    }
}