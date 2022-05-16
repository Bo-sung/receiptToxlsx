using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="TimePatrolStageDataManager"/>
    /// </summary>
    public class TimePatrolStageData : IData, INormalMonsterSpawnData
    {
        private const int NORMAL_MONSTER_MAX_INDEX = 4;


        public ObscuredInt id;
        public ObscuredInt level;
        public ObscuredInt zone_id;
        public ObscuredInt name_id;
        public ObscuredString scene_name;
        public ObscuredInt normal_monster_cost;
        public ObscuredInt normal_monster_level;
        public ObscuredInt normal_base_exp;
        public ObscuredInt normal_job_exp;
        public ObscuredInt normal_zeny;
        public ObscuredInt normal_monster_id;
        public ObscuredInt normal_drop_rate_1;
        public ObscuredInt normal_drop_rate_2;
        public ObscuredInt normal_drop_rate_3;
        public ObscuredInt normal_drop_rate_4;
        public ObscuredInt normal_drop_1;
        public ObscuredInt normal_drop_2;
        public ObscuredInt normal_drop_3;
        public ObscuredInt normal_drop_4;
        public ObscuredString bgm;
        public ObscuredInt required_core_level;

        int INormalMonsterSpawnData.MaxIndex => NORMAL_MONSTER_MAX_INDEX;
        int INormalMonsterSpawnData.Cost => normal_monster_cost;
        int INormalMonsterSpawnData.Level => normal_monster_level;

        public TimePatrolStageData(IList<MessagePackObject> data)
        {
            int index = 0;
            id                   = data[index++].AsInt32();
            level                = data[index++].AsInt32();
            zone_id              = data[index++].AsInt32();
            name_id              = data[index++].AsInt32();
            scene_name           = data[index++].AsString();
            normal_monster_cost  = data[index++].AsInt32();
            normal_monster_level = data[index++].AsInt32();
            normal_base_exp      = data[index++].AsInt32();
            normal_job_exp       = data[index++].AsInt32();
            normal_zeny          = data[index++].AsInt32();
            normal_monster_id    = data[index++].AsInt32();
            normal_drop_rate_1   = data[index++].AsInt32();
            normal_drop_rate_2   = data[index++].AsInt32();
            normal_drop_rate_3   = data[index++].AsInt32();
            normal_drop_rate_4   = data[index++].AsInt32();
            normal_drop_1        = data[index++].AsInt32();
            normal_drop_2        = data[index++].AsInt32();
            normal_drop_3        = data[index++].AsInt32();
            normal_drop_4        = data[index++].AsInt32();
            bgm                  = data[index++].AsString();
            required_core_level  = data[index++].AsInt32();
        }

        /// <summary>
        /// 일반 몬스터 인덱스에 해당하는 소환 정보
        /// </summary>
        (int monsterId, int spawnRate) INormalMonsterSpawnData.GetSpawnInfo(int index)
        {
            switch (index)
            {
                case 0: return (normal_monster_id, 100);
                case 1: return (normal_monster_id, 100);
                case 2: return (normal_monster_id, 100);
                case 3: return (normal_monster_id, 100);

                default: throw new System.ArgumentException($"잘못된 인덱스: index = {index}");
            }
        }
    }
}