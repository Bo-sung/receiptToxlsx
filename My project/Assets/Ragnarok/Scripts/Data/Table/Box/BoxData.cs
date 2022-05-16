using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public class BoxData : IData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt box_type;
        public readonly ObscuredInt group_1;        // 보상줄때 같은 그룹ID가 같은것중에 하나를 준다
        public readonly ObscuredInt rate_1;
        public readonly ObscuredInt type_1;
        public readonly ObscuredInt value_1;
        public readonly ObscuredInt count_1;
        public readonly ObscuredInt group_2;
        public readonly ObscuredInt rate_2;
        public readonly ObscuredInt type_2;
        public readonly ObscuredInt value_2;
        public readonly ObscuredInt count_2;
        public readonly ObscuredInt group_3;
        public readonly ObscuredInt rate_3;
        public readonly ObscuredInt type_3;
        public readonly ObscuredInt value_3;
        public readonly ObscuredInt count_3;
        public readonly ObscuredInt group_4;
        public readonly ObscuredInt rate_4;
        public readonly ObscuredInt type_4;
        public readonly ObscuredInt value_4;
        public readonly ObscuredInt count_4;
        public readonly ObscuredInt group_5;
        public readonly ObscuredInt rate_5;
        public readonly ObscuredInt type_5;
        public readonly ObscuredInt value_5;
        public readonly ObscuredInt count_5;
        public readonly ObscuredInt group_6;
        public readonly ObscuredInt rate_6;
        public readonly ObscuredInt type_6;
        public readonly ObscuredInt value_6;
        public readonly ObscuredInt count_6;
        public readonly ObscuredInt group_7;
        public readonly ObscuredInt rate_7;
        public readonly ObscuredInt type_7;
        public readonly ObscuredInt value_7;
        public readonly ObscuredInt count_7;
        public readonly ObscuredInt group_8;
        public readonly ObscuredInt rate_8;
        public readonly ObscuredInt type_8;
        public readonly ObscuredInt value_8;
        public readonly ObscuredInt count_8;
        public readonly ObscuredInt group_9;
        public readonly ObscuredInt rate_9;
        public readonly ObscuredInt type_9;
        public readonly ObscuredInt value_9;
        public readonly ObscuredInt count_9;
        public readonly ObscuredInt group_10;
        public readonly ObscuredInt rate_10;
        public readonly ObscuredInt type_10;
        public readonly ObscuredInt value_10;
        public readonly ObscuredInt count_10;

        public readonly RewardData[] rewards;

        public BoxData(IList<MessagePackObject> data)
        {
            id       = data[0].AsInt32();
            box_type = data[1].AsInt32();
            group_1  = data[2].AsInt32();
            rate_1   = data[3].AsInt32();
            type_1   = data[4].AsInt32();
            value_1  = data[5].AsInt32();
            count_1  = data[6].AsInt32();
            group_2  = data[7].AsInt32();
            rate_2   = data[8].AsInt32();
            type_2   = data[9].AsInt32();
            value_2  = data[10].AsInt32();
            count_2  = data[11].AsInt32();
            group_3  = data[12].AsInt32();
            rate_3   = data[13].AsInt32();
            type_3   = data[14].AsInt32();
            value_3  = data[15].AsInt32();
            count_3  = data[16].AsInt32();
            group_4  = data[17].AsInt32();
            rate_4   = data[18].AsInt32();
            type_4   = data[19].AsInt32();
            value_4  = data[20].AsInt32();
            count_4  = data[21].AsInt32();
            group_5  = data[22].AsInt32();
            rate_5   = data[23].AsInt32();
            type_5   = data[24].AsInt32();
            value_5  = data[25].AsInt32();
            count_5  = data[26].AsInt32();
            group_6  = data[27].AsInt32();
            rate_6   = data[28].AsInt32();
            type_6   = data[29].AsInt32();
            value_6  = data[30].AsInt32();
            count_6  = data[31].AsInt32();
            group_7  = data[32].AsInt32();
            rate_7   = data[33].AsInt32();
            type_7   = data[34].AsInt32();
            value_7  = data[35].AsInt32();
            count_7  = data[36].AsInt32();
            group_8  = data[37].AsInt32();
            rate_8   = data[38].AsInt32();
            type_8   = data[39].AsInt32();
            value_8  = data[40].AsInt32();
            count_8  = data[41].AsInt32();
            group_9  = data[42].AsInt32();
            rate_9   = data[43].AsInt32();
            type_9   = data[44].AsInt32();
            value_9  = data[45].AsInt32();
            count_9  = data[46].AsInt32();
            group_10 = data[47].AsInt32();
            rate_10  = data[48].AsInt32();
            type_10  = data[49].AsInt32();
            value_10 = data[50].AsInt32();
            count_10 = data[51].AsInt32();

            rewards = new RewardData[10];

            rewards[0] = new RewardData((byte)type_1, value_1, count_1);
            rewards[1] = new RewardData((byte)type_2, value_2, count_2);
            rewards[2] = new RewardData((byte)type_3, value_3, count_3);
            rewards[3] = new RewardData((byte)type_4, value_4, count_4);
            rewards[4] = new RewardData((byte)type_5, value_5, count_5);
            rewards[5] = new RewardData((byte)type_6, value_6, count_6);
            rewards[6] = new RewardData((byte)type_7, value_7, count_7);
            rewards[7] = new RewardData((byte)type_8, value_8, count_8);
            rewards[8] = new RewardData((byte)type_9, value_9, count_9);
            rewards[9] = new RewardData((byte)type_10, value_10, count_10);           
        }

        public bool ContainsGachaBox(int groupId)
        {
            if (value_1 == groupId ||
                value_2 == groupId ||
                value_3 == groupId ||
                value_4 == groupId ||
                value_5 == groupId ||
                value_6 == groupId ||
                value_7 == groupId ||
                value_8 == groupId ||
                value_9 == groupId ||
                value_10 == groupId)
            {
                return true;
            }

            return false;
        }
    }
}
