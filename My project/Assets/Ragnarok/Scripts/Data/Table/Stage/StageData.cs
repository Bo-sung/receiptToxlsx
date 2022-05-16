using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="StageDataManager"/>
    /// </summary>
    public sealed class StageData : IData, INormalMonsterSpawnData, IBossMonsterSpawnData, IStageInfoPopupData, UIBossComing.IInput
    {
        private const int NORMAL_MONSTER_MAX_INDEX = 4;
        private const int MVP_DROP_ITEM_COUNT = 4;
        private const int NORMAL_DROP_ITEM_COUNT = 4;

        public readonly ObscuredInt id;
        public readonly ObscuredInt chapter;
        public readonly ObscuredInt name_id;
        public readonly ObscuredString scene_name;
        [System.Obsolete]
        public readonly ObscuredInt stage_group; // 필드 그룹
        [System.Obsolete]
        public readonly ObscuredInt stage_group_name_id;

        public readonly ObscuredInt boss_monster_scale;
        public readonly ObscuredInt boss_monster_level;
        public readonly ObscuredInt boss_monster_id;
        public readonly ObscuredInt boss_base_exp;
        public readonly ObscuredInt boss_job_exp;
        public readonly ObscuredInt boss_zeny;
        public readonly ObscuredInt boss_drop_rate_1;
        public readonly ObscuredInt boss_drop_rate_2;
        public readonly ObscuredInt boss_drop_rate_3;
        public readonly ObscuredInt boss_drop_rate_4;
        public readonly ObscuredInt boss_drop_1;
        public readonly ObscuredInt boss_drop_2;
        public readonly ObscuredInt boss_drop_3;
        public readonly ObscuredInt boss_drop_4;

        public readonly ObscuredInt normal_monster_cost;
        public readonly ObscuredInt normal_monster_level;
        public readonly ObscuredInt normal_base_exp;
        public readonly ObscuredInt normal_job_exp;
        public readonly ObscuredInt normal_zeny;
        public readonly ObscuredInt normal_monster_rate_1;
        public readonly ObscuredInt normal_monster_rate_2;
        public readonly ObscuredInt normal_monster_rate_3;
        public readonly ObscuredInt normal_monster_rate_4;
        public readonly ObscuredInt normal_monster_id_1;
        public readonly ObscuredInt normal_monster_id_2;
        public readonly ObscuredInt normal_monster_id_3;
        public readonly ObscuredInt normal_monster_id_4;
        public readonly ObscuredInt normal_drop_rate_1;
        public readonly ObscuredInt normal_drop_rate_2;
        public readonly ObscuredInt normal_drop_rate_3;
        public readonly ObscuredInt normal_drop_rate_4;
        public readonly ObscuredInt normal_drop_1;
        public readonly ObscuredInt normal_drop_2;
        public readonly ObscuredInt normal_drop_3;
        public readonly ObscuredInt normal_drop_4;
        [System.Obsolete]
        public readonly ObscuredString scene_change_name;
        [System.Obsolete]
        public readonly ObscuredString icon_name;
        public readonly ObscuredInt maze_map;                   // 미로맵 ID
        [System.Obsolete]
        public readonly ObscuredInt maze_map_data;              // 미로맵 몬스터 생성 위치
        public readonly ObscuredByte maze_monster_type;         // 몬스터 타입 (1 = 일반 몬스터 [모두 처치 시 최종 몬스터 도전 가능]), (2 = 최종 몬스터 [처치 시 다음 미로 오픈])
        [System.Obsolete]
        public readonly ObscuredInt battleboss_count;
        public readonly ObscuredInt player_count;               // 등장 더미 캐릭터 수량

        public readonly ObscuredInt scenario_maze_id;           // 매칭 시나리오 미로 id
        public readonly ObscuredInt mvp_group_id;               // 등장 MVP 몬스터 그룹 아이디
        public readonly ObscuredInt mvp_level;                  // 등장 MVP 레벨
        public readonly ObscuredInt mpv_first_reward_id;        // MVP 동료 소환 유저 처치 보상(아이템)
        public readonly ObscuredInt mvp_reward_id_1;            // MVP 처치 보상1(아이템)
        public readonly ObscuredInt mvp_reward_id_2;            // MVP 처치 보상2(아이템)
        public readonly ObscuredInt mvp_reward_id_3;            // MVP 처치 보상3(아이템)
        public readonly ObscuredString bgm;                     // 배경 음악

        public readonly ObscuredInt agent_trade_time_rating1;
        public readonly ObscuredInt agent_trade_time_rating2;
        public readonly ObscuredInt agent_trade_time_rating3;
        public readonly ObscuredInt agent_trade_time_rating4;
        public readonly ObscuredInt agent_trade_time_rating5;
        public readonly ObscuredInt agent_trade_ropoint;
        public readonly ObscuredInt agent_collect_time;
        public readonly ObscuredInt agent_collect_item;
        public readonly ObscuredInt agent_collect_count_rating1;
        public readonly ObscuredInt agent_collect_count_rating2;
        public readonly ObscuredInt agent_collect_count_rating3;
        public readonly ObscuredInt agent_collect_count_rating4;
        public readonly ObscuredInt agent_collect_count_rating5;
        public readonly ObscuredInt agent_dig_time;
        public readonly ObscuredInt agent_dig_item;
        public readonly ObscuredInt agent_dig_count_rating1;
        public readonly ObscuredInt agent_dig_count_rating2;
        public readonly ObscuredInt agent_dig_count_rating3;
        public readonly ObscuredInt agent_dig_count_rating4;
        public readonly ObscuredInt agent_dig_count_rating5;
        public readonly ObscuredInt size_x_min;
        public readonly ObscuredInt size_x_max;
        public readonly ObscuredInt size_z_min;
        public readonly ObscuredInt size_z_max;
        public readonly ObscuredByte agent_explore_type; /// <summary><see cref="ExploreType"/></summary>
        public readonly ObscuredInt agent_production_time_rating1;
        public readonly ObscuredInt agent_production_time_rating2;
        public readonly ObscuredInt agent_production_time_rating3;
        public readonly ObscuredInt agent_production_time_rating4;
        public readonly ObscuredInt agent_production_time_rating5;
        public readonly ObscuredInt agent_production_reward_type;
        public readonly ObscuredInt agent_production_reward_value;
        public readonly ObscuredInt agent_production_reward_count;

        public readonly ObscuredInt challenge_type;
        public readonly ObscuredInt challenge_boss_drop;
        public readonly ObscuredInt challenge_rank_value;
        public readonly ObscuredInt challenge_rank_increase_value;
        public readonly ObscuredInt challenge_return_stage;

        int INormalMonsterSpawnData.MaxIndex => NORMAL_MONSTER_MAX_INDEX;
        int INormalMonsterSpawnData.Cost => normal_monster_cost;
        int INormalMonsterSpawnData.Level => normal_monster_level;

        int IStageInfoPopupData.MvpMonsterGroupId => mvp_group_id;
        int IStageInfoPopupData.MvpMonsterLevel => mvp_level;
        int IStageInfoPopupData.MaxMvpMonsterRewardIndex => MVP_DROP_ITEM_COUNT;

        int IStageInfoPopupData.MaxNormalMonsterIndex => NORMAL_MONSTER_MAX_INDEX;
        int IStageInfoPopupData.NormalMonsterLevel => normal_monster_level;
        int IStageInfoPopupData.MaxNormalMonsterRewardIndex => NORMAL_DROP_ITEM_COUNT;

        int IBossMonsterSpawnData.BossMonsterId => boss_monster_id;
        int IBossMonsterSpawnData.Level => boss_monster_level;
        float IBossMonsterSpawnData.Scale => MathUtils.ToPercentValue(boss_monster_scale);

        public int Chapter => chapter;

        public StageData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id                            = data[index++].AsInt32();
            chapter                       = data[index++].AsInt32();
            name_id                       = data[index++].AsInt32();
            scene_name                    = data[index++].AsString();
            stage_group                   = data[index++].AsInt32();
            stage_group_name_id           = data[index++].AsInt32();
            boss_monster_scale            = data[index++].AsInt32();
            boss_monster_level            = data[index++].AsInt32();
            boss_monster_id               = data[index++].AsInt32();
            boss_base_exp                 = data[index++].AsInt32();
            boss_job_exp                  = data[index++].AsInt32();
            boss_zeny                     = data[index++].AsInt32();
            boss_drop_rate_1              = data[index++].AsInt32();
            boss_drop_rate_2              = data[index++].AsInt32();
            boss_drop_rate_3              = data[index++].AsInt32();
            boss_drop_rate_4              = data[index++].AsInt32();
            boss_drop_1                   = data[index++].AsInt32();
            boss_drop_2                   = data[index++].AsInt32();
            boss_drop_3                   = data[index++].AsInt32();
            boss_drop_4                   = data[index++].AsInt32();
            normal_monster_cost           = data[index++].AsInt32();
            normal_monster_level          = data[index++].AsInt32();
            normal_base_exp               = data[index++].AsInt32();
            normal_job_exp                = data[index++].AsInt32();
            normal_zeny                   = data[index++].AsInt32();
            normal_monster_rate_1         = data[index++].AsInt32();
            normal_monster_rate_2         = data[index++].AsInt32();
            normal_monster_rate_3         = data[index++].AsInt32();
            normal_monster_rate_4         = data[index++].AsInt32();
            normal_monster_id_1           = data[index++].AsInt32();
            normal_monster_id_2           = data[index++].AsInt32();
            normal_monster_id_3           = data[index++].AsInt32();
            normal_monster_id_4           = data[index++].AsInt32();
            normal_drop_rate_1            = data[index++].AsInt32();
            normal_drop_rate_2            = data[index++].AsInt32();
            normal_drop_rate_3            = data[index++].AsInt32();
            normal_drop_rate_4            = data[index++].AsInt32();
            normal_drop_1                 = data[index++].AsInt32();
            normal_drop_2                 = data[index++].AsInt32();
            normal_drop_3                 = data[index++].AsInt32();
            normal_drop_4                 = data[index++].AsInt32();
            scene_change_name             = data[index++].AsString();
            icon_name                     = data[index++].AsString();
            maze_map                      = data[index++].AsInt32();
            maze_map_data                 = data[index++].AsInt32();
            maze_monster_type             = data[index++].AsByte();
            battleboss_count              = data[index++].AsInt32();
            player_count                  = data[index++].AsInt32();
            scenario_maze_id              = data[index++].AsInt32();
            mvp_group_id                  = data[index++].AsInt32();
            mvp_level                     = data[index++].AsInt32();
            mpv_first_reward_id           = data[index++].AsInt32();
            mvp_reward_id_1               = data[index++].AsInt32();
            mvp_reward_id_2               = data[index++].AsInt32();
            mvp_reward_id_3               = data[index++].AsInt32();
            bgm                           = data[index++].AsString();
            agent_trade_time_rating1      = data[index++].AsInt32();
            agent_trade_time_rating2      = data[index++].AsInt32();
            agent_trade_time_rating3      = data[index++].AsInt32();
            agent_trade_time_rating4      = data[index++].AsInt32();
            agent_trade_time_rating5      = data[index++].AsInt32();
            agent_trade_ropoint           = data[index++].AsInt32();
            agent_collect_time            = data[index++].AsInt32();
            agent_collect_item            = data[index++].AsInt32();
            agent_collect_count_rating1   = data[index++].AsInt32();
            agent_collect_count_rating2   = data[index++].AsInt32();
            agent_collect_count_rating3   = data[index++].AsInt32();
            agent_collect_count_rating4   = data[index++].AsInt32();
            agent_collect_count_rating5   = data[index++].AsInt32();
            agent_dig_time                = data[index++].AsInt32();
            agent_dig_item                = data[index++].AsInt32();
            agent_dig_count_rating1       = data[index++].AsInt32();
            agent_dig_count_rating2       = data[index++].AsInt32();
            agent_dig_count_rating3       = data[index++].AsInt32();
            agent_dig_count_rating4       = data[index++].AsInt32();
            agent_dig_count_rating5       = data[index++].AsInt32();
            size_x_min                    = data[index++].AsInt32();
            size_x_max                    = data[index++].AsInt32();
            size_z_min                    = data[index++].AsInt32();
            size_z_max                    = data[index++].AsInt32();
            agent_explore_type            = data[index++].AsByte();
            agent_production_time_rating1 = data[index++].AsInt32();
            agent_production_time_rating2 = data[index++].AsInt32();
            agent_production_time_rating3 = data[index++].AsInt32();
            agent_production_time_rating4 = data[index++].AsInt32();
            agent_production_time_rating5 = data[index++].AsInt32();
            agent_production_reward_type  = data[index++].AsInt32();
            agent_production_reward_value = data[index++].AsInt32();
            agent_production_reward_count = data[index++].AsInt32();
            challenge_type                = data[index++].AsInt32();
            challenge_boss_drop           = data[index++].AsInt32();
            challenge_rank_value          = data[index++].AsInt32();
            challenge_rank_increase_value = data[index++].AsInt32();
            challenge_return_stage        = data[index++].AsInt32();
        }

        /// <summary>
        /// 일반 몬스터 인덱스에 해당하는 소환 정보
        /// </summary>
        (int monsterId, int spawnRate) INormalMonsterSpawnData.GetSpawnInfo(int index)
        {
            switch (index)
            {
                case 0: return (normal_monster_id_1, normal_monster_rate_1);
                case 1: return (normal_monster_id_2, normal_monster_rate_2);
                case 2: return (normal_monster_id_3, normal_monster_rate_3);
                case 3: return (normal_monster_id_4, normal_monster_rate_4);

                default: throw new System.ArgumentException($"잘못된 인덱스: index = {index}");
            }
        }

        int IStageInfoPopupData.GetMvpMonsterRewardId(int index)
        {
            switch (index)
            {
                case 0: return mpv_first_reward_id;
                case 1: return mvp_reward_id_1;
                case 2: return mvp_reward_id_2;
                case 3: return mvp_reward_id_3;

                default: throw new System.ArgumentException($"잘못된 인덱스: index = {index}");
            }
        }

        int IStageInfoPopupData.GetNormalMonsterId(int index)
        {
            switch (index)
            {
                case 0: return normal_monster_id_1;
                case 1: return normal_monster_id_2;
                case 2: return normal_monster_id_3;
                case 3: return normal_monster_id_4;

                default: throw new System.ArgumentException($"잘못된 인덱스: index = {index}");
            }
        }

        int IStageInfoPopupData.GetNormalMonsterRewardId(int index)
        {
            switch (index)
            {
                case 0: return normal_drop_1;
                case 1: return normal_drop_2;
                case 2: return normal_drop_3;
                case 3: return normal_drop_4;

                default: throw new System.ArgumentException($"잘못된 인덱스: index = {index}");
            }
        }

        public int GetExploreRequiredTime(int rating = 1)
        {
            var type = agent_explore_type.ToEnum<ExploreType>();

            if (type == ExploreType.Trade)
            {
                if (rating == 1)
                    return agent_trade_time_rating1 / 1000;
                else if (rating == 2)
                    return agent_trade_time_rating2 / 1000;
                else if (rating == 3)
                    return agent_trade_time_rating3 / 1000;
                else if (rating == 4)
                    return agent_trade_time_rating4 / 1000;
                else if (rating == 5)
                    return agent_trade_time_rating5 / 1000;
            }
            else if (type == ExploreType.Collect)
            {
                return agent_collect_time / 1000;
            }
            else if (type == ExploreType.Dig)
            {
                return agent_dig_time / 1000;
            }
            else if (type == ExploreType.Production)
            {
                if (rating == 1)
                    return agent_production_time_rating1 / 1000;
                else if (rating == 2)
                    return agent_production_time_rating2 / 1000;
                else if (rating == 3)
                    return agent_production_time_rating3 / 1000;
                else if (rating == 4)
                    return agent_production_time_rating4 / 1000;
                else if (rating == 5)
                    return agent_production_time_rating5 / 1000;
            }

            return 0;
        }

        public int GetExploreRewardCountRating(int ratingIndex)
        {
            var type = agent_explore_type.ToEnum<ExploreType>();

            if (type == ExploreType.Collect)
            {
                if (ratingIndex == 1)
                    return agent_collect_count_rating1;
                else if (ratingIndex == 2)
                    return agent_collect_count_rating2;
                else if (ratingIndex == 3)
                    return agent_collect_count_rating3;
                else if (ratingIndex == 4)
                    return agent_collect_count_rating4;
                else if (ratingIndex == 5)
                    return agent_collect_count_rating5;
            }
            else if (type == ExploreType.Dig)
            {
                if (ratingIndex == 1)
                    return agent_dig_count_rating1;
                else if (ratingIndex == 2)
                    return agent_dig_count_rating2;
                else if (ratingIndex == 3)
                    return agent_dig_count_rating3;
                else if (ratingIndex == 4)
                    return agent_dig_count_rating4;
                else if (ratingIndex == 5)
                    return agent_dig_count_rating5;
            }

            return 0;
        }

        public RewardInfo GetExploreReward()
        {
            var type = agent_explore_type.ToEnum<ExploreType>();

            if (type == ExploreType.Trade)
            {
                return new RewardInfo(RewardType.ROPoint, agent_trade_ropoint, 0);
            }
            else if (type == ExploreType.Collect)
            {
                return new RewardInfo(RewardType.Item, agent_collect_item, 1);
            }
            else if (type == ExploreType.Dig)
            {
                return new RewardInfo(RewardType.Item, agent_dig_item, 1);
            }
            else if (type == ExploreType.Production)
            {
                return new RewardInfo((byte)agent_production_reward_type, agent_production_reward_value, agent_production_reward_count);
            }

            return null;
        }

        public int GetMonsterId()
        {
            return boss_monster_id;
        }

        public string GetDescription()
        {
            return LocalizeKey._2801.ToText(); // 보스 도전!
        }

        public string GetSpriteName()
        {
            return "Ui_Common_Icon_Boss4";
        }
    }
}