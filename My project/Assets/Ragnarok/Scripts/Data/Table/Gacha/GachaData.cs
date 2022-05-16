using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// group_type이 1일 때,
    /// pay_type은 거래 여부로 사용합니다.
    /// pay_type이 -1이면 해당 가챠에서 등장하는
    /// 아이템은 거래 불가입니다.
    /// 단 해당 아이템은 겹쳐지지 않는 아이템
    /// 이어야 합니다. 장비 or 카드
    /// </summary>
    public sealed class GachaData : IData
        , UISpecialRouletteElement.IInput
        , UIRewardListElement.IInput
        , UIMonopolyTile.IInput
    {
        public readonly ObscuredInt id;
        public readonly ObscuredByte group_type;
        public readonly ObscuredInt group_id;
        public readonly ObscuredInt rate;
        public readonly ObscuredInt reward_type;
        public readonly ObscuredInt reward_value;
        public readonly ObscuredInt reward_count;
        public readonly ObscuredSByte pay_type;
        public readonly ObscuredInt pay_value;

        public int Id => id;

        public RewardData Reward => GetRewardData();

        public bool IsComplete { get; private set; }

        int UISpecialRouletteElement.IInput.Rate => rate;
        int UIRewardListElement.IInput.Rate => pay_value;

        public int TotalRate { get; private set; }

        public GachaData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id           = data[index++].AsInt32();
            group_type   = data[index++].AsByte();
            group_id     = data[index++].AsInt32();
            rate         = data[index++].AsInt32();
            reward_type  = data[index++].AsInt32();
            reward_value = data[index++].AsInt32();
            reward_count = data[index++].AsInt32();
            pay_type     = data[index++].AsSByte();
            pay_value    = data[index++].AsInt32();
        }

        public RewardData GetRewardData()
        {
            return new RewardData(reward_type, reward_value, reward_count);
        }

        public void SetReceived(bool isReceived)
        {
            IsComplete = isReceived;
        }

        public void SetTotalRate(int totalRate)
        {
            TotalRate = totalRate;
        }
    }
}
