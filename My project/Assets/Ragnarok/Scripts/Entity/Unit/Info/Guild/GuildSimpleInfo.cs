using CodeStage.AntiCheat.ObscuredTypes;
using System;

namespace Ragnarok
{
    public class GuildSimpleInfo : IInfo, IInitializable<GuildSimplePacket>
    {
        bool IInfo.IsInvalidData => false;

        event Action IInfo.OnUpdateEvent
        {
            add { }
            remove { }
        }

        private ObscuredInt seq;
        private ObscuredInt guildId;
        private ObscuredString guildName;
        private ObscuredInt emblemBg;
        private ObscuredInt emblemframe;
        private ObscuredInt emblemIcon;
        private ObscuredString masterName;
        private ObscuredByte memberCount;
        private ObscuredString introduction;
        private ObscuredByte level;
        private ObscuredInt expPoint;
        private ObscuredInt maxMemberCount;
        private ObscuredString hexCid; // 길드 마스터
        private ObscuredBool isSubmitJoin; // 길드 가입 신청 여부
        private ObscuredByte isAutoJoin;

        public int Seq => seq;
        public int GuildId => guildId;
        public string EmblemBgName => $"background_{emblemBg}";
        public string EmblemframeName => $"frame_{emblemframe}";
        public string EmblemIconName => $"icon_{emblemIcon}";
        public int Level => level;
        public string Name => guildName;
        public string MasterName => masterName;
        public string MemeberCountName => $"{memberCount}/{maxMemberCount}";
        public bool IsMaxMember => memberCount == maxMemberCount;
        /// <summary>
        /// 길드 가입 신청 여부
        /// </summary>
        public bool IsSubmitJoin => isSubmitJoin;

        /// <summary>
        /// 자유가입 여부
        /// </summary>
        public bool IsAutoJoin => isAutoJoin == 1;

        public void Initialize(GuildSimplePacket packet)
        {
            guildId = packet.guild_id;
            guildName = packet.name;
            emblemBg = MathUtils.GetBitFieldValue(packet.emblem, 0);
            emblemframe = MathUtils.GetBitFieldValue(packet.emblem, 6);
            emblemIcon = MathUtils.GetBitFieldValue(packet.emblem, 12);
            masterName = packet.master_name;
            memberCount = packet.member_count;
            introduction = packet.introduction;
            level = packet.level;
            expPoint = packet.exppoint;
            maxMemberCount = packet.max_member_count;
            hexCid = packet.hex_cid;
            isAutoJoin = packet.is_Auto_Join;
        }

        public void Initialize(GuildRequestSimplePacket packet)
        {
            seq = packet.seq;
            guildId = packet.guild_id;
            guildName = packet.guild_name;
            emblemBg = MathUtils.GetBitFieldValue(packet.emblem, 0);
            emblemframe = MathUtils.GetBitFieldValue(packet.emblem, 6);
            emblemIcon = MathUtils.GetBitFieldValue(packet.emblem, 12);
            masterName = packet.master_name;
            level = packet.guild_level;
        }

        public void SetSubmitJoin(bool value)
        {
            isSubmitJoin = value;
        }
    }
}