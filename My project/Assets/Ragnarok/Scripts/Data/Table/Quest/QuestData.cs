using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class QuestData : IData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredByte quest_category; // QuestCategory
        public readonly ObscuredInt name_id;
        public readonly ObscuredInt description_id;
        public readonly ObscuredInt daily_group;
        public readonly ObscuredShort quest_type; // QuestType
        public readonly ObscuredInt condition_value; // 완료조건 (monsterID, level, zeny 등)
        public readonly ObscuredInt quest_value; // 완료값

        public readonly RewardData[] rewardDatas; // 보상 목록

        public readonly ObscuredInt shortCut_type;
        public readonly ObscuredInt shortCut_value;

        public readonly ObscuredInt event_tutorial; // 시작해야할 튜토리얼
        public readonly ObscuredInt event_content;  // 컨텐츠 해금 조건

        public QuestData(IList<MessagePackObject> data)
        {
            byte index         = 0;
            id                 = data[index++].AsInt32();
            quest_category     = data[index++].AsByte();
            name_id            = data[index++].AsInt32();
            description_id     = data[index++].AsInt32();
            daily_group        = data[index++].AsInt32();
            // 길드 퀘스트의 경우, 그룹에 고유 번호를 지정해준다.
            if (quest_category == QuestCategory.Guild.ToByteValue())
                daily_group    = id;
            quest_type         = data[index++].AsInt16();
            condition_value    = data[index++].AsInt32();
            quest_value        = data[index++].AsInt32();

            byte reward_type   = data[index++].AsByte();
            int reward_value   = data[index++].AsInt32();
            int reward_option  = data[index++].AsInt32();
            int reward_count   = data[index++].AsInt16();

            byte reward_type2  = data[index++].AsByte();
            int reward_value2  = data[index++].AsInt32();
            int reward_option2 = data[index++].AsInt32();
            int reward_count2  = data[index++].AsInt16();

            byte reward_type3  = data[index++].AsByte();
            int reward_value3  = data[index++].AsInt32();
            int reward_option3 = data[index++].AsInt32();
            int reward_count3  = data[index++].AsInt16();

            byte reward_type4  = data[index++].AsByte();
            int reward_value4  = data[index++].AsInt32();
            int reward_option4 = data[index++].AsInt32();
            int reward_count4  = data[index++].AsInt16();

            shortCut_type      = data[index++].AsInt32();
            shortCut_value     = data[index++].AsInt32();
            event_tutorial     = data[index++].AsInt32();
            event_content      = data[index++].AsInt32();

            rewardDatas        = new RewardData[4];
            rewardDatas[0]     = new RewardData(reward_type, reward_value, reward_count, reward_option);
            rewardDatas[1]     = new RewardData(reward_type2, reward_value2, reward_count2, reward_option2);
            rewardDatas[2]     = new RewardData(reward_type3, reward_value3, reward_count3, reward_option3);
            rewardDatas[3]     = new RewardData(reward_type4, reward_value4, reward_count4, reward_option4);
        }
    }
}