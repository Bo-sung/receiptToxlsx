using CodeStage.AntiCheat.ObscuredTypes;
using System;

namespace Ragnarok
{
    public class GuildJoinSubmitInfo : IInfo, IInitializable<GuildJoinSubmitPacket>
    {
        bool IInfo.IsInvalidData => false;

        event Action IInfo.OnUpdateEvent
        {
            add { }
            remove { }
        }

        private ObscuredInt seq;
        private ObscuredInt uid;
        private ObscuredInt cid;
        private ObscuredString name;
        private ObscuredByte job;
        private ObscuredByte gender;
        private ObscuredShort jobLevel;
        private ObscuredString hexCid;
        private DateTime insertDate;
        

        public int Seq => seq;
        public Job Job => job.ToEnum<Job>();
        public string Name => name;
        public string Id => hexCid;
        public int Level => jobLevel;
        public DateTime Date => insertDate;

        public void Initialize(GuildJoinSubmitPacket packet)
        {
            seq = packet.seq;
            uid = packet.uid;
            cid = packet.cid;
            name = packet.name;
            job = packet.job;
            gender = packet.gender;
            jobLevel = packet.job_level;
            hexCid = packet.hex_cid;
            insertDate = packet.insert_dt.ToDateTime();
        }
    }
}
