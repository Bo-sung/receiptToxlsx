using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="NabihoDataManager"/>
    /// </summary>
    public class NabihoData : IData, NabihoRewardElement.IInput
    {
        public const int GROUP_EQUIPMENT = 1;
        public const int GROUP_BOX = 2;
        public const int GROUP_SPECIAL = 3;

        public int Id { get; }
        public readonly int groupType;
        public int IntimacyCondition { get; }
        public readonly int sort;
        public int NeedMinute { get; }
        public RewardData Reward { get; }

        public NabihoData(IList<MessagePackObject> data)
        {
            byte index = 0;
            Id                 = data[index++].AsInt32();
            groupType          = data[index++].AsInt32();
            IntimacyCondition  = data[index++].AsInt32();
            sort               = data[index++].AsInt32();
            NeedMinute         = data[index++].AsInt32();
            int reward_type    = data[index++].AsInt32();
            int reward_value   = data[index++].AsInt32();
            int reward_count   = data[index++].AsInt32();
            Reward             = new RewardData(reward_type, reward_value, reward_count);
        }
    }
}