using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="ForestMonDataManager"/>
    /// </summary>
    public sealed class ForestMonData : IData, ISpawnMonster
    {
        private const int NORMAL_MONSTER_TYPE = 1;
        private const int MIDDLE_BOSS_MONSTER_TYPE = 2;
        private const int BOSS_MONSTER_TYPE = 3;

        public readonly ObscuredInt id;
        public readonly ObscuredInt group_id;
        private readonly ObscuredInt monster_type;
        public readonly ObscuredInt monster_id;
        public readonly ObscuredInt monster_level;
        private readonly ObscuredInt monster_scale;
        public readonly ObscuredInt monster_speed;

        MonsterType ISpawnMonster.Type => GetMonsterType();
        int ISpawnMonster.Id => monster_id;
        int ISpawnMonster.Level => monster_level;
        float ISpawnMonster.Scale => MathUtils.ToPercentValue(monster_scale);

        public ForestMonData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id                     = data[index++].AsInt32();
            group_id               = data[index++].AsInt32();
            monster_type           = data[index++].AsInt32();
            monster_id             = data[index++].AsInt32();
            monster_level          = data[index++].AsInt32();
            monster_scale          = data[index++].AsInt32();
            monster_speed          = data[index++].AsInt32();
            int choice_reward_group_id = data[index++].AsInt32();
            int clear_reward_type      = data[index++].AsInt32();
            int clear_reward_value     = data[index++].AsInt32();
            int clear_reward_count     = data[index++].AsInt32();
        }

        private MonsterType GetMonsterType()
        {
            switch (monster_type)
            {
                case NORMAL_MONSTER_TYPE:
                    return MonsterType.Normal;

                case MIDDLE_BOSS_MONSTER_TYPE:
                case BOSS_MONSTER_TYPE:
                    return MonsterType.Boss;
            }

            return MonsterType.None;
        }

        /// <summary>
        /// 최종 보스 타입
        /// </summary>
        public bool IsBoss()
        {
            return monster_type == BOSS_MONSTER_TYPE;
        }
    }
}