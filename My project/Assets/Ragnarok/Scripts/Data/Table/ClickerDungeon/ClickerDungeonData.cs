using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public class ClickerDungeonData : IData, IDungeonGroup
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt type;
        public readonly ObscuredInt name_id;
        public readonly ObscuredString scene_name;
        public readonly ObscuredInt max_count;
        public readonly ObscuredInt limit_time;
        public readonly ObscuredInt click_count;
        public readonly ObscuredInt click_reward_value;
        public readonly ObscuredInt click_reward_deviation;
        public readonly ObscuredInt open_condition_type;
        public readonly ObscuredInt open_condition_value;
        public readonly ObscuredInt difficulty;
        public readonly ObscuredString bgm;
        public readonly ObscuredInt clicker_type;
        public readonly ObscuredInt normal_monster_id;
        public readonly ObscuredInt normal_monster_count;
        public readonly ObscuredByte reward_type;
        public readonly ObscuredInt reward_value;
        public readonly ObscuredInt reward_count;
        public readonly ObscuredInt dungeon_info_id;

        public ClickerDungeonMonsterType SpawnMonsterType => clicker_type.ToEnum<ClickerDungeonMonsterType>();

        private RewardData rewardData;
        public RewardData RewardData
        {
            get
            {
                if (rewardData == null)
                    rewardData = new RewardData(reward_type, reward_value, reward_count);
                return rewardData;
            }
        }

        public ClickerDungeonData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id = data[index++].AsInt32();
            type = data[index++].AsInt32();
            name_id = data[index++].AsInt32();
            scene_name = data[index++].AsString();
            max_count = data[index++].AsInt32();
            limit_time = data[index++].AsInt32();
            click_count = data[index++].AsInt32();
            click_reward_value = data[index++].AsInt32();
            click_reward_deviation = data[index++].AsInt32();
            open_condition_type = data[index++].AsInt32();
            open_condition_value = data[index++].AsInt32();
            difficulty = data[index++].AsInt32();
            bgm = data[index++].AsString();
            clicker_type = data[index++].AsInt32();
            normal_monster_id = data[index++].AsInt32();
            normal_monster_count = data[index++].AsInt32();
            reward_type = data[index++].AsByte();
            reward_value = data[index++].AsInt32();
            reward_count = data[index++].AsInt32();
            dungeon_info_id = data[index++].AsInt32();
        }

        public int Chapter => 0;

        public DungeonType DungeonType => type.ToEnum<DungeonType>();

        public int Difficulty => difficulty;

        public string Name => name_id.ToText();

        public int Id => id;

        public int GroupId => 0;

        public DungeonOpenConditionType ConditionType => open_condition_type.ToEnum<DungeonOpenConditionType>();

        public int ConditionValue => open_condition_value;

        public (int monsterId, MonsterType type, int monsterLevel)[] GetMonsterInfos()
        {
            return new (int, MonsterType, int)[1] { (0, MonsterType.Normal, 0) };
        }

        public (RewardInfo info, bool isBoss)[] GetRewardInfos()
        {
            RewardType rewardType = default;

            switch (DungeonType)
            {
                case DungeonType.ZenyDungeon:
                    rewardType = RewardType.Zeny;
                    break;

                case DungeonType.ExpDungeon:
                    rewardType = RewardType.LevelExp;
                    break;
            }

            return new (RewardInfo info, bool isBoss)[2]
            {
                (new RewardInfo(rewardType, 0, 0), false),
                (new RewardInfo(reward_type, reward_value, reward_count), false),
            };
        }
    }
}