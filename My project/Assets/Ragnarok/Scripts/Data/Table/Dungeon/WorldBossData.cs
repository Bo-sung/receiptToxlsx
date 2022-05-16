using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="WorldBossDataManager"/>
    /// </summary>
    public class WorldBossData : IData, IDungeonGroup, IBossMonsterSpawnData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt monster_id;
        public readonly ObscuredInt monster_level;
        public readonly ObscuredInt open_condition;
        public readonly ObscuredInt max_user;
        public readonly ObscuredInt cool_time;
        public readonly ObscuredByte rank1_reward_type;
        public readonly ObscuredInt rank1_reward_value;
        public readonly ObscuredInt rank1_reward_count;
        public readonly ObscuredByte rank2_reward_type;
        public readonly ObscuredInt rank2_reward_value;
        public readonly ObscuredInt rank2_reward_count;
        public readonly ObscuredByte rank3_reward_type;
        public readonly ObscuredInt rank3_reward_value;
        public readonly ObscuredInt rank3_reward_count;
        public readonly ObscuredByte join_reward_type;
        public readonly ObscuredInt join_reward_value;
        public readonly ObscuredInt join_reward_count;
        public readonly ObscuredString scene_name;
        public readonly ObscuredInt scale;
        public readonly ObscuredString scene_change_name;
        public readonly ObscuredString icon_name;

        public WorldBossData(IList<MessagePackObject> data)
        {
            byte index         = 0;
            id                 = data[index++].AsInt32();
            monster_id         = data[index++].AsInt32();
            monster_level      = data[index++].AsInt32();
            open_condition     = data[index++].AsInt32();
            max_user           = data[index++].AsInt32();
            cool_time          = data[index++].AsInt32();
            rank1_reward_type  = data[index++].AsByte();
            rank1_reward_value = data[index++].AsInt32();
            rank1_reward_count = data[index++].AsInt32();
            rank2_reward_type  = data[index++].AsByte();
            rank2_reward_value = data[index++].AsInt32();
            rank2_reward_count = data[index++].AsInt32();
            rank3_reward_type  = data[index++].AsByte();
            rank3_reward_value = data[index++].AsInt32();
            rank3_reward_count = data[index++].AsInt32();
            join_reward_type   = data[index++].AsByte();
            join_reward_value  = data[index++].AsInt32();
            join_reward_count  = data[index++].AsInt32();
            scene_name         = data[index++].AsString();
            scale              = data[index++].AsInt32();
            scene_change_name  = data[index++].AsString();
            icon_name          = data[index++].AsString();
        }

        public DungeonType DungeonType => DungeonType.WorldBoss;
        int IDungeonGroup.Difficulty => 0; // 월드보스에서는 난이도가 존재하지 않음
        string IDungeonGroup.Name => DungeonType.WorldBoss.ToText();
        int IDungeonGroup.Id => id;
        DungeonOpenConditionType IOpenConditional.ConditionType => DungeonOpenConditionType.JobLevel;
        int IOpenConditional.ConditionValue => open_condition;
        int IBossMonsterSpawnData.BossMonsterId => monster_id;
        int IBossMonsterSpawnData.Level => monster_level;
        float IBossMonsterSpawnData.Scale => MathUtils.ToPercentValue(scale);

        (int monsterId, MonsterType type, int monsterLevel)[] IDungeonGroup.GetMonsterInfos()
        {
            return new (int, MonsterType, int)[] { (monster_id, MonsterType.Boss, monster_level) };
        }

        (RewardInfo info, bool isBoss)[] IDungeonGroup.GetRewardInfos()
        {
            List<(RewardInfo info, bool isBoss)> list = new List<(RewardInfo info, bool isBoss)>
            {
                (new RewardInfo(rank1_reward_type, rank1_reward_value, rank1_reward_count), true),
                (new RewardInfo(rank2_reward_type, rank2_reward_value, rank2_reward_count), true),
                (new RewardInfo(rank3_reward_type, rank3_reward_value, rank3_reward_count), true),
                (new RewardInfo(join_reward_type, join_reward_value, join_reward_count), true),
            };
            return list.ToArray();
        }

        public float GetScale()
        {
            return MathUtils.ToPercentValue(scale);
        }
    }
}