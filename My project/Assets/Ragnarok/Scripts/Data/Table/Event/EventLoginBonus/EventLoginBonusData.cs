using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ragnarok
{
    public class EventLoginBonusData : IData, IInfo
    {
        public bool IsInvalidData => id == 0;
        public event Action OnUpdateEvent;

        public readonly ObscuredInt id;
        public readonly ObscuredInt group_no;
        public readonly ObscuredInt day;
        public readonly ObscuredInt reward_type;
        public readonly ObscuredInt reward_value;
        public readonly ObscuredInt reward_count;
        public readonly ObscuredInt special;
        public readonly ObscuredInt dialogue_lid;

        public EventLoginBonusData(IList<MessagePackObject> data)
        {
            int index = 0;
            id = data[index++].AsInt32();
            group_no = data[index++].AsInt32();
            day = data[index++].AsInt32();
            reward_type = data[index++].AsInt32();
            reward_value = data[index++].AsInt32();
            reward_count = data[index++].AsInt32();
            special = data[index++].AsInt32();
            dialogue_lid = data[index++].AsInt32();
        }

        public RewardData GetRewardData()
        {
            return new RewardData(reward_type, reward_value, reward_count);
        }
    }
}
