using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using Ragnarok.View.League;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="PvETierDataManager"/>
    /// </summary>
    public class PvETierData : IData, UILeagueGradeRewardBar.IInput
    {
        public ObscuredInt id;
        //public ObscuredString description;
        public ObscuredInt tier_value;
        public ObscuredInt name_id;
        public readonly RewardData[] rewards;

        public PvETierData(IList<MessagePackObject> data)
        {
            int index = 0;
            id = data[index++].AsInt32();
            //description = data[index++].AsString();
            tier_value = data[index++].AsInt32();
            int reward1_type = data[index++].AsByte();
            int reward1_value = data[index++].AsInt32();
            int reward1_count = data[index++].AsInt32();
            int reward2_type = data[index++].AsByte();
            int reward2_value = data[index++].AsInt32();
            int reward2_count = data[index++].AsInt32();
            int reward3_type = data[index++].AsByte();
            int reward3_value = data[index++].AsInt32();
            int reward3_count = data[index++].AsInt32();
            int reward4_type = data[index++].AsByte();
            int reward4_value = data[index++].AsInt32();
            int reward4_count = data[index++].AsInt32();
            name_id = data[index++].AsInt32();

            rewards = new RewardData[4]
            {
                new RewardData(reward1_type, reward1_value, reward1_count),
                new RewardData(reward2_type, reward2_value, reward2_count),
                new RewardData(reward3_type, reward3_value, reward3_count),
                new RewardData(reward4_type, reward4_value, reward4_count),
            };
        }

        public string GetName()
        {
            return name_id.ToText();
        }

        public string GetIconName()
        {
            return string.Format("RankTier{0:D2}", id);
        }

        public int GetNeedPoint()
        {
            return tier_value;
        }

        public RewardData GetRewardData(int index)
        {
            return rewards[index];
        }
    }
}