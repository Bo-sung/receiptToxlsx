using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="ProfileDataManager"/>
    /// </summary>
    public sealed class ProfileData : IData, ProfileSelectElement.IInput
    {
        private readonly ObscuredInt id;
        public readonly int sort;
        private readonly string texture_name;
        private readonly ObscuredInt condition_value;

        public int Id => id;
        public int NeedMileage => condition_value;

        /// <summary>
        /// 프로필 (원형)
        /// </summary>
        public string ThumbnailName => texture_name;
        /// <summary>
        /// 프로필 (네모)
        /// </summary>
        public string ProfileName => string.Concat("Info_", ThumbnailName);
        /// <summary>
        /// 프로필 (육각)
        /// </summary>
        public string AgentProfileName => string.Concat("Agent_", ThumbnailName);

        public ProfileData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id                 = data[index++].AsInt32();
            int gender         = data[index++].AsInt32();
            sort               = data[index++].AsInt32();
            texture_name       = data[index++].AsString();
            int condition_type = data[index++].AsInt32();
            condition_value    = data[index++].AsInt32();
        }
    }
}