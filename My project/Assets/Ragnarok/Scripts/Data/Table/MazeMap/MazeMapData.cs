using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{

    /// <summary>
    /// <see cref="MazeMapDataManager"/>
    /// </summary>
    public class MazeMapData : IData, IMazeMapSlotData
    {
        public readonly ObscuredInt id;
        [System.Obsolete]
        public readonly ObscuredInt size_x;
        [System.Obsolete]
        public readonly ObscuredInt size_y;
        [System.Obsolete]
        public readonly ObscuredString mapdata;
        public readonly ObscuredInt random_path1_point_count;
        public readonly ObscuredInt random_path2_point_count;
        public readonly ObscuredInt start_pos_x;
        public readonly ObscuredInt start_pos_z;
        public readonly ObscuredString scene_name;
        public readonly ObscuredInt maz_user;
        public readonly ObscuredInt name_id;
        public readonly ObscuredInt cube_reward;
        public readonly ObscuredInt first_clear_reward;
        public readonly ObscuredInt random_treasure_point_count;

        public MazeMapData(IList<MessagePackObject> data)
        {
            byte index                  = 0;
            id                          = data[index++].AsInt32();
            size_x                      = data[index++].AsInt32();
            size_y                      = data[index++].AsInt32();
            mapdata                     = data[index++].AsString();
            random_path1_point_count    = data[index++].AsInt32();
            random_path2_point_count    = data[index++].AsInt32();
            start_pos_x                 = data[index++].AsInt32();
            start_pos_z                 = data[index++].AsInt32();
            scene_name                  = data[index++].AsString();
            maz_user                    = data[index++].AsInt32();
            name_id                     = data[index++].AsInt32();
            cube_reward                 = data[index++].AsInt32();
            first_clear_reward          = data[index++].AsInt32();
            random_treasure_point_count = data[index++].AsInt32();
        }

        int IMazeMapSlotData.MazeMapId => id;

        int IMazeMapSlotData.MazeMapNameId => name_id;       
    } 
}
