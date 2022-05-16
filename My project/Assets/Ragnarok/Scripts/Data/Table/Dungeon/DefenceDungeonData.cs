using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="DefenceDungeonDataManager"/>
    /// </summary>
    public class DefenceDungeonData : IData, IDungeonGroup
    {
        public const int MAX_WAVE = 3;
        private const int DROP_ITEM_SIZE = 4;

        public readonly ObscuredInt id;
        public readonly ObscuredInt name_id;
        public readonly ObscuredByte type;
        public readonly ObscuredInt difficulty;
        public readonly ObscuredString scene_name;
        public readonly ObscuredByte open_condition_type;
        public readonly ObscuredInt open_condition_value;
        public readonly ObscuredInt guardian_id;
        public readonly ObscuredInt guardian_level;
        private readonly WaveInfo wave1;
        private readonly WaveInfo wave2;
        private readonly WaveInfo wave3;
        public readonly ObscuredInt base_exp;
        public readonly ObscuredInt job_exp;
        public readonly ObscuredInt zeny;
        public readonly ObscuredInt drop_rate_1;
        public readonly ObscuredInt drop_rate_2;
        public readonly ObscuredInt drop_rate_3;
        public readonly ObscuredInt drop_rate_4;
        public readonly ObscuredInt drop_item_1;
        public readonly ObscuredInt drop_item_2;
        public readonly ObscuredInt drop_item_3;
        public readonly ObscuredInt drop_item_4;
        public readonly ObscuredInt drop_count_1;
        public readonly ObscuredInt drop_count_2;
        public readonly ObscuredInt drop_count_3;
        public readonly ObscuredInt drop_count_4;
        public readonly ObscuredString scene_change_name;
        public readonly ObscuredString icon_name;
        public readonly ObscuredInt dungeon_info_id;

        public DungeonType DungeonType => DungeonType.Defence;
        int IDungeonGroup.Difficulty => difficulty;
        string IDungeonGroup.Name => name_id.ToText();
        int IDungeonGroup.Id => id;
        DungeonOpenConditionType IOpenConditional.ConditionType => open_condition_type.ToEnum<DungeonOpenConditionType>();
        int IOpenConditional.ConditionValue => open_condition_value;

        public DefenceDungeonData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id                      = data[index++].AsInt32();
            name_id                 = data[index++].AsInt32();
            type                    = data[index++].AsByte();
            difficulty              = data[index++].AsInt32();
            scene_name              = data[index++].AsString();
            open_condition_type     = data[index++].AsByte();
            open_condition_value    = data[index++].AsInt32();
            guardian_id             = data[index++].AsInt32();
            guardian_level          = data[index++].AsInt32();
            int wave1_monster_count = data[index++].AsInt32();
            int wave1_monster_level = data[index++].AsInt32();
            int wave1_monster_a_id  = data[index++].AsInt32();
            int wave1_monster_b_id  = data[index++].AsInt32();
            int wave1_monster_c_id  = data[index++].AsInt32();
            int wave1_monster_d_id  = data[index++].AsInt32();
            int wave2_monster_count = data[index++].AsInt32();
            int wave2_monster_level = data[index++].AsInt32();
            int wave2_monster_a_id  = data[index++].AsInt32();
            int wave2_monster_b_id  = data[index++].AsInt32();
            int wave2_monster_c_id  = data[index++].AsInt32();
            int wave2_monster_d_id  = data[index++].AsInt32();
            int wave3_monster_count = data[index++].AsInt32();
            int wave3_monster_level = data[index++].AsInt32();
            int wave3_monster_a_id  = data[index++].AsInt32();
            int wave3_monster_b_id  = data[index++].AsInt32();
            int wave3_monster_c_id  = data[index++].AsInt32();
            int wave3_monster_d_id  = data[index++].AsInt32();
            base_exp                = data[index++].AsInt32();
            job_exp                 = data[index++].AsInt32();
            zeny                    = data[index++].AsInt32();
            drop_rate_1             = data[index++].AsInt32();
            drop_rate_2             = data[index++].AsInt32();
            drop_rate_3             = data[index++].AsInt32();
            drop_rate_4             = data[index++].AsInt32();
            drop_item_1             = data[index++].AsInt32();
            drop_item_2             = data[index++].AsInt32();
            drop_item_3             = data[index++].AsInt32();
            drop_item_4             = data[index++].AsInt32();
            drop_count_1            = data[index++].AsInt32();
            drop_count_2            = data[index++].AsInt32();
            drop_count_3            = data[index++].AsInt32();
            drop_count_4            = data[index++].AsInt32();
            scene_change_name       = data[index++].AsString();
            icon_name               = data[index++].AsString();
            dungeon_info_id         = data[index++].AsInt32();

            wave1 = new WaveInfo(wave1_monster_count, wave1_monster_level, wave1_monster_a_id, wave1_monster_b_id, wave1_monster_c_id, wave1_monster_d_id);
            wave2 = new WaveInfo(wave2_monster_count, wave2_monster_level, wave2_monster_a_id, wave2_monster_b_id, wave2_monster_c_id, wave2_monster_d_id);
            wave3 = new WaveInfo(wave3_monster_count, wave3_monster_level, wave3_monster_a_id, wave3_monster_b_id, wave3_monster_c_id, wave3_monster_d_id);
        }

        public WaveInfo GetWaveInfo(int index)
        {
            switch (index)
            {
                case 0: return wave1;
                case 1: return wave2;
                case 2: return wave3;

                default: throw new System.ArgumentException($"잘못된 인덱스: index = {index}");
            }
        }

        private (int itemId, int itemSize) GetDropItemInfo(int index)
        {
            switch (index)
            {
                case 0: return (drop_item_1, drop_count_1);
                case 1: return (drop_item_2, drop_count_2);
                case 2: return (drop_item_3, drop_count_3);
                case 3: return (drop_item_4, drop_count_4);

                default: throw new System.ArgumentException($"잘못된 인덱스: index = {index}");
            }
        }

        (int monsterId, MonsterType type, int monsterLevel)[] IDungeonGroup.GetMonsterInfos()
        {
            BetterList<(int,int)> list = new BetterList<(int,int)>();
            for (int i = 0; i < MAX_WAVE; i++)
            {
                WaveInfo waveInfo = GetWaveInfo(i);

                for (int j = 0; j < WaveInfo.ZONE_COUNT; j++)
                {
                    int monsterId = waveInfo.GetMonsterId(j);
                    int monsterLevel = waveInfo.GetMonsterLevel();
                    if (monsterId == 0)
                        continue;

                    if (list.Contains((monsterId, monsterLevel)))
                        continue;

                    list.Add((monsterId, monsterLevel));
                }
            }

            (int, MonsterType, int)[] output = new (int, MonsterType, int)[list.size];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = (list[i].Item1, MonsterType.Normal, list[i].Item2);
            }
            return output;
        }

        (RewardInfo info, bool isBoss)[] IDungeonGroup.GetRewardInfos()
        {
            BetterList<(int, int)> list = new BetterList<(int, int)>();

            for (int i = 0; i < DROP_ITEM_SIZE; i++)
            {
                (int, int) tuple = GetDropItemInfo(i);
                if (tuple.Item1 == 0 || tuple.Item2 == 0)
                    continue;

                if (list.Contains(tuple))
                    continue;

                list.Add(tuple);
            }

            (RewardInfo, bool)[] output = new (RewardInfo, bool)[list.size];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = (new RewardInfo(RewardType.Item, list[i].Item1, list[i].Item2), false);
            }
            return output;
        }

        public struct WaveInfo
        {
            public const int ZONE_COUNT = 4;

            public readonly ObscuredInt monsterCount;
            public readonly ObscuredInt monsterLevel;
            private readonly ObscuredInt monsterId_zoneA;
            private readonly ObscuredInt monsterId_zoneB;
            private readonly ObscuredInt monsterId_zoneC;
            private readonly ObscuredInt monsterId_zoneD;

            public WaveInfo(int monsterCount, int monsterLevel, int monsterId_zoneA, int monsterId_zoneB, int monsterId_zoneC, int monsterId_zoneD)
            {
                this.monsterCount = monsterCount;
                this.monsterLevel = monsterLevel;
                this.monsterId_zoneA = monsterId_zoneA;
                this.monsterId_zoneB = monsterId_zoneB;
                this.monsterId_zoneC = monsterId_zoneC;
                this.monsterId_zoneD = monsterId_zoneD;
            }

            public int GetMonsterId(int index)
            {
                switch (index)
                {
                    case 0: return monsterId_zoneA;
                    case 1: return monsterId_zoneB;
                    case 2: return monsterId_zoneC;
                    case 3: return monsterId_zoneD;

                    default: throw new System.ArgumentException($"잘못된 인덱스: index = {index}");
                }
            }

            public int GetMonsterLevel()
            {
                return monsterLevel;
            }
        }
    }
}