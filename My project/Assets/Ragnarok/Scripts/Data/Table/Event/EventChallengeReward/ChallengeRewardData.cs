using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class ChallengeRewardData : IData
    {
        public const int SCORE_RANK_GROUP = 1; // 점수 랭킹
        public const int KILL_RANK_GROUP = 2; // 처치 랭킹

        public readonly int groupId;
        public readonly int startRank;
        public readonly int endRank;
        public readonly RewardData reward;

        public ChallengeRewardData(IList<MessagePackObject> data)
        {
            int index = 0;
            int id          = data[index++].AsInt32();
            groupId         = data[index++].AsInt32();
            startRank       = data[index++].AsInt32();
            endRank         = data[index++].AsInt32();
            int mailText    = data[index++].AsInt32();
            int rewardType  = data[index++].AsInt32();
            int rewardValue = data[index++].AsInt32();
            int rewardCount = data[index++].AsInt32();
            reward = new RewardData(rewardType, rewardValue, rewardCount);
        }
    }
}