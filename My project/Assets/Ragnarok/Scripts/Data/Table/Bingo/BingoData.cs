using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System;
using System.Collections.Generic;

namespace Ragnarok
{
    public class BingoData : IData
    {
        public ObscuredInt id;
        public ObscuredInt group_id;
        public ObscuredLong start_date;
        public ObscuredLong end_date;
        public ObscuredInt bingo_x;
        public ObscuredInt bingo_y;
        public ObscuredInt collect_id;
        public ObscuredInt reward_type;
        public ObscuredInt reward_id;
        public ObscuredInt reward_size;

        public bool IsLineReward => collect_id == 0;
        public DateTime StartDate => ((long)start_date).ToDateTime();
        public DateTime EndDate => ((long)end_date).ToDateTime();

        public BingoData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id = data[index++].AsInt32();
            group_id = data[index++].AsInt32();
            start_date = data[index++].AsInt64();
            end_date = data[index++].AsInt64();
            bingo_x = data[index++].AsInt32();
            bingo_y = data[index++].AsInt32();
            collect_id = data[index++].AsInt32();
            reward_type = data[index++].AsInt32();
            reward_id = data[index++].AsInt32();
            reward_size = data[index++].AsInt32();
        }

        public RewardData GetCollectItemData()
        {
            return new RewardData(RewardType.Item, collect_id, 1);
        }

        public RewardData GetReward()
        {
            return new RewardData(reward_type, reward_id, reward_size);
        }
    }
}
