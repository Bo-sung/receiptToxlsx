using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="ForestBaseDataManager"/>
    /// </summary>
    public sealed class ForestBaseData : IData, IOpenConditional
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt group_id;
        public readonly ObscuredInt floor;
        public readonly ObscuredInt name_id;
        public readonly ObscuredString scene_name;
        public readonly ObscuredString bgm;
        public readonly ObscuredString boss_battle_scene_name;
        public readonly ObscuredString boss_bgm;
        public readonly ObscuredInt size_x;
        public readonly ObscuredInt size_y;
        public readonly ObscuredString multi_maze_data;
        public readonly ObscuredInt max_user;
        public readonly ObscuredInt monster_group;
        public readonly ObscuredInt potion_count;
        public readonly ObscuredInt emperium_count;
        public readonly ObscuredInt open_job_level;

        DungeonOpenConditionType IOpenConditional.ConditionType => GetConditionType();
        int IOpenConditional.ConditionValue => open_job_level;

        public ForestBaseData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id                     = data[index++].AsInt32();
            group_id               = data[index++].AsInt32();
            floor                  = data[index++].AsInt32();
            name_id                = data[index++].AsInt32();
            scene_name             = data[index++].AsString();
            bgm                    = data[index++].AsString();
            boss_battle_scene_name = data[index++].AsString();
            boss_bgm               = data[index++].AsString();
            size_x                 = data[index++].AsInt32();
            size_y                 = data[index++].AsInt32();
            multi_maze_data        = data[index++].AsString();
            max_user               = data[index++].AsInt32();
            monster_group          = data[index++].AsInt32();
            potion_count           = data[index++].AsInt32();
            emperium_count         = data[index++].AsInt32();
            open_job_level         = data[index++].AsInt32();
        }

        private DungeonOpenConditionType GetConditionType()
        {
            if (BasisOpenContetsType.ForestMaze.IsOpend())
                return DungeonOpenConditionType.JobLevel;

            return DungeonOpenConditionType.UpdateLater;
        }
    }
}