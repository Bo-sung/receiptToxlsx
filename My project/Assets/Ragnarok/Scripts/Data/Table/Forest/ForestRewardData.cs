using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="ForestRewardDataManager"/>
    /// </summary>
    public sealed class ForestRewardData : IData, UIForestMazeSkillElement.IInput
    {
        private const int REWARD_CHOICE_TYPE = 0; // 보상아이템 (선택한 아이템을 퇴장 시에 모두 획득)
        private const int BATTLE_CHOICE_TYPE_1 = 1; // 공략아이템 (초승달 반지)
        private const int BATTLE_CHOICE_TYPE_2 = 2; // 공략아이템 (마제스틱 고우트)
        private const int BATTLE_CHOICE_TYPE_3 = 4; // 공략아이템 (이그드라실의 씨앗)
        private const int BATTLE_CHOICE_TYPE_4 = 8; // 공략아이템 (악마의 뿔)

        public readonly ObscuredInt id;
        public readonly ObscuredInt choice_type;
        public readonly ObscuredInt reward_value;
        public readonly ObscuredInt reward_count;

        int UIForestMazeSkillElement.IInput.Id => id;
        RewardData UIForestMazeSkillElement.IInput.Reward => GetReward();
        UISkillInfo.IInfo UIForestMazeSkillElement.IInput.Skill => GetSkill();

        public ForestRewardData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id           = data[index++].AsInt32();
            int group_id = data[index++].AsInt32();
            choice_type  = data[index++].AsInt32();
            int rate     = data[index++].AsInt32();
            reward_value = data[index++].AsInt32();
            reward_count = data[index++].AsInt32();
        }

        public RewardData GetReward()
        {
            if (choice_type == REWARD_CHOICE_TYPE)
                return new RewardData(RewardType.Item, reward_value, reward_count);

            return null;
        }

        public ForestMazeSkill GetSkill()
        {
            switch (choice_type)
            {
                case BATTLE_CHOICE_TYPE_1: return ForestMazeSkill.SKILL_1;
                case BATTLE_CHOICE_TYPE_2: return ForestMazeSkill.SKILL_2;
                case BATTLE_CHOICE_TYPE_3: return ForestMazeSkill.SKILL_3;
                case BATTLE_CHOICE_TYPE_4: return ForestMazeSkill.SKILL_4;
            }

            return null;
        }
    }
}