using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="MultiMazeDataManager"/>
    /// </summary>
    public sealed class MultiMazeData : IData, IBossMonsterSpawnData, UIBossComing.IInput
    {
        public enum MultiMazeType
        {
            Normal,
            Match,
        }

        private const int MULTI_MAZE_TYPE = 4;
        private const int DARK_MAZE_TYPE = 6;

        public readonly ObscuredInt id;
        public readonly ObscuredInt open_scenario_id;
        public readonly ObscuredString scene_name;
        public readonly ObscuredString bgm;
        public readonly ObscuredInt boss_battle_condition;
        public readonly ObscuredInt boss_monster_scale;
        public readonly ObscuredInt boss_monster_level;
        public readonly ObscuredInt boss_monster_id;
        public readonly ObscuredInt normal_monster_level;
        public readonly ObscuredInt normal_monster_id;
        public readonly ObscuredInt normal_monster_count;
        public readonly ObscuredInt reward_type1;
        public readonly ObscuredInt reward_value1;
        public readonly ObscuredInt reward_count1;
        public readonly ObscuredInt reward_type2;
        public readonly ObscuredInt reward_value2;
        public readonly ObscuredInt reward_count2;
        public readonly ObscuredInt reward_type3;
        public readonly ObscuredInt reward_value3;
        public readonly ObscuredInt reward_count3;
        public readonly ObscuredInt reward_type4;
        public readonly ObscuredInt reward_value4;
        public readonly ObscuredInt reward_count4;
        public readonly ObscuredInt size_x;
        public readonly ObscuredInt size_y;
        public readonly ObscuredInt multi_maze_type;
        public readonly ObscuredString multi_maze_data;
        public readonly ObscuredInt max_user;
        public readonly ObscuredInt zeny_max_count;
        public readonly ObscuredInt zeny_value;
        public readonly ObscuredInt radom_item_max_count;
        public readonly ObscuredString boss_battle_scene_name;
        public readonly ObscuredString boss_battle_bgm;
        public readonly ObscuredInt character_speed;
        public readonly ObscuredInt boss_monster_speed;
        public readonly ObscuredInt normal_monster_speed;
        public readonly ObscuredInt boss_monster_respawn;
        public readonly ObscuredInt normal_monster_respawn;
        public readonly ObscuredInt random_item_type1;
        public readonly ObscuredInt random_item_type2;
        public readonly ObscuredInt random_item_type3;
        public readonly ObscuredInt random_item_type4;
        public readonly ObscuredInt random_item_type5;
        public readonly ObscuredInt random_item_type6;
        public readonly ObscuredInt random_item_type7;
        public readonly ObscuredInt random_item_type8;
        public readonly ObscuredInt random_item_type9;
        public readonly ObscuredInt random_item_type10;
        public readonly ObscuredInt name_id;
        public readonly ObscuredInt chapter;
        public readonly ObscuredInt difficulty;
        public readonly ObscuredInt waiting_room_id;
        public readonly ObscuredInt waiting_room_spawn_x;
        public readonly ObscuredInt waiting_room_spawn_y;
        public readonly ObscuredInt waiting_room_spawn_z;
        public readonly ObscuredInt waiting_room_index;
        public readonly ObscuredInt battle_score;
        public readonly ObscuredInt dungeon_info_id;

        /// <summary>
        /// 보스 몬스터 소환 아이디
        /// </summary>
        int IBossMonsterSpawnData.BossMonsterId => boss_monster_id;

        /// <summary>
        /// 보스 몬스터 소환 레벨
        /// </summary>
        int IBossMonsterSpawnData.Level => boss_monster_level;

        /// <summary>
        /// 보스 몬스터 소환 크기
        /// </summary>
        float IBossMonsterSpawnData.Scale => GetScale(MonsterType.Boss);

        private int waitingRoomId; // 클라 전용

        public MultiMazeData(IList<MessagePackObject> data)
        {
            int index = 0;
            id                     = data[index++].AsInt32();
            open_scenario_id       = data[index++].AsInt32();
            scene_name             = data[index++].AsString();
            bgm                    = data[index++].AsString();
            boss_battle_condition  = data[index++].AsInt32();
            boss_monster_scale     = data[index++].AsInt32();
            boss_monster_level     = data[index++].AsInt32();
            boss_monster_id        = data[index++].AsInt32();
            normal_monster_level   = data[index++].AsInt32();
            normal_monster_id      = data[index++].AsInt32();
            normal_monster_count   = data[index++].AsInt32();
            reward_type1           = data[index++].AsInt32();
            reward_value1          = data[index++].AsInt32();
            reward_count1          = data[index++].AsInt32();
            reward_type2           = data[index++].AsInt32();
            reward_value2          = data[index++].AsInt32();
            reward_count2          = data[index++].AsInt32();
            reward_type3           = data[index++].AsInt32();
            reward_value3          = data[index++].AsInt32();
            reward_count3          = data[index++].AsInt32();
            reward_type4           = data[index++].AsInt32();
            reward_value4          = data[index++].AsInt32();
            reward_count4          = data[index++].AsInt32();
            size_x                 = data[index++].AsInt32();
            size_y                 = data[index++].AsInt32();
            multi_maze_type        = data[index++].AsInt32();
            multi_maze_data        = data[index++].AsString();
            max_user               = data[index++].AsInt32();
            zeny_max_count         = data[index++].AsInt32();
            zeny_value             = data[index++].AsInt32();
            radom_item_max_count   = data[index++].AsInt32();
            boss_battle_scene_name = data[index++].AsString();
            boss_battle_bgm        = data[index++].AsString();
            character_speed        = data[index++].AsInt32();
            boss_monster_speed     = data[index++].AsInt32();
            normal_monster_speed   = data[index++].AsInt32();
            boss_monster_respawn   = data[index++].AsInt32();
            normal_monster_respawn = data[index++].AsInt32();
            random_item_type1      = data[index++].AsInt32();
            random_item_type2      = data[index++].AsInt32();
            random_item_type3      = data[index++].AsInt32();
            random_item_type4      = data[index++].AsInt32();
            random_item_type5      = data[index++].AsInt32();
            random_item_type6      = data[index++].AsInt32();
            random_item_type7      = data[index++].AsInt32();
            random_item_type8      = data[index++].AsInt32();
            random_item_type9      = data[index++].AsInt32();
            random_item_type10     = data[index++].AsInt32();
            name_id                = data[index++].AsInt32();
            chapter                = data[index++].AsInt32();
            difficulty             = data[index++].AsInt32();
            waiting_room_id        = data[index++].AsInt32();
            waiting_room_spawn_x   = data[index++].AsInt32();
            waiting_room_spawn_y   = data[index++].AsInt32();
            waiting_room_spawn_z   = data[index++].AsInt32();
            waiting_room_index     = data[index++].AsInt32();
            battle_score           = data[index++].AsInt32();
            dungeon_info_id        = data[index++].AsInt32();
        }

        public void SetWaitingRoomId(int waitingRoomId)
        {
            this.waitingRoomId = waitingRoomId;
        }

        public int GetWaitingRoomId()
        {
            // 따로 세팅되어있지 않다면 id 값이 waitingRoomId 와 동일
            if (waitingRoomId == 0)
                return id;

            return waitingRoomId;
        }

        public MazeMode GetMazeMode()
        {
            return multi_maze_type.ToEnum<MazeMode>();
        }

        public MazeMapTuple ToMazeMapTuple()
        {
            return new MazeMapTuple(multi_maze_data, size_x, size_y);
        }

        public int GetLevel(MonsterType type)
        {
            return type == MonsterType.Boss ? boss_monster_level : normal_monster_level;
        }

        public float GetScale(MonsterType type)
        {
            return type == MonsterType.Boss ? MathUtils.ToPercentValue(boss_monster_scale) : 1f;
        }

        public float GetCharacterSpeed()
        {
            return MathUtils.ToPermyriadValue(character_speed);
        }

        public float GetBossMonsterSpeed()
        {
            return MathUtils.ToPermyriadValue(boss_monster_speed);
        }

        public float GetNormalMonsterSpeed()
        {
            return MathUtils.ToPermyriadValue(normal_monster_speed);
        }

        public MultiMazeType GetMultiMazeType()
        {
            if (multi_maze_type == MULTI_MAZE_TYPE || multi_maze_type == DARK_MAZE_TYPE)
                return MultiMazeType.Match;

            return MultiMazeType.Normal;
        }

        public int GetMaxMatchingUser()
        {
            return max_user;
        }

        public int GetMaxQuestCoinCount()
        {
            return max_user * 5;
        }

        public bool IsEvent()
        {
            return chapter == 0;
        }

        int UIBossComing.IInput.GetMonsterId()
        {
            return boss_monster_id;
        }

        string UIBossComing.IInput.GetDescription()
        {
            return LocalizeKey._2802.ToText(); // 몬스터 출현!
        }

        string UIBossComing.IInput.GetSpriteName()
        {
            return "Ui_Common_Icon_Boss4";
        }
    }
}