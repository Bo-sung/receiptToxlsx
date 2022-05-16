using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="CentralLabDataManager"/>
    /// </summary>
    public sealed class CentralLabData : IData, IDungeonGroup, CentralLabLevelSelectView.IInput
    {
        /// <summary>
        /// 스탯 보너스 최대수치는 200%
        /// </summary>
        private const int MAX_STAT_BUFF_BONUS = 200;

        public readonly ObscuredInt id; // 실험실 고유 아이디
        public readonly ObscuredInt stage_level; // 권장 레벨이면서 클론의 캐릭터 레벨
        private readonly ObscuredInt basic_reward_type; // 참여보상
        private readonly ObscuredInt basic_reward_value; // 참여보상
        private readonly ObscuredInt basic_reward_count; // 참여보상
        private readonly ObscuredInt stage_reward_count; // 스테이지클리어 당 보상

        private DungeonOpenConditionType conditionType;
        private ObscuredInt conditionValue;

        public CentralLabData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id = data[index++].AsInt32();
            stage_level = data[index++].AsInt32();
            basic_reward_type = data[index++].AsInt32();
            basic_reward_value = data[index++].AsInt32();
            basic_reward_count = data[index++].AsInt32();
            stage_reward_count = data[index++].AsInt32();
        }

        public void SetOpenCondition(DungeonOpenConditionType conditionType, int conditionValue)
        {
            this.conditionType = conditionType;
            this.conditionValue = conditionValue;
        }

        DungeonType IDungeonGroup.DungeonType => DungeonType.CentralLab;
        int IDungeonGroup.Difficulty => 0; // 난이도가 존재하지 않음
        string IDungeonGroup.Name => DungeonType.CentralLab.ToText();
        int IDungeonGroup.Id => id;
        DungeonOpenConditionType IOpenConditional.ConditionType => conditionType;
        int IOpenConditional.ConditionValue => conditionValue;

        int CentralLabLevelSelectView.IInput.StageLevel => stage_level;
        int CentralLabLevelSelectView.IInput.PlusRewardCount => stage_reward_count;

        (int monsterId, MonsterType type, int monsterLevel)[] IDungeonGroup.GetMonsterInfos()
        {
            throw new System.NotImplementedException();
        }

        (RewardInfo info, bool isBoss)[] IDungeonGroup.GetRewardInfos()
        {
            throw new System.NotImplementedException();
        }

        public int GetStatBonus(int jobLevel)
        {
            // 공식: +(내 JOB LV - stage_level) * 40%
            // 단, Max 200 %
            int bonus = (jobLevel - stage_level) * 40;
            return MathUtils.Clamp(bonus, 0, MAX_STAT_BUFF_BONUS);
        }

        RewardData CentralLabLevelSelectView.IInput.GetBaseReward()
        {
            return new RewardData(basic_reward_type, basic_reward_value, basic_reward_count);
        }
    }
}