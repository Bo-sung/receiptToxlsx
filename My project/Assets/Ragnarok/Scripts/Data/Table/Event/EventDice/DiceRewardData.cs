﻿using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="DiceRewardDataManager"/>
    /// </summary>
    public sealed class DiceRewardData : IData, UIDiceCompleteElement.IInput
    {
        public readonly int id;

        public int Seq { get; }
        public RewardData Reward { get; }

        public DiceRewardData(IList<MessagePackObject> data)
        {
            int index = 0;
            id = data[index++].AsInt32();
            Seq = data[index++].AsInt32();
            int reward_type = data[index++].AsInt32();
            int reward_value = data[index++].AsInt32();
            int reward_count = data[index++].AsInt32();
            Reward = new RewardData(reward_type, reward_value, reward_count);
        }
    }
}