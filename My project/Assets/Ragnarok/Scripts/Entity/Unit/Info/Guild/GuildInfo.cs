using CodeStage.AntiCheat.ObscuredTypes;
using System;

namespace Ragnarok
{
    public class GuildInfo : IInfo, IInitializable<GuildPacket>
    {
        bool IInfo.IsInvalidData => false;

        event Action IInfo.OnUpdateEvent
        {
            add { }
            remove { }
        }

        private ObscuredInt guildId;
        private ObscuredString guildName;
        private ObscuredInt emblemBg;
        private ObscuredInt emblemframe;
        private ObscuredInt emblemIcon;
        private ObscuredByte guildPosition;
        private ObscuredInt guildCoin;
        private ObscuredInt guildQuestRewardCount;

        public int GuildId => guildId;

        public void Initialize(GuildPacket packet)
        {
            guildId = packet.guild_id;
            guildName = packet.guild_name;
            emblemBg = MathUtils.GetBitFieldValue(packet.guild_emblem, 0);
            emblemframe = MathUtils.GetBitFieldValue(packet.guild_emblem, 6);
            emblemIcon = MathUtils.GetBitFieldValue(packet.guild_emblem, 12);
            guildPosition = packet.guild_position;
            guildCoin = packet.guild_coin;
            guildQuestRewardCount = packet.guild_quest_reward_cnt;
        }
    }
}