using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;
using System.Linq;

namespace Ragnarok
{
    public class JobLevelRewardData : IData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt mail_des_id;
        public readonly ObscuredInt job_level;
        public readonly ObscuredInt reward_type;
        public readonly ObscuredInt reward_value;
        public readonly ObscuredInt reward_count;
        public readonly ObscuredInt group_id;

        public JobLevelRewardData(IList<MessagePackObject> data)
        {
            int index = 0;
            id              = data[index++].AsInt32();
            mail_des_id     = data[index++].AsInt32();
            job_level       = data[index++].AsInt32();
            reward_type     = data[index++].AsInt32();
            reward_value    = data[index++].AsInt32();
            reward_count    = data[index++].AsInt32();
            group_id        = data[index++].AsInt32();
        }
    }
}