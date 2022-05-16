using CodeStage.AntiCheat.ObscuredTypes;
using System;

namespace Ragnarok
{
    public class GuildMemberInfo : IInfo
    {
        bool IInfo.IsInvalidData => false;

        private Action onUpdateEvent;

        event Action IInfo.OnUpdateEvent
        {
            add { onUpdateEvent += value; }
            remove { onUpdateEvent -= value; }
        }
        protected void InvokeEvent()
        {
            onUpdateEvent?.Invoke();
        }

        private ObscuredInt uid;
        private ObscuredInt cid;
        private ObscuredString name;
        private ObscuredByte job;
        private ObscuredByte gender;
        private ObscuredShort jobLevel;
        private ObscuredInt emblem;
        private ObscuredByte position;
        private ObscuredInt remainPoint;
        private ObscuredInt donatePoint;
        private ObscuredByte connect;
        private ObscuredBool attend;
        private ObscuredString hexCid;
        private RemainTime guildOutRemainTime;
        private int profileId;

        public int UID => uid;
        public int CID => cid;
        public string ID => hexCid;
        public Job Job => job.ToEnum<Job>();
        public Gender Gender => gender.ToEnum<Gender>();
        public string Name => name;
        public int Level => jobLevel;
        public GuildPosition GuildPosition => position.ToEnum<GuildPosition>();
        public int DonatePoint => donatePoint;
        public bool IsOnline => connect != 0;
        public CumulativeTime OfflineTime;
        public int JobLevel => jobLevel;
        public RemainTime GuildOutRemainTime => guildOutRemainTime;
        public string ProfileName { get; private set; }
        public int BattleScore { get; private set; }

        public void Initialize(GuildMemberPacket packet)
        {
            uid = packet.uid;
            cid = packet.cid;
            name = packet.name;
            job = packet.job;
            gender = packet.gender;
            jobLevel = packet.job_level;
            emblem = packet.guild_emblem;
            position = packet.guild_position;
            remainPoint = packet.guild_remain_point;
            donatePoint = packet.guild_donate_point;
            connect = packet.connect;
            attend = packet.attend;
            OfflineTime = packet.login_time;
            hexCid = packet.hex_cid;
            guildOutRemainTime = packet.guild_out_remain_time;
            profileId = packet.profileId;
            BattleScore = packet.batlteScore;
        }

        public void Initialize(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl)
        {
            ProfileName = GetProfileName(profileDataRepoImpl);
        }

        public void SetGuildPosition(byte position)
        {
            this.position = position;
            InvokeEvent();
        }

        private string GetProfileName(ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl)
        {
            if (profileId > 0)
            {
                ProfileData profileData = profileDataRepoImpl.Get(profileId);
                if (profileData != null)
                    return profileData.ProfileName;
            }

            return Job.GetJobProfile(Gender);
        }
    }
}
