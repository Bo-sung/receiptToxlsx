using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    public class DungeonInfoData : IData, ForestMazeView.IInput
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt title_name_id;
        public readonly ObscuredString icon_name_1;
        public readonly ObscuredInt name_id_1;
        public readonly ObscuredInt des_id_1;
        public readonly ObscuredString icon_name_2;
        public readonly ObscuredInt name_id_2;
        public readonly ObscuredInt des_id_2;
        public readonly ObscuredString icon_name_3;
        public readonly ObscuredInt name_id_3;
        public readonly ObscuredInt des_id_3;

        public DungeonInfoData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id              = data[index++].AsInt32();
            title_name_id   = data[index++].AsInt32();
            icon_name_1     = data[index++].AsString();
            name_id_1       = data[index++].AsInt32();
            des_id_1        = data[index++].AsInt32();
            icon_name_2     = data[index++].AsString();
            name_id_2       = data[index++].AsInt32();
            des_id_2        = data[index++].AsInt32();
            icon_name_3     = data[index++].AsString();
            name_id_3       = data[index++].AsInt32();
            des_id_3        = data[index++].AsInt32();
        }

        int ForestMazeView.IInput.GetNameKey(int index)
        {
            switch (index)
            {
                case 0: return name_id_1;
                case 1: return name_id_2;
                case 2: return name_id_3;
            }

            throw new System.ArgumentException($"유효하지 않은 처리: {nameof(index)} = {index}");
        }

        int ForestMazeView.IInput.GetDescKey(int index)
        {
            switch (index)
            {
                case 0: return des_id_1;
                case 1: return des_id_2;
                case 2: return des_id_3;
            }

            throw new System.ArgumentException($"유효하지 않은 처리: {nameof(index)} = {index}");
        }
    }
}