using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public class EventDuelRewardData : IData, UIDuelReward.IInput
    {
        public const int WORLD_SERVER_REWARD_TYPE = 1;
        public const int SERVER_REWARD_TYPE = 2;

        public ObscuredInt id;
        public ObscuredInt group_id; // 1. 개인(전체 서버), 2. 개인(내 서버)
        public ObscuredInt start_rank;
        public ObscuredInt end_rank;
        public ObscuredInt mail_text;
        public ObscuredInt reward_type_1;
        public ObscuredInt reward_value_1;
        public ObscuredInt reward_count_1;
        public ObscuredInt reward_type_2;
        public ObscuredInt reward_value_2;
        public ObscuredInt reward_count_2;

        public EventDuelRewardData(IList<MessagePackObject> data)
        {
            int index      = 0;
            id             = data[index++].AsInt32();
            group_id       = data[index++].AsInt32();
            start_rank     = data[index++].AsInt32();
            end_rank       = data[index++].AsInt32();
            mail_text      = data[index++].AsInt32();
            reward_type_1  = data[index++].AsInt32();
            reward_value_1 = data[index++].AsInt32();
            reward_count_1 = data[index++].AsInt32();
            reward_type_2  = data[index++].AsInt32();
            reward_value_2 = data[index++].AsInt32();
            reward_count_2 = data[index++].AsInt32();
        }

        public string GetTitle()
        {
            if (end_rank == 0)
                return LocalizeKey._47915.ToText(); // 참여 보상

            if (start_rank == end_rank)
                return LocalizeKey._47914.ToText().Replace(ReplaceKey.RANK, start_rank); // {RANK}위

            return StringBuilderPool.Get()
                .Append(LocalizeKey._47914.ToText().Replace(ReplaceKey.RANK, start_rank)) // {RANK}위
                .Append(" ~ ")
                .Append(LocalizeKey._47914.ToText().Replace(ReplaceKey.RANK, end_rank)) // {RANK}위
                .Release();
        }

        public RewardData[] GetRewards()
        {
            return new RewardData[2]
            {
                new RewardData(reward_type_1, reward_value_1, reward_count_1),
                new RewardData(reward_type_2, reward_value_2, reward_count_2),
            };
        }
    }
}