using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    public class PaymentRewardData : IData, MileageSlot.IInput
	{
		public ObscuredInt id;
		public ObscuredInt needPoint;
		public ObscuredInt reward_type;
		public ObscuredInt reward_value;
		public ObscuredInt reward_count;
		public ObscuredInt visable;

		public PaymentRewardData(IList<MessagePackObject> data)
        {
            int index = 0;
			id           =data[index++].AsInt32();
			needPoint    =data[index++].AsInt32();
			reward_type  =data[index++].AsInt32();
			reward_value =data[index++].AsInt32();
			reward_count =data[index++].AsInt32();
			visable      =data[index++].AsInt32();
		}

		public RewardData GetReward()
		{
			return new RewardData(reward_type, reward_value, reward_count);
		}

		public int GetRewardGroupId()
        {
			return reward_value;
		}

		public int GetStep()
        {
			return id;
        }

		public int GetMileage()
        {
			return needPoint;
        }
	}
}