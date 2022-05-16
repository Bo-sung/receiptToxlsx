using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
	/// <summary>
	/// <see cref="FreeFightRewardDataManager"/>
	/// </summary>
	public sealed class FreeFightRewardData : IData, UIFreeFightReward.IInput
	{
		public int id;
		public int kill_count;
		public int reward1_type;
		public int reward1_value;
		public int reward1_count;
		public int reward2_type;
		public int reward2_value;
		public int reward2_count;
		public int event_type;

		int UIFreeFightReward.IInput.KillCount => kill_count;

		public FreeFightRewardData(IList<MessagePackObject> data)
		{
			int index     = 0;
			id            = data[index++].AsInt32();
			kill_count    = data[index++].AsInt32();
			reward1_type  = data[index++].AsInt32();
			reward1_value = data[index++].AsInt32();
			reward1_count = data[index++].AsInt32();
			reward2_type  = data[index++].AsInt32();
			reward2_value = data[index++].AsInt32();
			reward2_count = data[index++].AsInt32();
			event_type    = data[index++].AsInt32();
		}

		public RewardData[] GetRewards()
		{
			return new RewardData[2]
			{
				new RewardData(reward1_type, reward1_value, reward1_count),
				new RewardData(reward2_type, reward2_value, reward2_count),
			};
		}
	}
}