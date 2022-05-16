using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="GuildAttackMonsterDataManager"/>
    /// </summary>
    public sealed class GuildAttackMonsterData : IData
    {
        public readonly int id;
        public readonly int emperium_lv;
        public readonly int monster_id;
        public readonly int monster_type;
        public readonly int monster_level;
        public readonly int monster_scale;
        public readonly int spawn_time_min;
        public readonly int spawn_time_max;
        public readonly int spawn_delay;
        public readonly int pos_x;
        public readonly int pos_y;
        public readonly int group_id;

        public GuildAttackMonsterData(IList<MessagePackObject> data)
        {
            int index = 0;
            id = data[index++].AsInt32();
            emperium_lv = data[index++].AsInt32();
            monster_id = data[index++].AsInt32();
            monster_type = data[index++].AsInt32();
            monster_level = data[index++].AsInt32();
            monster_scale = data[index++].AsInt32();
            spawn_time_min = data[index++].AsInt32();
            spawn_time_max = data[index++].AsInt32();
            spawn_delay = data[index++].AsInt32();
            pos_x = data[index++].AsInt32();
            pos_y = data[index++].AsInt32();
            group_id = data[index++].AsInt32();
        }
    }
}