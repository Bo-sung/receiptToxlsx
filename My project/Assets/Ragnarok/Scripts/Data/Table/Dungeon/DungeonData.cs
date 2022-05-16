using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="DungeonDataManager"/>
    /// </summary>
    [System.Obsolete("사용X")]
    public class DungeonData : IData, INormalMonsterSpawnData, IBossMonsterSpawnData
    {
        /// <summary>
        /// 몬스터 인덱스 최대 개수
        /// </summary>
        public const int MAX_MONSTER_INDEX = 4;

        /// <summary>
        /// 보상 인덱스 최대 개수
        /// </summary>
        public const int MAX_REWARD_INDEX = 8;

        /// <summary>
        /// 고유값
        /// </summary>
        public readonly ObscuredInt id;
        public readonly ObscuredInt group_id;
        public readonly ObscuredInt chapter;
        public readonly ObscuredInt name_id;
        public readonly ObscuredInt type;
        public readonly ObscuredInt difficulty;
        public readonly ObscuredString scene_name;
        public readonly ObscuredInt boss_monster_scale;
        public readonly ObscuredInt boss_monster_level;
        public readonly ObscuredInt normal_monster_level;
        public readonly ObscuredInt open_condition_type;
        public readonly ObscuredInt open_condition_value;
        public readonly ObscuredInt boss_id;
        public readonly ObscuredInt wave_bump;
        public readonly ObscuredInt wave_cost;
        public readonly ObscuredInt spawn_rate_1;
        public readonly ObscuredInt spawn_rate_2;
        public readonly ObscuredInt spawn_rate_3;
        public readonly ObscuredInt spawn_rate_4;
        public readonly ObscuredInt monster_id_1;
        public readonly ObscuredInt monster_id_2;
        public readonly ObscuredInt monster_id_3;
        public readonly ObscuredInt monster_id_4;
        public readonly ObscuredInt base_exp;
        public readonly ObscuredInt job_exp;
        public readonly ObscuredInt zeny;
        public readonly ObscuredInt drop_rate_1;
        public readonly ObscuredInt drop_rate_2;
        public readonly ObscuredInt drop_rate_3;
        public readonly ObscuredInt drop_rate_4;
        public readonly ObscuredInt drop_rate_5;
        public readonly ObscuredInt drop_rate_6;
        public readonly ObscuredInt drop_rate_7;
        public readonly ObscuredInt drop_rate_8;
        public readonly ObscuredInt drop_1;
        public readonly ObscuredInt drop_2;
        public readonly ObscuredInt drop_3;
        public readonly ObscuredInt drop_4;
        public readonly ObscuredInt drop_5;
        public readonly ObscuredInt drop_6;
        public readonly ObscuredInt drop_7;
        public readonly ObscuredInt drop_8;
        public readonly ObscuredString scene_theme_name;
        public readonly ObscuredString icon_name;

        /// <summary>
        /// 일반 몬스터 소환 가능한 최대 인덱스
        /// </summary>
        int INormalMonsterSpawnData.MaxIndex => 4;

        /// <summary>
        /// 일반 몬스터 소환 비용
        /// </summary
        int INormalMonsterSpawnData.Cost => wave_cost;

        /// <summary>
        /// 일반 몬스터 소환 레벨
        /// </summary>
        int INormalMonsterSpawnData.Level => normal_monster_level;

        /// <summary>
        /// 보스 몬스터 소환 아이디
        /// </summary>
        int IBossMonsterSpawnData.BossMonsterId => boss_id;

        /// <summary>
        /// 보스 몬스터 소환 레벨
        /// </summary>
        int IBossMonsterSpawnData.Level => boss_monster_level;

        /// <summary>
        /// 보스 몬스터 소환 크기
        /// </summary>
        float IBossMonsterSpawnData.Scale => boss_monster_scale;

        public DungeonData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id                   = data[index++].AsInt32();
            group_id             = data[index++].AsInt32();
            chapter              = data[index++].AsInt32();
            name_id              = data[index++].AsInt32();
            type                 = data[index++].AsInt32();
            difficulty           = data[index++].AsInt32();
            scene_name           = data[index++].AsString();
            boss_monster_scale   = data[index++].AsInt32();
            boss_monster_level   = data[index++].AsInt32();
            normal_monster_level = data[index++].AsInt32();
            open_condition_type  = data[index++].AsInt32();
            open_condition_value = data[index++].AsInt32();
            boss_id              = data[index++].AsInt32();
            wave_bump            = data[index++].AsInt32();
            wave_cost            = data[index++].AsInt32();
            spawn_rate_1         = data[index++].AsInt32();
            spawn_rate_2         = data[index++].AsInt32();
            spawn_rate_3         = data[index++].AsInt32();
            spawn_rate_4         = data[index++].AsInt32();
            monster_id_1         = data[index++].AsInt32();
            monster_id_2         = data[index++].AsInt32();
            monster_id_3         = data[index++].AsInt32();
            monster_id_4         = data[index++].AsInt32();
            base_exp             = data[index++].AsInt32();
            job_exp              = data[index++].AsInt32();
            zeny                 = data[index++].AsInt32();
            drop_rate_1          = data[index++].AsInt32();
            drop_rate_2          = data[index++].AsInt32();
            drop_rate_3          = data[index++].AsInt32();
            drop_rate_4          = data[index++].AsInt32();
            drop_rate_5          = data[index++].AsInt32();
            drop_rate_6          = data[index++].AsInt32();
            drop_rate_7          = data[index++].AsInt32();
            drop_rate_8          = data[index++].AsInt32();
            drop_1               = data[index++].AsInt32();
            drop_2               = data[index++].AsInt32();
            drop_3               = data[index++].AsInt32();
            drop_4               = data[index++].AsInt32();
            drop_5               = data[index++].AsInt32();
            drop_6               = data[index++].AsInt32();
            drop_7               = data[index++].AsInt32();
            drop_8               = data[index++].AsInt32();
            scene_theme_name    = data[index++].AsString();
            icon_name            = data[index++].AsString();
        }

        /// <summary>
        /// 일반 몬스터 인덱스에 해당하는 소환 정보
        /// </summary>
        (int monsterId, int spawnRate) INormalMonsterSpawnData.GetSpawnInfo(int index)
        {
            switch (index)
            {
                case 0: return (monster_id_1, spawn_rate_1);
                case 1: return (monster_id_2, spawn_rate_2);
                case 2: return (monster_id_3, spawn_rate_3);
                case 3: return (monster_id_4, spawn_rate_4);

                default: throw new System.ArgumentException($"잘못된 인덱스: index = {index}");
            }
        }
    }
}