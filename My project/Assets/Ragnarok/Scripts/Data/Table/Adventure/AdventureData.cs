using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="AdventureDataManager"/>
    /// </summary>
    public sealed class AdventureData : IData
    {
        private const int CHAPTER_LINK_TYPE = 1;
        private const int STAGE_LINK_TYPE = 2;

        public readonly ObscuredInt id;
        public readonly ObscuredInt name_id;
        public readonly ObscuredInt link_type; // 1 : 스테이지 챕터, 2 : 스테이지
        public readonly ObscuredInt link_value;
        public readonly ObscuredInt chapter;
        public readonly string icon_name;
        /// <summary>
        /// <see cref="AdventureGroup"/>
        /// </summary>
        public readonly int scenario_id; // group 분기 (1: chapter 1~12, 2: chapter 13~)

        public AdventureData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id = data[index++].AsInt32();
            name_id = data[index++].AsInt32();
            link_type = data[index++].AsInt32();
            link_value = data[index++].AsInt32();
            chapter = data[index++].AsInt32();
            icon_name = data[index++].AsString();
            string map_icon = data[index++].AsString();
            string road_icon = data[index++].AsString();
            scenario_id = data[index++].AsInt32();
        }

        public bool IsChapterData()
        {
            return link_type == CHAPTER_LINK_TYPE;
        }

        public bool IsStageData()
        {
            return link_type == STAGE_LINK_TYPE;
        }
    }
}