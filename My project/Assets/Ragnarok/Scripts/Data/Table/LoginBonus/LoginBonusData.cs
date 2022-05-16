using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class LoginBonusData : IData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt group_id;
        public readonly ObscuredInt bonus_type;
        public readonly ObscuredInt day;
        public readonly ObscuredInt gift_type;
        public readonly ObscuredInt gift;
        public readonly ObscuredInt gift_option;
        public readonly ObscuredInt item_count;
        public readonly ObscuredInt vip_grade;
        public readonly ObscuredInt effect;
        public readonly ObscuredInt lid;

        public readonly RewardData rewardData;

        public LoginBonusData(IList<MessagePackObject> data)
        {
            id          = data[0].AsInt32();
            group_id    = data[1].AsInt32();
            bonus_type  = data[2].AsInt32();
            day         = data[3].AsInt32();
            gift_type   = data[4].AsInt32();
            gift        = data[5].AsInt32();
            gift_option = data[6].AsInt32();
            item_count  = data[7].AsInt32();
            vip_grade   = data[8].AsInt32();
            effect      = data[9].AsInt32();
            lid         = data[10].AsInt32();

            rewardData = new RewardData((byte)gift_type, gift, item_count, gift_option);
        }
    }
}
