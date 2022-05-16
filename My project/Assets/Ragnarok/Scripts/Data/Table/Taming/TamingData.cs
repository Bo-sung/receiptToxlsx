using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="TamingDataManager"/>
    /// </summary>
    public sealed class TamingData : IData
    {
        public const int MAX_SPAWN_MONSTER_INDEX = 6;

        public readonly ObscuredInt id;
        public readonly ObscuredInt day_type;
        public readonly ObscuredInt rotation_value;
        public readonly ObscuredInt name_id;
        public readonly ObscuredString scene_name;
        public readonly ObscuredInt max_monster;
        public readonly ObscuredInt respwan_time;
        public readonly ObscuredInt use_item_id;
        public readonly ObscuredInt monster_level;
        public readonly ObscuredInt spwan_rate_1;
        public readonly ObscuredInt spwan_rate_2;
        public readonly ObscuredInt spwan_rate_3;
        public readonly ObscuredInt spwan_rate_4;
        public readonly ObscuredInt spwan_rate_5;
        public readonly ObscuredInt spwan_rate_6;
        public readonly ObscuredInt spwan_monster_1;
        public readonly ObscuredInt spwan_monster_2;
        public readonly ObscuredInt spwan_monster_3;
        public readonly ObscuredInt spwan_monster_4;
        public readonly ObscuredInt spwan_monster_5;
        public readonly ObscuredInt spwan_monster_6;
        public readonly ObscuredInt drop_item_1;
        public readonly ObscuredInt drop_item_2;
        public readonly ObscuredInt drop_item_3;
        public readonly ObscuredInt drop_item_4;
        public readonly ObscuredInt drop_item_5;
        public readonly ObscuredInt drop_item_6;
        public readonly ObscuredInt start_pos_x;
        public readonly ObscuredInt start_pos_z;
        public readonly ObscuredInt spawn_point_count;
        public readonly ObscuredInt fail_rate;
        public readonly ObscuredInt success_rate;
        public readonly ObscuredInt great_success_rate;
        public readonly ObscuredString prefab_name;
        public readonly ObscuredString bgm;

        public TamingData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id                  = data[index++].AsInt32();
            day_type            = data[index++].AsInt32();
            rotation_value      = data[index++].AsInt32();
            name_id             = data[index++].AsInt32();
            scene_name          = data[index++].AsString();
            max_monster         = data[index++].AsInt32();
            respwan_time        = data[index++].AsInt32();
            use_item_id         = data[index++].AsInt32();
            monster_level       = data[index++].AsInt32();
            spwan_rate_1        = data[index++].AsInt32();
            spwan_rate_2        = data[index++].AsInt32();
            spwan_rate_3        = data[index++].AsInt32();
            spwan_rate_4        = data[index++].AsInt32();
            spwan_rate_5        = data[index++].AsInt32();
            spwan_rate_6        = data[index++].AsInt32();
            spwan_monster_1     = data[index++].AsInt32();
            spwan_monster_2     = data[index++].AsInt32();
            spwan_monster_3     = data[index++].AsInt32();
            spwan_monster_4     = data[index++].AsInt32();
            spwan_monster_5     = data[index++].AsInt32();
            spwan_monster_6     = data[index++].AsInt32();            
            drop_item_1         = data[index++].AsInt32();
            drop_item_2         = data[index++].AsInt32();
            drop_item_3         = data[index++].AsInt32();
            drop_item_4         = data[index++].AsInt32();
            drop_item_5         = data[index++].AsInt32();
            drop_item_6         = data[index++].AsInt32();
            start_pos_x         = data[index++].AsInt32();
            start_pos_z         = data[index++].AsInt32();
            spawn_point_count   = data[index++].AsInt32();
            fail_rate           = data[index++].AsInt32();
            success_rate        = data[index++].AsInt32();
            great_success_rate  = data[index++].AsInt32();
            prefab_name         = data[index++].AsString();
            bgm                 = data[index++].AsString();
        }

        public int GetSpawnMonsterId(int index)
        {
            switch (index)
            {
                case 0: return spwan_monster_1;
                case 1: return spwan_monster_2;
                case 2: return spwan_monster_3;
                case 3: return spwan_monster_4;
                case 4: return spwan_monster_5;
                case 5: return spwan_monster_6;
            }

            throw new System.ArgumentException($"유효하지 않은 처리: {nameof(index)} = {index}");
        }
    }
}